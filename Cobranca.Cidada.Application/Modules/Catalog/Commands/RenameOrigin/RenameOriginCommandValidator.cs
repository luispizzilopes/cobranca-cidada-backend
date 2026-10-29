using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameOrigin;

public class RenameOriginCommandValidator : AbstractValidator<RenameOriginCommand>
{
    public RenameOriginCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100).WithMessage("O código é inválido.");
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor da alteração ausente.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");
    }
}
