using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.Register;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<LoginResponse>>
{
    private readonly RegisterUseCase _useCase;

    public RegisterCommandHandler(RegisterUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, command.Metadata, cancellationToken);
}
