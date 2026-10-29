namespace Cobranca.Cidada.Infrastructure.Services;

/// <summary>
/// Forma canônica do e-mail para identificar uma pessoa: minúsculas, sem sufixo "+tag" e, no Gmail, sem pontos.
/// Impede que a mesma caixa postal vire várias identidades de voto. Não é usada para enviar e-mails.
/// </summary>
public static class EmailNormalizer
{
    public static string Canonicalize(string email)
    {
        var value = email.Trim().ToLowerInvariant();
        var at = value.LastIndexOf('@');
        if (at <= 0) return value;

        var local = value[..at];
        var domain = value[(at + 1)..];

        var plus = local.IndexOf('+');
        if (plus > 0) local = local[..plus];

        if (domain is "gmail.com" or "googlemail.com")
        {
            local = local.Replace(".", string.Empty);
            domain = "gmail.com";
        }

        return $"{local}@{domain}";
    }

    /// <summary>Forma usada para armazenar e comparar o e-mail de cadastro (sem alterar a caixa postal).</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
