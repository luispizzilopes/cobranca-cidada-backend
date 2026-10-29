using ErrorOr;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteComment;

/// <summary>Remoção de comentário por moderação (administrador).</summary>
public record DeleteCommentCommand(Guid CommentId, string ActorId) : IRequest<ErrorOr<Success>>;
