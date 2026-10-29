using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteComment;

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("O identificador do comentário é inválido.");
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor da ação ausente.");
    }
}
