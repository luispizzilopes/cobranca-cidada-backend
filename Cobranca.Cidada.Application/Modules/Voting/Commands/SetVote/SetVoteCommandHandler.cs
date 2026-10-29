using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Application.Modules.Voting.UseCases.SetVote;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.SetVote;

public class SetVoteCommandHandler : IRequestHandler<SetVoteCommand, ErrorOr<VoteStateResponse>>
{
    private readonly SetVoteUseCase _useCase;

    public SetVoteCommandHandler(SetVoteUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<VoteStateResponse>> Handle(SetVoteCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.VoterKey, command.Slug, command.Request, command.ClientIp, cancellationToken);
}
