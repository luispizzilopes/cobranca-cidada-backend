using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.GetComments;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Queries.GetComments;

public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, ErrorOr<PaginedResult<CommentResponse>>>
{
    private readonly GetCommentsUseCase _useCase;

    public GetCommentsQueryHandler(GetCommentsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<PaginedResult<CommentResponse>>> Handle(GetCommentsQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query, cancellationToken);
}
