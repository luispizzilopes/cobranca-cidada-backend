using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.DeleteAccount;

public record DeleteAccountCommand(string UserId, DeleteAccountRequest Request) : IRequest<ErrorOr<Success>>;
