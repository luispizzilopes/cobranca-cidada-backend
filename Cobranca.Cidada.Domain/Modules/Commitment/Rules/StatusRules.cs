using Cobranca.Cidada.Domain.Modules.Commitment.Enums;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Rules;

/// <summary>
/// Regras editoriais de situação de execução. Fonte única de verdade: usadas pelo seed,
/// pelo painel de curadoria (validators) e pelo agregado.
/// </summary>
public static class StatusRules
{
    /// <summary>Retorna a mensagem de violação, ou null quando o registro é válido.</summary>
    public static string? Check(
        CommitmentStatus status,
        DateOnly referenceDate,
        string? sourceUrl,
        DateOnly? deadlineDate,
        bool isFormalAct,
        DateOnly today)
    {
        if (referenceDate > today)
            return "A data de referência não pode estar no futuro.";

        if (status == CommitmentStatus.NotEvaluated)
            return null;

        if (!IsValidHttpUrl(sourceUrl))
            return "Toda mudança de situação exige uma fonte com URL válida (http ou https).";

        if (status == CommitmentStatus.Unfulfilled)
        {
            if (deadlineDate is { } deadline)
            {
                if (referenceDate <= deadline)
                    return "Não é possível marcar como Descumprido antes de vencido o prazo.";
            }
            else if (!isFormalAct)
            {
                return "Sem prazo definido, 'Descumprido' exige ato formal em sentido contrário, com fonte.";
            }
        }

        return null;
    }

    public static bool IsValidHttpUrl(string? url)
        => Uri.TryCreate(url, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
