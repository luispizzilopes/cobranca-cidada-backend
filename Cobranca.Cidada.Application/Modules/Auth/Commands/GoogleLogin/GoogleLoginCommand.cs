using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(GoogleLoginRequest Request, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
