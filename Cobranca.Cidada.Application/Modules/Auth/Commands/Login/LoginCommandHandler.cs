using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.Login;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    private readonly LoginUseCase _useCase;

    public LoginCommandHandler(LoginUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, command.Metadata, cancellationToken);
}
