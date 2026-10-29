using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.RegisterCommitmentStatus;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.RegisterCommitmentStatus;

public class RegisterCommitmentStatusCommandHandler : IRequestHandler<RegisterCommitmentStatusCommand, ErrorOr<AdminCommitmentDetailResponse>>
{
    private readonly RegisterCommitmentStatusUseCase _useCase;

    public RegisterCommitmentStatusCommandHandler(RegisterCommitmentStatusUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> Handle(RegisterCommitmentStatusCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Slug, command.ActorId, command.Request, cancellationToken);
}
