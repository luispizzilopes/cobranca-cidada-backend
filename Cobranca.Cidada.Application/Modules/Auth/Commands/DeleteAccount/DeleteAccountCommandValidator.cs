using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.DeleteAccount;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Request.Password) || !string.IsNullOrWhiteSpace(x.Request.GoogleCredential))
            .WithName("Password")
            .WithMessage("Informe a senha para confirmar a exclusão.");
        RuleFor(x => x.Request.GoogleCredential).MaximumLength(8192).WithMessage("Credencial do Google inválida.");
    }
}
