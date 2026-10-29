using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.UseCases.GoogleLogin;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ErrorOr<LoginResponse>>
{
    private readonly GoogleLoginUseCase _useCase;

    public GoogleLoginCommandHandler(GoogleLoginUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<ErrorOr<LoginResponse>> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
        => await _useCase.ExecuteAsync(command.Request, command.Metadata, cancellationToken);
}
