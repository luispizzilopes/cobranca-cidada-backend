using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Comment.Queries.GetComments;

public class GetCommentsQueryValidator : AbstractValidator<GetCommentsQuery>
{
    public GetCommentsQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("O número da página deve ser maior ou igual a 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage("O tamanho da página deve ser entre 1 e 50.");
    }
}
