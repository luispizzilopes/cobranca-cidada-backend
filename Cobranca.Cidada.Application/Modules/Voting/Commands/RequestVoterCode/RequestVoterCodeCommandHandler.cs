using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.UseCases.RequestVoterCode;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.RequestVoterCode;

public class RequestVoterCodeCommandHandler : IRequestHandler<RequestVoterCodeCommand, ErrorOr<Success>>
{
    private readonly RequestVoterCodeUseCase _useCase;

    public RequestVoterCodeCommandHandler(RequestVoterCodeUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(RequestVoterCodeCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, cancellationToken);
}
