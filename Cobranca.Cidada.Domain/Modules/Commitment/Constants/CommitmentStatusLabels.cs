using Cobranca.Cidada.Domain.Modules.Commitment.Enums;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Constants;

/// <summary>Rótulos públicos (pt-BR) dos nove status editoriais.</summary>
public static class CommitmentStatusLabels
{
    private static readonly IReadOnlyDictionary<CommitmentStatus, string> Labels = new Dictionary<CommitmentStatus, string>
    {
        [CommitmentStatus.NotEvaluated] = "Não avaliado",
        [CommitmentStatus.NoEvidenceFound] = "Sem evidência localizada",
        [CommitmentStatus.InPreparation] = "Em preparação",
        [CommitmentStatus.InProgress] = "Em andamento",
        [CommitmentStatus.PartiallyFulfilled] = "Parcialmente cumprido",
        [CommitmentStatus.Fulfilled] = "Cumprido",
        [CommitmentStatus.Unfulfilled] = "Descumprido",
        [CommitmentStatus.ChangedOrReplaced] = "Alterado/substituído",
        [CommitmentStatus.ImpairedOrConditioned] = "Prejudicado/condicionado",
    };

    public static string ToLabel(this CommitmentStatus status) => Labels[status];

    public static bool TryParse(string? label, out CommitmentStatus status)
    {
        var match = Labels.FirstOrDefault(pair => string.Equals(pair.Value, label?.Trim(), StringComparison.OrdinalIgnoreCase));
        status = match.Key;
        return match.Value is not null;
    }

    public static IReadOnlyCollection<string> AllLabels => Labels.Values.ToArray();
}
