using Cobranca.Cidada.Domain.Modules.Mandate.Entities;
using MandateAggregate = Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Mandate;

public interface IMandateRepository
{
    Task<MandateAggregate?> GetByTitleAsync(string title, CancellationToken cancellationToken);
    Task<MandateAggregate?> GetCurrentAsync(CancellationToken cancellationToken);
    Task<Official?> GetOfficialAsync(Guid officialId, CancellationToken cancellationToken);
    Task<Official?> GetOfficialByNameAsync(string name, CancellationToken cancellationToken);
    Task CreateOfficialAsync(Official official, CancellationToken cancellationToken);
    Task CreateAsync(MandateAggregate mandate, CancellationToken cancellationToken);
}
