using System.Globalization;
using System.Text;

namespace Cobranca.Cidada.Application.Modules.Comment.Filters;

/// <summary>
/// Troca palavras ofensivas por asteriscos mantendo a primeira letra ("merda" → "m****").
/// Ignora maiúsculas e acentos ("MÉRDA" também é pega), aceita letras esticadas ("putaaaaa", "caralhoooo") e só casa
/// palavra inteira ("cuidado" não é afetado por "cu"). Expressões com mais de uma palavra ("filho da puta") têm cada
/// palavra mascarada. O texto original não é alterado.
/// </summary>
/// <remarks>
/// A repetição é comparada por sequências de letras iguais: cada sequência da palavra precisa aparecer no texto com pelo
/// menos o mesmo tamanho. Assim "ass" casa com "assss", mas não com "as" (o artigo em português), e "porra" exige os dois
/// "r" mas aceita "porrrrra".
/// </remarks>
public static class ProfanityCensor
{
    private readonly record struct Run(char Char, int Count, int Start, int End);

    /// <param name="words">Palavras já normalizadas (veja <c>ProfanityWord.Normalize</c>).</param>
    public static string Censor(string text, IReadOnlyCollection<string> words)
    {
        if (string.IsNullOrEmpty(text) || words.Count == 0) return text;

        var textRuns = ToRuns(Normalize(text));
        var masked = new bool[text.Length];
        var any = false;

        foreach (var word in words)
        {
            if (word.Length == 0) continue;

            var wordRuns = ToRuns(word.Select((c, i) => (c, i, i + 1)).ToList());
            for (var r = 0; r <= textRuns.Count - wordRuns.Count; r++)
            {
                if (r > 0 && char.IsLetterOrDigit(textRuns[r - 1].Char)) continue;

                var last = r + wordRuns.Count - 1;
                if (last + 1 < textRuns.Count && char.IsLetterOrDigit(textRuns[last + 1].Char)) continue;

                var matches = true;
                for (var k = 0; k < wordRuns.Count && matches; k++)
                    matches = textRuns[r + k].Char == wordRuns[k].Char && textRuns[r + k].Count >= wordRuns[k].Count;
                if (!matches) continue;

                for (var i = textRuns[r].Start; i < textRuns[last].End; i++) masked[i] = true;
                any = true;
            }
        }

        if (!any) return text;

        var result = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var isWordChar = char.IsLetterOrDigit(text[i]);
            var startsRun = i == 0 || !masked[i - 1] || !char.IsLetterOrDigit(text[i - 1]);
            result.Append(masked[i] && isWordChar && !startsRun ? '*' : text[i]);
        }

        return result.ToString();
    }

    /// <summary>Agrupa caracteres iguais seguidos, guardando onde a sequência começa e termina no texto original.</summary>
    private static List<Run> ToRuns(List<(char Char, int Start, int End)> chars)
    {
        var runs = new List<Run>(chars.Count);
        foreach (var (c, start, end) in chars)
        {
            if (runs.Count > 0 && runs[^1].Char == c && c != ' ')
                runs[^1] = runs[^1] with { Count = runs[^1].Count + 1, End = end };
            else
                runs.Add(new Run(c, 1, start, end));
        }

        return runs;
    }

    /// <summary>Minúsculas, sem acentos e com hífens/espaços colapsados, guardando de onde cada caractere veio no original.</summary>
    private static List<(char Char, int Start, int End)> Normalize(string text)
    {
        var chars = new List<(char Char, int Start, int End)>(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsWhiteSpace(c) || c == '-')
            {
                if (chars.Count > 0 && chars[^1].Char == ' ')
                    chars[^1] = chars[^1] with { End = i + 1 };
                else
                    chars.Add((' ', i, i + 1));
                continue;
            }

            foreach (var d in char.ToLowerInvariant(c).ToString().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(d) == UnicodeCategory.NonSpacingMark) continue;
                chars.Add((d, i, i + 1));
            }
        }

        return chars;
    }
}
