using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Comment.Queries.GetComments;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;

namespace Cobranca.Cidada.Application.Modules.Comment.UseCases.GetComments;

public class GetCommentsUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCommentsUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<PaginedResult<CommentResponse>>> ExecuteAsync(GetCommentsQuery query, CancellationToken cancellationToken = default)
    {
        var commitmentId = await _unitOfWork.CommitmentRepository.GetPublishedIdBySlugAsync(query.Slug.Trim(), cancellationToken);
        if (commitmentId is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        var pagination = new PaginationParams { PageNumber = query.PageNumber, PageSize = query.PageSize };
        var result = await _unitOfWork.CommentRepository.GetByCommitmentAsync(commitmentId.Value, pagination, cancellationToken);

        var replies = (await _unitOfWork.CommentRepository.GetRepliesAsync(result.Items.Select(c => c.Id.Value), cancellationToken))
            .ToLookup(c => c.ParentId!.Value);

        CommentResponse Map(CommentAggregate c, IReadOnlyList<CommentResponse> children)
            => new(c.Id.Value, c.Text, c.CreatedAtUtc, query.VoterKey is not null && c.VoterKey == query.VoterKey, c.ParentId?.Value, children);

        return new PaginedResult<CommentResponse>
        {
            Items = result.Items
                .Select(c => Map(c, replies[c.Id.Value].Select(r => Map(r, [])).ToList()))
                .ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        };
    }
}
