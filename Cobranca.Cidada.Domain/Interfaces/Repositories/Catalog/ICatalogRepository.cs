using Cobranca.Cidada.Domain.Modules.Catalog.Entities;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Catalog;

public interface IAreaRepository
{
    Task<IReadOnlyList<Area>> GetAllAsync(CancellationToken cancellationToken);
    Task<Area?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task CreateAsync(Area area, CancellationToken cancellationToken);
}

public interface IOriginRepository
{
    Task<IReadOnlyList<Origin>> GetAllAsync(CancellationToken cancellationToken);
    Task<Origin?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task CreateAsync(Origin origin, CancellationToken cancellationToken);
}
