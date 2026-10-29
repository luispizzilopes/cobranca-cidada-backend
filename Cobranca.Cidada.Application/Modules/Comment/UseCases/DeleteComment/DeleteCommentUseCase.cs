using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.GetRecentComments;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;

namespace Cobranca.Cidada.Application.Modules.Comment.UseCases.DeleteComment;

public class DeleteCommentUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteCommentUseCase(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(Guid commentId, string actorId, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.CommentRepository.GetAsync(commentId, cancellationToken);
        if (comment is null)
            return Error.NotFound("Comment.NotFound", "Comentário não encontrado.");

        _unitOfWork.CommentRepository.Remove(comment);
        await _unitOfWork.AuditLogRepository.CreateAsync(
            AuditLog.Create(actorId, AuditActions.CommentRemoved, nameof(CommentAggregate), comment.Id.Value.ToString(), $"commitment:{comment.CommitmentId.Value}"),
            cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _cache.RemoveByPrefixAsync(GetRecentCommentsUseCase.CachePrefix, cancellationToken);

        return Result.Success;
    }
}
