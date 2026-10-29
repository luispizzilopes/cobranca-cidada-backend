using Cobranca.Cidada.Domain.Modules.Commitment.Constants;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.Filters;

namespace Cobranca.Cidada.Application.Modules.Commitment.Parsers;

/// <summary>Interpretação dos parâmetros de situação e ordenação, compartilhada pela consulta pública e pela do painel.</summary>
public static class CommitmentQueryParser
{
    /// <summary>Aceita o rótulo em português ou o nome do enum; nulo quando vazio ou desconhecido.</summary>
    public static CommitmentStatus? ParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (CommitmentStatusLabels.TryParse(value, out var byLabel)) return byLabel;
        return Enum.TryParse<CommitmentStatus>(value, out var byName) ? byName : null;
    }

    public static CommitmentSort ParseSort(string? value) => value?.ToLowerInvariant() switch
    {
        "code" => CommitmentSort.Code,
        "deadline" => CommitmentSort.Deadline,
        "mais-cobradas" => CommitmentSort.MostCharged,
        "melhor-avaliadas" => CommitmentSort.BestRated,
        "pior-avaliadas" => CommitmentSort.WorstRated,
        _ => CommitmentSort.Area
    };
}
