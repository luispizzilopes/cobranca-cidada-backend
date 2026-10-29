using Cobranca.Cidada.Application.Modules.Auth.Rules;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Request.CurrentPassword).NotEmpty().WithMessage("Informe a senha atual.");
        RuleFor(x => x.Request.NewPassword).MustBeStrongPassword();
        RuleFor(x => x.Request.NewPassword)
            .NotEqual(x => x.Request.CurrentPassword)
            .WithMessage("A nova senha deve ser diferente da atual.");
    }
}
