namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Moderation;

public interface IProfanityWordRepository
{
    /// <summary>Palavras ativas já normalizadas, das mais longas às mais curtas.</summary>
    Task<IReadOnlyList<string>> GetActiveWordsAsync(CancellationToken cancellationToken);
}
