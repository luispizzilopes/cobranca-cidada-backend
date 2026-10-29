using System.Text.Json;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Services;
using Cobranca.Cidada.Application.Modules.Commitment.Validators;
using Cobranca.Cidada.Domain.Modules.Commitment.Constants;

namespace Cobranca.Cidada.Tests.Application.Commitment;

/// <summary>
/// Contrato do arquivo de carga: o build falha se o JSON de exemplo tiver ID duplicado,
/// status inválido, URL malformada ou prazo vazio.
/// </summary>
public class SeedFileContractTests
{
    private static readonly SeedFileRequest File = SeedFileLocator.Load();

    [Fact]
    public void File_HasCommitments() => Assert.NotEmpty(File.Commitments);

    [Fact]
    public void Ids_AreUniqueAndPresent()
    {
        Assert.All(File.Commitments, c => Assert.False(string.IsNullOrWhiteSpace(c.Id)));
        var duplicated = File.Commitments.GroupBy(c => c.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.True(duplicated.Count == 0, $"IDs duplicados: {string.Join(", ", duplicated)}");
    }

    [Fact]
    public void Titles_AreNotTruncated()
    {
        var truncated = File.Commitments
            .Where(c => c.Title is not null && (c.Title.TrimEnd().EndsWith('…') || c.Title.TrimEnd().EndsWith("...")))
            .Select(c => c.Id)
            .ToList();
        Assert.True(truncated.Count == 0, $"Títulos cortados: {string.Join(", ", truncated)}");
    }

    [Fact]
    public void Statuses_AreWithinTheAllowedSet()
    {
        var invalid = File.Commitments
            .Where(c => !CommitmentStatusLabels.TryParse(c.Execution?.Status, out _))
            .Select(c => $"{c.Id}: '{c.Execution?.Status}'")
            .ToList();
        Assert.True(invalid.Count == 0, $"Status inválidos: {string.Join("; ", invalid)}");
    }

    [Fact]
    public void Urls_AreWellFormed()
    {
        var invalid = File.Commitments
            .SelectMany(c => (c.Sources ?? []).Select(s => (c.Id, s.Url)))
            .Where(x => !Uri.TryCreate(x.Url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
            .Select(x => $"{x.Id}: '{x.Url}'")
            .ToList();
        Assert.True(invalid.Count == 0, $"URLs inválidas: {string.Join("; ", invalid)}");
    }

    [Fact]
    public void DeadlineText_IsNullWhenThereIsNoDeadline()
        => Assert.All(File.Commitments, c => Assert.True(c.DeadlineText is null || !string.IsNullOrWhiteSpace(c.DeadlineText), $"{c.Id}: prazo_texto vazio"));

    [Fact]
    public void EveryItem_PassesTheSharedCommitmentValidator()
    {
        var validator = new CommitmentInputValidator(TimeProvider.System);
        var failures = File.Commitments
            .Select(c => (c.Id, Result: validator.Validate(SeedCommitmentMapper.ToInput(c))))
            .Where(x => !x.Result.IsValid)
            .Select(x => $"{x.Id}: {string.Join(" | ", x.Result.Errors.Select(e => e.ErrorMessage))}")
            .ToList();
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}

internal static class SeedFileLocator
{
    public const string RelativePath = "docs/dados/promessas-lula-eleicoes-2026.json";

    public static string Path()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = System.IO.Path.Combine(dir.FullName, RelativePath);
            if (System.IO.File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        throw new FileNotFoundException($"Arquivo de carga não encontrado ({RelativePath}).");
    }

    public static SeedFileRequest Load()
        => JsonSerializer.Deserialize<SeedFileRequest>(System.IO.File.ReadAllText(Path()))!;
}
