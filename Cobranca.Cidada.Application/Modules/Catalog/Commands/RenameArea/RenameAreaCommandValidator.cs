using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameArea;

public class RenameAreaCommandValidator : AbstractValidator<RenameAreaCommand>
{
    public RenameAreaCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100).WithMessage("O código é inválido.");
        RuleFor(x => x.ActorId).NotEmpty().WithMessage("Autor da alteração ausente.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(200).WithMessage("O nome deve ter no máximo 200 caracteres.");
    }
}
