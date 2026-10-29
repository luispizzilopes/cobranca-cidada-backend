using Cobranca.Cidada.Domain.Interfaces.Repositories.Audit;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Catalog;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Comment;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Commitment;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Mandate;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Moderation;
using Cobranca.Cidada.Domain.Interfaces.Repositories.User;
using Cobranca.Cidada.Domain.Interfaces.Repositories.Voting;
using Cobranca.Cidada.Infrastructure.Repositories.Audit;
using Cobranca.Cidada.Infrastructure.Repositories.Catalog;
using Cobranca.Cidada.Infrastructure.Repositories.Comment;
using Cobranca.Cidada.Infrastructure.Repositories.Commitment;
using Cobranca.Cidada.Infrastructure.Repositories.Mandate;
using Cobranca.Cidada.Infrastructure.Repositories.Moderation;
using Cobranca.Cidada.Infrastructure.Repositories.User;
using Cobranca.Cidada.Infrastructure.Repositories.Voting;
using Microsoft.Extensions.DependencyInjection;

namespace Cobranca.Cidada.CrossCutting.IoC.Infrastructure.Repositories;

public static class RepositoriesDependencyInjection
{
    public static void AddRepositoriesDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<ICommitmentRepository, CommitmentRepository>();
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IOriginRepository, OriginRepository>();
        services.AddScoped<IMandateRepository, MandateRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IVoterIdentityRepository, VoterIdentityRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IProfanityWordRepository, ProfanityWordRepository>();
    }
}
