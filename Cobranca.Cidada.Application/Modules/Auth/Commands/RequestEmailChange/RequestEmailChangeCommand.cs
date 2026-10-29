using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestEmailChange;

public record RequestEmailChangeCommand(string UserId, RequestEmailChangeRequest Request) : IRequest<ErrorOr<Success>>;
