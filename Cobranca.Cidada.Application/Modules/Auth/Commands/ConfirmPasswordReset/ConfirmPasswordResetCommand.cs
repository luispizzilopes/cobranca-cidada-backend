using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.ConfirmPasswordReset;

public record ConfirmPasswordResetCommand(ConfirmPasswordResetRequest Request) : IRequest<ErrorOr<Success>>;
