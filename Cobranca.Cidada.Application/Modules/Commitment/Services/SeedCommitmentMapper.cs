using System.Globalization;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;

namespace Cobranca.Cidada.Application.Modules.Commitment.Services;

public static class SeedCommitmentMapper
{
    private const string LegacyIdPrefix = "LULA26-";

    private static readonly string[] PrimaryHostSuffixes = [".gov.br", ".jus.br", ".leg.br", ".mp.br", ".def.br"];

    public static CommitmentInput ToInput(SeedCommitmentRequest item)
    {
        var sources = (item.Sources ?? [])
            .Select(s => new SourceInput(s.Name ?? string.Empty, s.Url ?? string.Empty, s.Page, ClassifySource(s.Url)))
            .ToList();

        InitialStatusInput? status = null;
        if (item.Execution is { Status: not null })
        {
            var reference = TryParseDate(item.Execution.ReferenceDate) ?? default;

            status = new InitialStatusInput(item.Execution.Status, reference, sources.FirstOrDefault()?.Url, "Carga inicial.", false);
        }

        return new CommitmentInput(
            ExternalId: item.Id,
            Slug: SlugFromLegacyId(item.Id),
            AreaCode: item.AreaCode ?? string.Empty,
            AreaName: item.Area ?? string.Empty,
            OriginName: item.Origin ?? string.Empty,
            Title: item.Title ?? string.Empty,
            Description: item.Description ?? string.Empty,
            ShortDescription: item.ShortDescription,
            HowItWillBeDone: item.HowItWillBeDone,
            HasPlanDetail: string.Equals(item.PlanDetail, "sim", StringComparison.OrdinalIgnoreCase),
            Date: TryParseDate(item.Date),
            DateKind: item.DateKind,
            DeadlineText: item.DeadlineText,
            DeadlineDate: null,
            GoalOrCondition: item.GoalOrCondition,
            Classification: item.Classification ?? [],
            Evidence: item.Evidence ?? [],
            PlanPage: item.PlanPage,
            Sources: sources,
            InitialStatus: status);
    }

    public static string? SlugFromLegacyId(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var trimmed = id.Trim();
        return trimmed.StartsWith(LegacyIdPrefix, StringComparison.OrdinalIgnoreCase) ? trimmed[LegacyIdPrefix.Length..] : trimmed;
    }

    /// <summary>Heurística de tipo de fonte: domínios oficiais são primárias; o restante, imprensa. A curadoria pode corrigir.</summary>
    public static string ClassifySource(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return "press";
        var host = uri.Host.ToLowerInvariant();
        return PrimaryHostSuffixes.Any(suffix => host.EndsWith(suffix, StringComparison.Ordinal)) ? "primary" : "press";
    }

    private static DateOnly? TryParseDate(string? value)
        => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
}
