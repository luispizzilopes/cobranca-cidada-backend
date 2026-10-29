using Cobranca.Cidada.BuildingBlocks.Common.Interfaces;
using Cobranca.Cidada.BuildingBlocks.Common.Interfaces.Events;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.Entities;
using Cobranca.Cidada.Domain.Modules.Comment.Aggregates;
using Cobranca.Cidada.Domain.Modules.Mandate.Entities;
using Cobranca.Cidada.Domain.Modules.Moderation.Entities;
using Cobranca.Cidada.Domain.Modules.User.Entities;
using Cobranca.Cidada.Domain.Modules.Voting.Aggregates;
using Cobranca.Cidada.Domain.Modules.Voting.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;
using MandateAggregate = Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate;

namespace Cobranca.Cidada.Infrastructure.Context;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<Area> Areas { get; set; }
    public virtual DbSet<Origin> Origins { get; set; }
    public virtual DbSet<Official> Officials { get; set; }
    public virtual DbSet<MandateAggregate> Mandates { get; set; }
    public virtual DbSet<CommitmentAggregate> Commitments { get; set; }
    public virtual DbSet<CommitmentSource> CommitmentSources { get; set; }
    public virtual DbSet<StatusEntry> StatusEntries { get; set; }
    public virtual DbSet<CommitmentRevision> CommitmentRevisions { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<UserSession> UserSessions { get; set; }
    public virtual DbSet<Vote> Votes { get; set; }
    public virtual DbSet<VoterIdentity> VoterIdentities { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<ProfanityWord> ProfanityWords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<AppUser>().Property(user => user.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<AppUser>().Property(user => user.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Ignore<List<IDomainEvent>>();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreatedAt(now);

            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.SetUpdatedAt(now);
        }
    }
}
