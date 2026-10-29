using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;

public class SeedCommitmentsCommandValidator : AbstractValidator<SeedCommitmentsCommand>
{
    public SeedCommitmentsCommandValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("O arquivo de carga é obrigatório.");
        RuleFor(x => x.File.Commitments).NotEmpty().WithMessage("O arquivo de carga não contém compromissos.");
        RuleFor(x => x.File.Metadata.Candidate).NotEmpty().WithMessage("O arquivo de carga precisa informar o candidato em 'metadados.candidato'.");
        RuleFor(x => x.MandateEnd).GreaterThan(x => x.MandateStart).WithMessage("O fim do mandato deve ser posterior ao início.");
        RuleFor(x => x.Actor).NotEmpty().WithMessage("O autor da carga é obrigatório.");
    }
}
