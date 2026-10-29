using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.UpdateCommitment;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.UpdateCommitment;

public class UpdateCommitmentCommandHandler : IRequestHandler<UpdateCommitmentCommand, ErrorOr<AdminCommitmentDetailResponse>>
{
    private readonly UpdateCommitmentUseCase _useCase;

    public UpdateCommitmentCommandHandler(UpdateCommitmentUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> Handle(UpdateCommitmentCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Slug, command.ActorId, command.Request, cancellationToken);
}
