using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Logout;

public class LogoutCommandHandler(IUnitOfWork unitOfWork, ITokenJwtService tokenService)
    : IRequestHandler<LogoutCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
            return Result.Success;

        var tokenHash = tokenService.HashRefreshToken(command.RefreshToken);
        var session = await unitOfWork.UserSessionRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (session is not null && session.RevokedAtUtc is null)
        {
            session.Revoke("Logout do usuário.");
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success;
    }
}
