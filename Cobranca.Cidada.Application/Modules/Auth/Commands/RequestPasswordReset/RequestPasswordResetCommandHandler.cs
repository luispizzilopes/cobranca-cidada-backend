using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestPasswordReset;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestPasswordReset;

public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommand, ErrorOr<Success>>
{
    private readonly RequestPasswordResetUseCase _useCase;

    public RequestPasswordResetCommandHandler(RequestPasswordResetUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(RequestPasswordResetCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, cancellationToken);
}
