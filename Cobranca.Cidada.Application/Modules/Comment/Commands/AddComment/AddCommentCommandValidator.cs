using FluentValidation;
using CommentAggregate = Cobranca.Cidada.Domain.Modules.Comment.Aggregates.Comment;

namespace Cobranca.Cidada.Application.Modules.Comment.Commands.AddComment;

public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.VoterKey).NotEmpty().WithMessage("Identidade do votante ausente.");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
        RuleFor(x => x.Request.Text)
            .NotEmpty().WithMessage("Escreva o comentário.")
            .Must(text => (text?.Trim().Length ?? 0) >= CommentAggregate.MinLength)
            .WithMessage($"O comentário deve ter ao menos {CommentAggregate.MinLength} caracteres.")
            .Must(text => (text?.Trim().Length ?? 0) <= CommentAggregate.MaxLength)
            .WithMessage($"O comentário deve ter no máximo {CommentAggregate.MaxLength} caracteres.");
    }
}
