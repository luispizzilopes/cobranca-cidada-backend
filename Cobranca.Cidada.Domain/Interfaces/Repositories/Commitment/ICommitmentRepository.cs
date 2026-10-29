using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Modules.Commitment.Filters;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;

/// <summary>Identificador público e data da última alteração de um compromisso publicado (para o sitemap).</summary>
public record PublishedCommitmentRef(string Slug, DateTime UpdatedAtUtc);

public interface ICommitmentRepository
{
    /// <summary>Todos os compromissos publicados, só com o necessário para o sitemap.</summary>
    Task<IReadOnlyList<PublishedCommitmentRef>> GetPublishedRefsAsync(CancellationToken cancellationToken);

    Task<CommitmentAggregate?> GetBySlugAsync(string slug, bool onlyPublished, CancellationToken cancellationToken);
    Task<CommitmentAggregate?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken);
    Task<HashSet<string>> GetExistingExternalIdsAsync(IEnumerable<string> externalIds, CancellationToken cancellationToken);
    Task<HashSet<string>> GetExistingSlugsAsync(CancellationToken cancellationToken);
    Task<PaginedResult<CommitmentAggregate>> GetPublishedAsync(CommitmentFilters filters, PaginationParams pagination, CancellationToken cancellationToken);
    /// <summary>Lista para o painel de administração: publicados ou não, conforme <paramref name="published"/>.</summary>
    Task<PaginedResult<CommitmentAggregate>> GetAllAsync(CommitmentFilters filters, bool? published, PaginationParams pagination, CancellationToken cancellationToken);
    /// <summary>Leitura completa (publicado ou não, com revisões), sem rastreamento.</summary>
    Task<CommitmentAggregate?> GetForAdminAsync(string slug, CancellationToken cancellationToken);
    /// <summary>Carrega o agregado rastreado (fontes e histórico) para edição.</summary>
    Task<CommitmentAggregate?> GetForUpdateAsync(string slug, CancellationToken cancellationToken);
    Task<Guid?> GetPublishedIdBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, (string Slug, string Title)>> GetHeadersByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task CreateAsync(CommitmentAggregate commitment, CancellationToken cancellationToken);
    Task UpdateAsync(CommitmentAggregate commitment, CancellationToken cancellationToken);
}
