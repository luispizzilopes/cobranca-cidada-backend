using Cobranca.Cidada.Domain.Interfaces.Repositories.Moderation;
using Cobranca.Cidada.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Cobranca.Cidada.Infrastructure.Repositories.Moderation;

public class ProfanityWordRepository : IProfanityWordRepository
{
    private readonly AppDbContext _context;

    public ProfanityWordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetActiveWordsAsync(CancellationToken cancellationToken)
        => await _context.ProfanityWords.AsNoTracking()
            .Where(w => w.IsActive)
            .OrderByDescending(w => w.Word.Length)
            .Select(w => w.Word)
            .ToListAsync(cancellationToken);
}
