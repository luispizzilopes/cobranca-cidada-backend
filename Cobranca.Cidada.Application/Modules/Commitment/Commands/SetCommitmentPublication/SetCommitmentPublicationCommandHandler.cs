using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.UseCases.SetCommitmentPublication;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.SetCommitmentPublication;

public class SetCommitmentPublicationCommandHandler : IRequestHandler<SetCommitmentPublicationCommand, ErrorOr<AdminCommitmentDetailResponse>>
{
    private readonly SetCommitmentPublicationUseCase _useCase;

    public SetCommitmentPublicationCommandHandler(SetCommitmentPublicationUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> Handle(SetCommitmentPublicationCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Slug, command.ActorId, command.Published, cancellationToken);
}
