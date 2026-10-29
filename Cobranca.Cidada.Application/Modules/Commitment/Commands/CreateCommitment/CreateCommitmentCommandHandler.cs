using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.CreateCommitment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.CreateCommitment;

public class CreateCommitmentCommandHandler : IRequestHandler<CreateCommitmentCommand, ErrorOr<AdminCommitmentDetailResponse>>
{
    private readonly CreateCommitmentUseCase _useCase;

    public CreateCommitmentCommandHandler(CreateCommitmentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> Handle(CreateCommitmentCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.ActorId, command.Request, cancellationToken);
}
