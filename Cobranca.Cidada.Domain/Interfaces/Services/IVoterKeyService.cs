namespace Cobranca.Cidada.Domain.Interfaces.Services;

/// <summary>Deriva a identidade pseudônima do votante a partir do e-mail verificado.</summary>
public interface IVoterKeyService
{
    /// <summary>HMAC-SHA256 (hex) do e-mail canônico. Mesmo e-mail, mesma chave; sem a chave secreta não é reversível.</summary>
    string Compute(string email);

    /// <summary>HMAC (hex) de um valor dentro de um escopo. Para guardar IP ou chaves de contadores sem expô-los em claro.</summary>
    string Pseudonymize(string scope, string value);
}
