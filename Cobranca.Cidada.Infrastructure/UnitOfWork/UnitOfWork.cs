using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Audit;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Catalog;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Comment;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Mandate;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Moderation;
using Cobranca.Cidada.Domain.Interfaces.Repositories.User;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Infrastructure.Context;
using Cobranca.Cidada.Infrastructure.Repositories.Audit;
using Cobranca.Cidada.Infrastructure.Repositories.Catalog;
using Cobranca.Cidada.Infrastructure.Repositories.Comment;
using Cobranca.Cidada.Infrastructure.Repositories.Commitment;
using Cobranca.Cidada.Infrastructure.Repositories.Mandate;
using Cobranca.Cidada.Infrastructure.Repositories.Moderation;
using Cobranca.Cidada.Infrastructure.Repositories.User;
using Cobranca.Cidada.Infrastructure.Repositories.Voting;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cobranca.Cidada.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private CommitmentRepository _commitmentRepository = null!;
    private readonly ILogger<CommitmentRepository> _commitmentLogger;

    private AreaRepository _areaRepository = null!;
    private OriginRepository _originRepository = null!;
    private MandateRepository _mandateRepository = null!;
    private AuditLogRepository _auditLogRepository = null!;
    private UserSessionRepository _userSessionRepository = null!;
    private VoteRepository _voteRepository = null!;
    private VoterIdentityRepository _voterIdentityRepository = null!;
    private CommentRepository _commentRepository = null!;
    private ProfanityWordRepository _profanityWordRepository = null!;

    public UnitOfWork(AppDbContext context, ILogger<CommitmentRepository> commitmentLogger)
    {
        _context = context;
        _commitmentLogger = commitmentLogger;
    }

    public ICommitmentRepository CommitmentRepository
    {
        get => _commitmentRepository ??= new CommitmentRepository(_context, _commitmentLogger);
        set { }
    }

    public IAreaRepository AreaRepository
    {
        get => _areaRepository ??= new AreaRepository(_context);
        set { }
    }

    public IOriginRepository OriginRepository
    {
        get => _originRepository ??= new OriginRepository(_context);
        set { }
    }

    public IMandateRepository MandateRepository
    {
        get => _mandateRepository ??= new MandateRepository(_context);
        set { }
    }

    public IAuditLogRepository AuditLogRepository
    {
        get => _auditLogRepository ??= new AuditLogRepository(_context);
        set { }
    }

    public IUserSessionRepository UserSessionRepository
    {
        get => _userSessionRepository ??= new UserSessionRepository(_context);
        set { }
    }

    public IVoteRepository VoteRepository
    {
        get => _voteRepository ??= new VoteRepository(_context);
        set { }
    }

    public IVoterIdentityRepository VoterIdentityRepository
    {
        get => _voterIdentityRepository ??= new VoterIdentityRepository(_context);
        set { }
    }

    public ICommentRepository CommentRepository
    {
        get => _commentRepository ??= new CommentRepository(_context);
        set { }
    }

    public IProfanityWordRepository ProfanityWordRepository
    {
        get => _profanityWordRepository ??= new ProfanityWordRepository(_context);
        set { }
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await action(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_Votes_VoterKey_CommitmentId"
            })
        {
            foreach (var entry in _context.ChangeTracker.Entries<Cobranca.Cidada.Domain.Modules.Voting.Aggregates.Vote>().Where(e => e.State == EntityState.Added).ToList())
                entry.State = EntityState.Detached;

            throw new ConcurrencyConflictException("Voto concorrente para o mesmo compromisso.", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entities = string.Join(", ", ex.Entries.Select(entry => $"{entry.Metadata.ClrType.Name}:{entry.State}"));
            throw new ConcurrencyConflictException($"O registro foi alterado por outra operação concorrente. Entidades: {entities}", ex);
        }
    }
}
