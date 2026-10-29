using ErrorOr;
using Cobranca.Cidada.Domain.Interfaces.Services;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Application.Modules.Voting.Settings;
using Microsoft.Extensions.Logging;

namespace Cobranca.Cidada.Application.Modules.Voting.Guards;

/// <summary>
/// Detecção simples de rajadas de votos. Três regras com bloqueio temporário (por pessoa, por IP e por
/// quantidade de pessoas distintas num mesmo IP) e um alerta, sem bloqueio, quando um compromisso recebe um pico de votos.
/// Contadores e registros usam apenas valores pseudonimizados: nunca IP ou e-mail em claro.
/// </summary>
public class VoteBurstGuard
{
    public const string ErrorCode = "Vote.BurstDetected";

    private readonly IVoteActivityStore _store;
    private readonly IVoterKeyService _pseudonyms;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VoteBurstSettings _settings;
    private readonly ILogger<VoteBurstGuard> _logger;

    public VoteBurstGuard(
        IVoteActivityStore store,
        IVoterKeyService pseudonyms,
        IUnitOfWork unitOfWork,
        VoteBurstSettings settings,
        ILogger<VoteBurstGuard> logger)
    {
        _store = store;
        _pseudonyms = pseudonyms;
        _unitOfWork = unitOfWork;
        _settings = settings;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> CheckAsync(string voterKey, string? clientIp, string commitmentSlug, CancellationToken cancellationToken)
    {
        var voter = _pseudonyms.Pseudonymize("burst-voter", voterKey);
        var ip = string.IsNullOrWhiteSpace(clientIp) ? null : _pseudonyms.Pseudonymize("burst-ip", clientIp);

        var remaining = await Longest(
            _store.GetBlockRemainingAsync($"voter:{voter}", cancellationToken),
            ip is null ? Task.FromResult<TimeSpan?>(null) : _store.GetBlockRemainingAsync($"ip:{ip}", cancellationToken));
        if (remaining is not null)
            return Throttled(remaining.Value);

        if (await _store.IncrementAsync($"voter-actions:{voter}", _settings.VoterWindow, cancellationToken) > _settings.VoterMaxActions)
            return await BlockAsync($"voter:{voter}", _settings.VoterBlock, "pessoa", commitmentSlug, cancellationToken);

        if (ip is not null)
        {
            if (await _store.IncrementAsync($"ip-actions:{ip}", _settings.IpWindow, cancellationToken) > _settings.IpMaxActions)
                return await BlockAsync($"ip:{ip}", _settings.IpBlock, "ip", commitmentSlug, cancellationToken);

            if (await _store.AddDistinctAsync($"ip-voters:{ip}", voter, _settings.IpDistinctWindow, cancellationToken) > _settings.IpMaxDistinctVoters)
                return await BlockAsync($"ip:{ip}", _settings.IpDistinctBlock, "ip-pessoas-distintas", commitmentSlug, cancellationToken);
        }

        var spike = await _store.IncrementAsync($"commitment-actions:{commitmentSlug}", _settings.CommitmentSpikeWindow, cancellationToken);
        if (spike > _settings.CommitmentSpikeMaxActions &&
            await _store.TryMarkOnceAsync($"commitment-alert:{commitmentSlug}", _settings.CommitmentSpikeAlertInterval, cancellationToken))
        {
            _logger.LogWarning("Pico de votos no compromisso {Commitment}: {Actions} ações em {Window}s.", commitmentSlug, spike, (int)_settings.CommitmentSpikeWindow.TotalSeconds);
            await _unitOfWork.AuditLogRepository.CreateAsync(AuditLog.Create(
                "system", AuditActions.VoteBurstOnCommitment, "Commitment", commitmentSlug,
                $"{spike} ações de voto em {(int)_settings.CommitmentSpikeWindow.TotalSeconds}s (limite {_settings.CommitmentSpikeMaxActions})."), cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> BlockAsync(string key, TimeSpan duration, string rule, string commitmentSlug, CancellationToken cancellationToken)
    {
        await _store.BlockAsync(key, duration, cancellationToken);

        _logger.LogWarning("Rajada de votos bloqueada pela regra {Rule} por {Seconds}s.", rule, (int)duration.TotalSeconds);
        await _unitOfWork.AuditLogRepository.CreateAsync(AuditLog.Create(
            "system", AuditActions.VoteBurstBlocked, "Commitment", commitmentSlug,
            $"Regra: {rule}. Bloqueio de {(int)duration.TotalSeconds}s."), cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Throttled(duration);
    }

    private static Error Throttled(TimeSpan retryAfter)
    {
        var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
        return Error.Custom(
            429,
            ErrorCode,
            "Detectamos muitos votos em pouco tempo. Aguarde um pouco antes de continuar.",
            new Dictionary<string, object> { ["retryAfterSeconds"] = seconds });
    }

    private static async Task<TimeSpan?> Longest(Task<TimeSpan?> a, Task<TimeSpan?> b)
    {
        var first = await a;
        var second = await b;
        if (first is null) return second;
        if (second is null) return first;
        return first > second ? first : second;
    }
}
