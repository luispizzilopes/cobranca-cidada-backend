using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Comment.Queries.GetRecentComments;

public class GetRecentCommentsQueryValidator : AbstractValidator<GetRecentCommentsQuery>
{
    public GetRecentCommentsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 12).WithMessage("O limite deve ser entre 1 e 12.");
    }
}
