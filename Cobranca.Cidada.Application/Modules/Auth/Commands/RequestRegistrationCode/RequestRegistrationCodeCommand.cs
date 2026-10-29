using ErrorOr;
using Cobranca.Cidada.Application.Modules.Auth.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Auth.Commands.RequestRegistrationCode;

public record RequestRegistrationCodeCommand(RequestRegistrationCodeRequest Request) : IRequest<ErrorOr<Success>>;
