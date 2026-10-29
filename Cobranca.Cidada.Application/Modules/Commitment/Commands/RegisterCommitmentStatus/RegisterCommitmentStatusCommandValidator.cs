using Cobranca.Cidada.Domain.Modules.Commitment.Constants;
using Cobranca.Cidada.Application.Modules.Commitment.Parsers;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.RegisterCommitmentStatus;

public class RegisterCommitmentStatusCommandValidator : AbstractValidator<RegisterCommitmentStatusCommand>
{
    public RegisterCommitmentStatusCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor do registro ausente.");
        RuleFor(x => x.Request.Status)
            .Must(status => CommitmentQueryParser.ParseStatus(status) is not null)
            .WithMessage($"Situação inválida. Valores permitidos: {string.Join("; ", CommitmentStatusLabels.AllLabels)}.");
        RuleFor(x => x.Request.ReferenceDate).NotEqual(default(DateOnly)).WithMessage("A data de referência é obrigatória.");
        RuleFor(x => x.Request.SourceUrl).MaximumLength(2000).WithMessage("A URL da fonte deve ter no máximo 2000 caracteres.");
        RuleFor(x => x.Request.Note).MaximumLength(2000).WithMessage("A observação deve ter no máximo 2000 caracteres.");
    }
}
