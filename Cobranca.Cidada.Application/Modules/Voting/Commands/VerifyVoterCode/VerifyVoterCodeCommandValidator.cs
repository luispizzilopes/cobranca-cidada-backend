using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.VerifyVoterCode;

public class VerifyVoterCodeCommandValidator : AbstractValidator<VerifyVoterCodeCommand>
{
    public VerifyVoterCodeCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().MaximumLength(254).WithMessage("E-mail inválido.");
        RuleFor(x => x.Request.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$").WithMessage("Código de verificação inválido.");
    }
}
