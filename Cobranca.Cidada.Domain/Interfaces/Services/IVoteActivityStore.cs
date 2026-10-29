namespace Cobranca.Cidada.Domain.Interfaces.Services;

/// <summary>
/// Contadores de curta duração para detectar rajadas de votos. Redis em produção; memória do processo em
/// desenvolvimento. As chaves nunca contêm IP ou e-mail em claro (use hashes).
/// </summary>
public interface IVoteActivityStore
{
    /// <summary>Soma 1 ao contador da janela fixa (que expira sozinha) e devolve o total.</summary>
    Task<long> IncrementAsync(string key, TimeSpan window, CancellationToken cancellationToken = default);

    /// <summary>Registra um membro distinto no conjunto da janela e devolve quantos membros distintos existem.</summary>
    Task<long> AddDistinctAsync(string key, string member, TimeSpan window, CancellationToken cancellationToken = default);

    /// <summary>Liga um bloqueio por tempo determinado.</summary>
    Task BlockAsync(string key, TimeSpan duration, CancellationToken cancellationToken = default);

    /// <summary>Tempo restante do bloqueio, ou nulo se não houver.</summary>
    Task<TimeSpan?> GetBlockRemainingAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Tenta marcar um evento como já tratado na janela. Verdadeiro só na primeira vez (usado para não repetir alertas).</summary>
    Task<bool> TryMarkOnceAsync(string key, TimeSpan window, CancellationToken cancellationToken = default);
}
