using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using Cobranca.Cidada.BuildingBlocks.Pagination;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Queries.GetComments;

/// <param name="VoterKey">Opcional: só serve para marcar quais comentários são da própria pessoa.</param>
public record GetCommentsQuery(string Slug, int PageNumber, int PageSize, string? VoterKey) : IRequest<ErrorOr<PaginedResult<CommentResponse>>>;
