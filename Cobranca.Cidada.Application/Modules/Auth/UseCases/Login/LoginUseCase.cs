using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using Microsoft.AspNetCore.Identity;

namespace Cobranca.Cidada.Application.Modules.Auth.UseCases.Login;

/// <summary>
/// Login com senha. Contra tentativa de adivinhar senha e contra enumeração de contas:
/// a resposta de falha é a mesma para e-mail inexistente, conta inativa e senha errada, e o custo do hash
/// também (há um hash fictício quando a conta não existe). O bloqueio é por par IP + conta, para que um terceiro
/// não consiga trancar a conta de outra pessoa, mais um limite alto por conta contra ataques vindos de muitos IPs.
/// O contador acompanha o carimbo de segurança da conta: trocar ou redefinir a senha zera o bloqueio.
/// </summary>
public class LoginUseCase : IUseCase
{
    private const int PairMaxFailures = 5;
    private static readonly TimeSpan PairWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PairBlock = TimeSpan.FromMinutes(15);

    private const int AccountMaxFailures = 30;
    private static readonly TimeSpan AccountWindow = TimeSpan.FromHours(1);
    private static readonly TimeSpan AccountBlock = TimeSpan.FromMinutes(15);

    private static string? _dummyHash;

    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AuthSessionService _authSessionService;
    private readonly IVoteActivityStore _activity;
    private readonly IVoterKeyService _pseudonyms;
    private readonly TimeProvider _timeProvider;

    public LoginUseCase(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        AuthSessionService authSessionService,
        IVoteActivityStore activity,
        IVoterKeyService pseudonyms,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _authSessionService = authSessionService;
        _activity = activity;
        _pseudonyms = pseudonyms;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<LoginResponse>> ExecuteAsync(
        LoginRequest request,
        SessionMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        var account = _pseudonyms.Pseudonymize("login-account", $"{email.ToLowerInvariant()}|{user?.SecurityStamp}");
        var pair = _pseudonyms.Pseudonymize("login-pair", $"{metadata.IpAddress ?? "unknown"}|{account}");
        var pairKey = $"login-pair:{pair}";
        var accountKey = $"login-account:{account}";

        var blocked = await Longest(
            _activity.GetBlockRemainingAsync(pairKey, cancellationToken),
            _activity.GetBlockRemainingAsync(accountKey, cancellationToken));
        if (blocked is not null)
            return TooManyAttempts(blocked.Value);

        if (!await PasswordIsValidAsync(user, request.Password) || user is null || !user.Active)
            return await RegisterFailureAsync(pairKey, accountKey, cancellationToken);

        user.LastLoginAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _userManager.UpdateAsync(user);

        return await _authSessionService.CreateAsync(user, metadata, cancellationToken);
    }

    /// <summary>Sempre faz uma verificação de hash, exista a conta ou não, para o tempo de resposta não denunciar contas.</summary>
    private async Task<bool> PasswordIsValidAsync(AppUser? user, string password)
    {
        if (user is null)
        {
            _dummyHash ??= _userManager.PasswordHasher.HashPassword(new AppUser(), Guid.NewGuid().ToString("N"));
            _userManager.PasswordHasher.VerifyHashedPassword(new AppUser(), _dummyHash, password);
            return false;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        return result.Succeeded;
    }

    private async Task<ErrorOr<LoginResponse>> RegisterFailureAsync(string pairKey, string accountKey, CancellationToken cancellationToken)
    {
        if (await _activity.IncrementAsync($"{pairKey}:failures", PairWindow, cancellationToken) >= PairMaxFailures)
            await _activity.BlockAsync(pairKey, PairBlock, cancellationToken);

        if (await _activity.IncrementAsync($"{accountKey}:failures", AccountWindow, cancellationToken) >= AccountMaxFailures)
            await _activity.BlockAsync(accountKey, AccountBlock, cancellationToken);

        return Error.Unauthorized("Auth.InvalidCredentials", "E-mail ou senha inválidos.");
    }

    private static async Task<TimeSpan?> Longest(Task<TimeSpan?> first, Task<TimeSpan?> second)
    {
        var a = await first;
        var b = await second;
        return a is null ? b : b is null ? a : a > b ? a : b;
    }

    private static Error TooManyAttempts(TimeSpan remaining)
        => Error.Custom(
            429,
            "Auth.TooManyAttempts",
            "Muitas tentativas de entrada. Tente novamente em alguns minutos.",
            new Dictionary<string, object> { ["retryAfterSeconds"] = (int)Math.Ceiling(remaining.TotalSeconds) });
}
