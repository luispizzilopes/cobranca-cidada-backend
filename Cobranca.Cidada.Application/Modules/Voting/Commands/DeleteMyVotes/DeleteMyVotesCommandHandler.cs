using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Application.Modules.Voting.UseCases.DeleteMyVotes;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.DeleteMyVotes;

public class DeleteMyVotesCommandHandler : IRequestHandler<DeleteMyVotesCommand, ErrorOr<DeleteVotesResponse>>
{
    private readonly DeleteMyVotesUseCase _useCase;

    public DeleteMyVotesCommandHandler(DeleteMyVotesUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<DeleteVotesResponse>> Handle(DeleteMyVotesCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.VoterKey, command.ForgetVoterIdentity, cancellationToken);
}
