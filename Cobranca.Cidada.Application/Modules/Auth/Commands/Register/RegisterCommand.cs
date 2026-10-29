using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
