using System.Globalization;
using System.Text;
using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Moderation.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Moderation.Entities;

/// <summary>
/// Palavra (ou expressão) a censurar nos comentários. Fica guardada já normalizada (minúscula, sem acento,
/// espaços simples), então quem compara deve normalizar o texto do mesmo jeito com <see cref="Normalize"/>.
/// </summary>
public class ProfanityWord : Entity<ProfanityWordId>
{
    public const string Portuguese = "pt";
    public const string English = "en";
    public const int MaxLength = 100;

    public string Word { get; private set; } = null!;
    public string Language { get; private set; } = null!;
    /// <summary>Permite desligar uma palavra sem apagá-la (ex.: falso positivo).</summary>
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ProfanityWord() { }

    private ProfanityWord(string word, string language) : base(ProfanityWordId.CreateIdentifier())
    {
        Word = word;
        Language = language;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static ProfanityWord Create(string word, string language)
    {
        var normalized = Normalize(word);
        if (normalized.Length == 0) throw new DomainException("A palavra é obrigatória.");
        if (normalized.Length > MaxLength) throw new DomainException($"A palavra deve ter no máximo {MaxLength} caracteres.");

        var lang = language?.Trim().ToLowerInvariant();
        if (lang != Portuguese && lang != English) throw new DomainException("O idioma deve ser 'pt' ou 'en'.");

        return new ProfanityWord(normalized, lang);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    /// <summary>Minúscula, sem acentos e com espaços/hífens colapsados em um espaço ("Foda-se" → "foda se").</summary>
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var decomposed = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);
        var lastWasSpace = false;
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsWhiteSpace(c) || c == '-')
            {
                if (!lastWasSpace) sb.Append(' ');
                lastWasSpace = true;
                continue;
            }

            sb.Append(c);
            lastWasSpace = false;
        }

        return sb.ToString().Trim().Normalize(NormalizationForm.FormC);
    }
}
