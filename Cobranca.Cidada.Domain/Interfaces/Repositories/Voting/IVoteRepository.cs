using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.Enums;

namespace Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;

public record VoteTally(Guid CommitmentId, int Charges, int Good, int Bad);

public record VoteView(Guid CommitmentId, VoteRating? Rating, bool Charge);

public interface IVoteRepository
{
    Task<Vote?> GetAsync(string voterKey, Guid commitmentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Vote>> GetAllByVoterAsync(string voterKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<VoteView>> GetViewsAsync(string voterKey, IEnumerable<Guid> commitmentIds, CancellationToken cancellationToken);
    Task<IReadOnlyList<VoteTally>> GetTalliesAsync(IEnumerable<Guid> commitmentIds, CancellationToken cancellationToken);
    Task CreateAsync(Vote vote, CancellationToken cancellationToken);
    void Remove(Vote vote);
    /// <summary>
    /// Move os votos de uma chave para outra (troca de e-mail). Quando as duas chaves votaram no mesmo compromisso,
    /// vale o voto mais recente. Não confirma; quem chama confirma pela unidade de trabalho.
    /// </summary>
    Task MoveAsync(string fromVoterKey, string toVoterKey, CancellationToken cancellationToken);
    Task<int> DeleteAllByVoterAsync(string voterKey, CancellationToken cancellationToken);
}
