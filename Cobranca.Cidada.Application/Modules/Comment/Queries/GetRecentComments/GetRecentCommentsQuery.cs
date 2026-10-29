using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Queries.GetRecentComments;

public record GetRecentCommentsQuery(int Limit) : IRequest<ErrorOr<IReadOnlyList<RecentCommentResponse>>>;
