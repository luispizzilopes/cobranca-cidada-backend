using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestEmailChange;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestEmailChange;

public class RequestEmailChangeCommandHandler : IRequestHandler<RequestEmailChangeCommand, ErrorOr<Success>>
{
    private readonly RequestEmailChangeUseCase _useCase;

    public RequestEmailChangeCommandHandler(RequestEmailChangeUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(RequestEmailChangeCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.UserId, command.Request, cancellationToken);
}
