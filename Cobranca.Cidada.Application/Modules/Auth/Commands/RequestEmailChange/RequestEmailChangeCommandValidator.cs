using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestEmailChange;

public class RequestEmailChangeCommandValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeCommandValidator()
    {
        RuleFor(x => x.Request.NewEmail).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Request.Password) || !string.IsNullOrWhiteSpace(x.Request.GoogleCredential))
            .WithName("Password")
            .WithMessage("Informe a senha para confirmar a troca de e-mail.");
        RuleFor(x => x.Request.GoogleCredential).MaximumLength(8192).WithMessage("Credencial do Google inválida.");
    }
}
