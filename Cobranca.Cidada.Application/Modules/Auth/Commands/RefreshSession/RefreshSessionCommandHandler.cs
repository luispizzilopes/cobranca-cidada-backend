using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using Cobranca.Cidada.Application.Modules.Auth.Services;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler(AuthSessionService sessionService)
    : IRequestHandler<RefreshSessionCommand, ErrorOr<LoginResponse>>
{
    public Task<ErrorOr<LoginResponse>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
        => sessionService.RefreshAsync(command.RefreshToken, command.Metadata, cancellationToken);
}
