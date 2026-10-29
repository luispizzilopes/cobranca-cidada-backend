using Cobranca.Cidada.Domain.Interfaces.Repositories.Audit;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Catalog;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Comment;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Mandate;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Moderation;
using Cobranca.Cidada.Domain.Interfaces.Repositories.User;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;

namespace Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

public interface IUnitOfWork
{
    ICommitmentRepository CommitmentRepository { get; set; }
    IAreaRepository AreaRepository { get; set; }
    IOriginRepository OriginRepository { get; set; }
    IMandateRepository MandateRepository { get; set; }
    IAuditLogRepository AuditLogRepository { get; set; }
    IUserSessionRepository UserSessionRepository { get; set; }
    IVoteRepository VoteRepository { get; set; }
    IVoterIdentityRepository VoterIdentityRepository { get; set; }
    ICommentRepository CommentRepository { get; set; }
    IProfanityWordRepository ProfanityWordRepository { get; set; }
    /// <summary>Executa as operações numa única transação; se algo lançar exceção, nada é gravado.</summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
