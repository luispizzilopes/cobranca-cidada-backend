using System.Text.RegularExpressions;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.CreateCommitment;

/// <summary>Valida o envelope. As regras do conteúdo ficam no <c>CommitmentInputValidator</c>, o mesmo do seed.</summary>
public partial class CreateCommitmentCommandValidator : AbstractValidator<CreateCommitmentCommand>
{
    public CreateCommitmentCommandValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor da criação ausente.");
        RuleFor(x => x.Request.AreaCode).NotEmpty().WithMessage("A área é obrigatória.");
        RuleFor(x => x.Request.OriginCode).NotEmpty().WithMessage("A origem é obrigatória.");
        RuleFor(x => x.Request.Slug)
            .Must(slug => string.IsNullOrWhiteSpace(slug) || (slug.Trim().Length <= 120 && SlugPattern().IsMatch(slug.Trim())))
            .WithMessage("O identificador público deve ter até 120 caracteres, só letras, números e hífens (ex.: ECON-050).");
    }

    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9-]*$")]
    private static partial Regex SlugPattern();
}
