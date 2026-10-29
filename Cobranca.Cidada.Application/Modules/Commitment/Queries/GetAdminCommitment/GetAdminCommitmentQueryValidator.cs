using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitment;

public class GetAdminCommitmentQueryValidator : AbstractValidator<GetAdminCommitmentQuery>
{
    public GetAdminCommitmentQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage("O identificador do compromisso é obrigatório.")
            .MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
    }
}
