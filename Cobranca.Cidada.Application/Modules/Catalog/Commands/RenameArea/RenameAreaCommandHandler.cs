using ErrorOr;
using Cobranca.Cidada.Application.Modules.Catalog.UseCases.RenameArea;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameArea;

public class RenameAreaCommandHandler : IRequestHandler<RenameAreaCommand, ErrorOr<LookupResponse>>
{
    private readonly RenameAreaUseCase _useCase;

    public RenameAreaCommandHandler(RenameAreaUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<LookupResponse>> Handle(RenameAreaCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Code, command.ActorId, command.Name, cancellationToken);
}
