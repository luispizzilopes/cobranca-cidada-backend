using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.SetCommitmentPublication;

public class SetCommitmentPublicationCommandValidator : AbstractValidator<SetCommitmentPublicationCommand>
{
    public SetCommitmentPublicationCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor da ação ausente.");
    }
}
