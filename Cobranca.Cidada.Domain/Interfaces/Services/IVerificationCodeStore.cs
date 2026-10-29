namespace Cobranca.Cidada.Domain.Interfaces.Services;

public enum VerificationOutcome
{
    Valid,
    Invalid,
    Expired,
    TooManyAttempts
}

/// <summary>Códigos de uso único enviados por e-mail (cadastro e voto). Só o hash do código é guardado.</summary>
public interface IVerificationCodeStore
{
    /// <summary>Falso quando já houve um envio recente (intervalo mínimo entre envios).</summary>
    Task<bool> TryReserveSendAsync(string purpose, string email, CancellationToken cancellationToken = default);
    Task StoreAsync(string purpose, string email, string code, CancellationToken cancellationToken = default);
    Task<VerificationOutcome> VerifyAsync(string purpose, string email, string code, CancellationToken cancellationToken = default);
    Task RemoveAsync(string purpose, string email, CancellationToken cancellationToken = default);
}
