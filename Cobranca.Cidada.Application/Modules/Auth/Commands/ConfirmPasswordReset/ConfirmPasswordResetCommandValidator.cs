using Cobranca.Cidada.Application.Common;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmPasswordReset;

public class ConfirmPasswordResetCommandValidator : AbstractValidator<ConfirmPasswordResetCommand>
{
    public ConfirmPasswordResetCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
        RuleFor(x => x.Request.Code).NotEmpty().Matches("^[0-9]{6}$").WithMessage("Código de verificação inválido.");
        RuleFor(x => x.Request.NewPassword).MustBeStrongPassword();
    }
}
