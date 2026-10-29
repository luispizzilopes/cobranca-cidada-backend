using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().WithMessage("E-mail inválido.");
        RuleFor(x => x.Request.Password).NotEmpty().WithMessage("Senha é obrigatória.");
    }
}
