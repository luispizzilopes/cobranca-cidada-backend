using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeCommandValidator : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeCommandValidator()
    {
        RuleFor(x => x.Request.NewEmail).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
        RuleFor(x => x.Request.Code).NotEmpty().Matches("^[0-9]{6}$").WithMessage("Código de verificação inválido.");
    }
}
