using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cobranca.Cidada.Application.Modules.Commitment.Normalizers;

public static partial class TextNormalizer
{
    /// <summary>Converte texto em slug ASCII minúsculo separado por hífens.</summary>
    public static string Slugify(string text)
    {
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        var ascii = builder.ToString().ToLowerInvariant();
        return NonAlphanumeric().Replace(ascii, "-").Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();
}
