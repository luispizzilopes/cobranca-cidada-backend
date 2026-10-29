using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetCommitmentBySlug;

public class GetCommitmentBySlugQueryValidator : AbstractValidator<GetCommitmentBySlugQuery>
{
    public GetCommitmentBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage("O identificador do compromisso é obrigatório.")
            .MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
    }
}
