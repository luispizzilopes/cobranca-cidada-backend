using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.RequestRegistrationCode;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestRegistrationCode;

public class RequestRegistrationCodeCommandHandler : IRequestHandler<RequestRegistrationCodeCommand, ErrorOr<Success>>
{
    private readonly RequestRegistrationCodeUseCase _useCase;

    public RequestRegistrationCodeCommandHandler(RequestRegistrationCodeUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<Success>> Handle(RequestRegistrationCodeCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, cancellationToken);
}
