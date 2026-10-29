using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Domain.Modules.User;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.GetProfile;

public class GetProfileUseCase : IUseCase
{
    private readonly UserManager<AppUser> _userManager;

    public GetProfileUseCase(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<ProfileResponse>> ExecuteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Error.NotFound("Auth.UserNotFound", "Usuário não encontrado.");

        var roles = UserRoles.Resolve(user.IsAdmin, await _userManager.GetRolesAsync(user));
        var hasPassword = await _userManager.HasPasswordAsync(user);
        var googleLinked = (await _userManager.GetLoginsAsync(user)).Any(l => l.LoginProvider == UserLoginProviders.Google);
        return new ProfileResponse(user.Id, user.Email, user.Name, roles, user.ConsentVersion, user.ConsentAcceptedAtUtc, hasPassword, googleLinked);
    }
}
