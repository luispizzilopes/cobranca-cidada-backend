using ErrorOr;
using Cobranca.Cidada.Application.Modules.Comment.Requests;
using Cobranca.Cidada.Application.Modules.Comment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.AddComment;

public record AddCommentCommand(string VoterKey, string Slug, AddCommentRequest Request) : IRequest<ErrorOr<CommentResponse>>;
