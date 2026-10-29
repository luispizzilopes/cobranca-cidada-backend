using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.GetRecentComments;
using Cobranca.Cidada.Domain.Interfaces.Caching;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;

namespace Cobranca.Cidada.Application.Modules.Comment.UseCases.DeleteMyComment;

public class DeleteMyCommentUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DeleteMyCommentUseCase(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<ErrorOr<Success>> ExecuteAsync(string voterKey, Guid commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.CommentRepository.GetAsync(commentId, cancellationToken);

        if (comment is null || comment.VoterKey != voterKey)
            return Error.NotFound("Comment.NotFound", "Comentário não encontrado.");

        _unitOfWork.CommentRepository.Remove(comment);
        await _unitOfWork.CommitAsync(cancellationToken);

        await _cache.RemoveByPrefixAsync(GetRecentCommentsUseCase.CachePrefix, cancellationToken);

        return Result.Success;
    }
}
