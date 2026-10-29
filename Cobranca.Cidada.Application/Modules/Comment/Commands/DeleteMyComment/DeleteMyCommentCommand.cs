using ErrorOr;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteMyComment;

public record DeleteMyCommentCommand(string VoterKey, Guid CommentId) : IRequest<ErrorOr<Success>>;
