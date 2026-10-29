using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.Request.Credential)
            .NotEmpty().WithMessage("Credencial do Google ausente.")
            .MaximumLength(8192).WithMessage("Credencial do Google inválida.");
    }
}
