using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestPasswordReset;

public record RequestPasswordResetCommand(RequestPasswordResetRequest Request) : IRequest<ErrorOr<Success>>;
