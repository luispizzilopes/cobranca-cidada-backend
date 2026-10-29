using Cobranca.Cidada.Domain.Modules.Commitment.Constants;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Queries.GetAdminCommitments;

public class GetAdminCommitmentsQueryValidator : AbstractValidator<GetAdminCommitmentsQuery>
{
    public static readonly string[] AllowedSorts = ["area", "code", "deadline"];

    public GetAdminCommitmentsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("O número da página deve ser maior ou igual a 1.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("O tamanho da página deve ser entre 1 e 100.");
        RuleFor(x => x.Search).MaximumLength(100).WithMessage("A busca deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || CommitmentStatusLabels.TryParse(status, out _) || Enum.TryParse<CommitmentStatus>(status, out _))
            .WithMessage("Situação inválida.");
        RuleFor(x => x.Sort)
            .Must(sort => string.IsNullOrEmpty(sort) || AllowedSorts.Contains(sort.ToLowerInvariant()))
            .WithMessage($"Ordenação inválida. Use: {string.Join(", ", AllowedSorts)}.");
    }
}
