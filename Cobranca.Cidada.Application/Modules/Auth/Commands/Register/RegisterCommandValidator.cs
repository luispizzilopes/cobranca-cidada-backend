using Cobranca.Cidada.Application.Modules.Auth.Rules;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.Name).NotEmpty().WithMessage("Nome é obrigatório.").MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres.");
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
        RuleFor(x => x.Request.Password).MustBeStrongPassword();
        RuleFor(x => x.Request.Code).NotEmpty().Length(6).WithMessage("Código de verificação inválido.");
        RuleFor(x => x.Request.AcceptedPrivacyPolicy).Equal(true).WithMessage("É necessário aceitar a política de privacidade para criar a conta.");
    }
}
