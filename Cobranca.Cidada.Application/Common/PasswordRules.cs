using FluentValidation;

namespace Cobranca.Cidada.Application.Common;

public static class PasswordRules
{
    /// <summary>Mesma política de senha do cadastro, reutilizada na recuperação de senha.</summary>
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(12).WithMessage("Senha deve ter pelo menos 12 caracteres.")
            .Matches("[A-Z]").WithMessage("Senha deve conter uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("Senha deve conter uma letra minúscula.")
            .Matches("[0-9]").WithMessage("Senha deve conter um número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter um caractere especial.");
}
