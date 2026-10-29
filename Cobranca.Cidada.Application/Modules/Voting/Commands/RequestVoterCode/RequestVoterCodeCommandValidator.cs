using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.RequestVoterCode;

public class RequestVoterCodeCommandValidator : AbstractValidator<RequestVoterCodeCommand>
{
    public RequestVoterCodeCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
    }
}
