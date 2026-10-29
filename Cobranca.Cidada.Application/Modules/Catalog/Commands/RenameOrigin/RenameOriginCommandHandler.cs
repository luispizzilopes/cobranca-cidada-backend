using ErrorOr;
using Cobranca.Cidada.Application.Modules.Catalog.UseCases.RenameOrigin;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Catalog.Commands.RenameOrigin;

public class RenameOriginCommandHandler : IRequestHandler<RenameOriginCommand, ErrorOr<LookupResponse>>
{
    private readonly RenameOriginUseCase _useCase;

    public RenameOriginCommandHandler(RenameOriginUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<LookupResponse>> Handle(RenameOriginCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Code, command.ActorId, command.Name, cancellationToken);
}
