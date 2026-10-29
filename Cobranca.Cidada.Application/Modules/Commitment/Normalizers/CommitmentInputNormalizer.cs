using Cobranca.Cidada.Application.Modules.Commitment.Requests;

namespace Cobranca.Cidada.Application.Modules.Commitment.Normalizers;

/// <summary>Limpeza de campos vindos do painel antes da validação: vazio vira nulo, listas sem linhas em branco.</summary>
public static class CommitmentInputNormalizer
{
    public static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static List<string> Clean(IEnumerable<string>? values)
        => (values ?? []).Select(v => v?.Trim() ?? string.Empty).Where(v => v.Length > 0).ToList();

    public static List<SourceInput> Sources(IEnumerable<SourceInput>? sources)
        => (sources ?? [])
            .Select(s => new SourceInput(s.Name?.Trim() ?? string.Empty, s.Url?.Trim() ?? string.Empty, Blank(s.Page), s.Kind?.Trim().ToLowerInvariant() ?? string.Empty))
            .ToList();
}
