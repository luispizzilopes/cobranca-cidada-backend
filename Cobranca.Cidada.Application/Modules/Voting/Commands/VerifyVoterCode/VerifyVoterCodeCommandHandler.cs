using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using Cobranca.Cidada.Application.Modules.Voting.UseCases.VerifyVoterCode;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.VerifyVoterCode;

public class VerifyVoterCodeCommandHandler : IRequestHandler<VerifyVoterCodeCommand, ErrorOr<VoterTokenResponse>>
{
    private readonly VerifyVoterCodeUseCase _useCase;

    public VerifyVoterCodeCommandHandler(VerifyVoterCodeUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<VoterTokenResponse>> Handle(VerifyVoterCodeCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, cancellationToken);
}
