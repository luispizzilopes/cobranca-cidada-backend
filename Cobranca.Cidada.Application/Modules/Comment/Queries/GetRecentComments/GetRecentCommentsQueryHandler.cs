using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.Application.Modules.Comment.UseCases.GetRecentComments;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Queries.GetRecentComments;

public class GetRecentCommentsQueryHandler : IRequestHandler<GetRecentCommentsQuery, ErrorOr<IReadOnlyList<RecentCommentResponse>>>
{
    private readonly GetRecentCommentsUseCase _useCase;

    public GetRecentCommentsQueryHandler(GetRecentCommentsUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<IReadOnlyList<RecentCommentResponse>>> Handle(GetRecentCommentsQuery query, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(query, cancellationToken);
}
