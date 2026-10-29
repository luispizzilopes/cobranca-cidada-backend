using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmEmailChange;

public record ConfirmEmailChangeCommand(string UserId, ConfirmEmailChangeRequest Request) : IRequest<ErrorOr<Success>>;
