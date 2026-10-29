using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Models;
using Cobranca.Cidada.Application.Modules.Auth.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RefreshSession;

public record RefreshSessionCommand(string RefreshToken, SessionMetadata Metadata) : IRequest<ErrorOr<LoginResponse>>;
