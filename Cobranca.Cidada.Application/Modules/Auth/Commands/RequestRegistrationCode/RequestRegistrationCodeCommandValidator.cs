using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestRegistrationCode;

public class RequestRegistrationCodeCommandValidator : AbstractValidator<RequestRegistrationCodeCommand>
{
    public RequestRegistrationCodeCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
    }
}
