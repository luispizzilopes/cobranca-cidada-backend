using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.UpdateCommitment;

/// <summary>Valida o envelope da edição. As regras do conteúdo ficam no <c>CommitmentInputValidator</c>, o mesmo do seed.</summary>
public class UpdateCommitmentCommandValidator : AbstractValidator<UpdateCommitmentCommand>
{
    public UpdateCommitmentCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor da edição ausente.");
        RuleFor(x => x.Request.ExpectedVersion).GreaterThanOrEqualTo(1).WithMessage("Informe a versão do compromisso que está sendo editada.");
        RuleFor(x => x.Request.Reason).NotEmpty().WithMessage("Informe o motivo da edição.")
            .MaximumLength(500).WithMessage("O motivo deve ter no máximo 500 caracteres.");
        RuleFor(x => x.Request.AreaCode).NotEmpty().WithMessage("A área é obrigatória.");
        RuleFor(x => x.Request.OriginCode).NotEmpty().WithMessage("A origem é obrigatória.");
    }
}
