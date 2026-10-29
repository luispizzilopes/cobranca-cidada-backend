using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.DeleteMyComment;

public class DeleteMyCommentCommandValidator : AbstractValidator<DeleteMyCommentCommand>
{
    public DeleteMyCommentCommandValidator()
    {
        RuleFor(x => x.VoterKey).NotEmpty().WithMessage("Identidade do votante ausente.");
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("O identificador do comentário é inválido.");
    }
}
