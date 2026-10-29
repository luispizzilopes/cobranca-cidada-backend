using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
    }
}
