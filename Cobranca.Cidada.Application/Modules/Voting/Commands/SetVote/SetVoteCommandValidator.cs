using Cobranca.Cidada.Application.Modules.Voting.Policies;
using FluentValidation;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.SetVote;

public class SetVoteCommandValidator : AbstractValidator<SetVoteCommand>
{
    public SetVoteCommandValidator()
    {
        RuleFor(x => x.VoterKey).NotEmpty().WithMessage("Identidade do votante ausente.");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120).WithMessage("O identificador do compromisso é inválido.");
        RuleFor(x => x.Request.Rating)
            .Must(rating => VotePolicy.TryParseRating(rating, out _))
            .WithMessage("Avaliação inválida. Use \"boa\", \"ma\" ou deixe em branco.");
    }
}
