using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.Services;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Cobranca.Cidada.Application.Modules.Auth.Services;

public class AuthSessionService(
    IUnitOfWork unitOfWork,
    ITokenJwtService tokenService,
    IVoterKeyService voterKeyService,
    UserManager<AppUser> userManager,
    IConfiguration configuration)
{
    public async Task<LoginResponse> CreateAsync(
        AppUser user,
        SessionMetadata metadata,
        CancellationToken cancellationToken)
    {
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(GetRefreshTokenLifetimeDays());
        var session = UserSession.Create(
            user.Id,
            tokenService.HashRefreshToken(refreshToken),
            metadata.DeviceName,
            metadata.UserAgent,
            metadata.IpAddress,
            refreshExpiresAtUtc);

        await unitOfWork.UserSessionRepository.CreateAsync(session, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await BuildResponseAsync(user, session.FamilyId, refreshToken, refreshExpiresAtUtc);
    }

    public async Task<ErrorOr<LoginResponse>> RefreshAsync(
        string refreshToken,
        SessionMetadata metadata,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(refreshToken);
        var currentSession = await unitOfWork.UserSessionRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (currentSession is null)
            return InvalidSession();

        if (currentSession.RevokedAtUtc is not null)
        {
            if (currentSession.ReplacedBySessionId.HasValue)
            {
                await unitOfWork.UserSessionRepository.RevokeAllAsync(
                    currentSession.UserId,
                    "Reutilização de token de atualização detectada.",
                    exceptSessionId: null,
                    cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }

            return InvalidSession();
        }

        if (currentSession.ExpiresAtUtc <= DateTime.UtcNow)
        {
            currentSession.Revoke("Token de atualização expirado.");
            await unitOfWork.CommitAsync(cancellationToken);
            return InvalidSession();
        }

        var user = await userManager.FindByIdAsync(currentSession.UserId);
        if (user is null || !user.Active)
        {
            currentSession.Revoke("Usuário inexistente ou inativo.");
            await unitOfWork.CommitAsync(cancellationToken);
            return InvalidSession();
        }

        var newRefreshToken = tokenService.CreateRefreshToken();
        var refreshExpiresAtUtc = DateTime.UtcNow.AddDays(GetRefreshTokenLifetimeDays());
        var replacement = UserSession.Create(
            user.Id,
            tokenService.HashRefreshToken(newRefreshToken),
            metadata.DeviceName,
            metadata.UserAgent,
            metadata.IpAddress,
            refreshExpiresAtUtc,
            currentSession.FamilyId);

        currentSession.Touch(metadata.IpAddress);
        currentSession.Rotate(replacement.Id);
        await unitOfWork.UserSessionRepository.CreateAsync(replacement, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return await BuildResponseAsync(user, replacement.FamilyId, newRefreshToken, refreshExpiresAtUtc);
    }

    private async Task<LoginResponse> BuildResponseAsync(
        AppUser user,
        Guid sessionId,
        string refreshToken,
        DateTime refreshExpiresAtUtc)
    {
        var roles = UserRoles.Resolve(user.IsAdmin, await userManager.GetRolesAsync(user));
        var accessToken = tokenService.CreateTokenUser(user, sessionId, voterKeyService.Compute(user.Email!), roles);
        return new LoginResponse(user.Id, user.Email, user.Name, roles, accessToken.Token, accessToken.DateExpiration)
        {
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = refreshExpiresAtUtc
        };
    }

    private int GetRefreshTokenLifetimeDays()
        => int.TryParse(configuration["TokenConfiguration:RefreshTokenExpireDays"], out var days)
            ? Math.Clamp(days, 1, 90)
            : 30;

    private static Error InvalidSession()
        => Error.Unauthorized("Auth.InvalidSession", "Sessão inválida ou expirada.");
}
