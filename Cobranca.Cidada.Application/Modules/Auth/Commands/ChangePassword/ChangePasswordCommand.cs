using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(string UserId, ChangePasswordRequest Request) : IRequest<ErrorOr<Success>>;
