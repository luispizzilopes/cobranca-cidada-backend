using ErrorOr;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Logout;

public record LogoutCommand(string? RefreshToken) : IRequest<ErrorOr<Success>>;
