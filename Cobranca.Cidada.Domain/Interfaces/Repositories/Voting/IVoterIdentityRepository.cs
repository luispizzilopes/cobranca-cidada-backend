using Cobranca.Cidada.Domain.Modules.Voting.Entities;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;

public interface IVoterIdentityRepository
{
    Task<VoterIdentity?> GetAsync(string voterKey, CancellationToken cancellationToken);
    Task CreateAsync(VoterIdentity identity, CancellationToken cancellationToken);
    Task<bool> IsTokenValidAsync(string voterKey, DateTime issuedAtUtc, CancellationToken cancellationToken);
    Task DeleteAsync(string voterKey, CancellationToken cancellationToken);
}
