using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
