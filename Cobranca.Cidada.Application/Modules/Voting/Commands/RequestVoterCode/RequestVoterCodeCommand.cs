using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.RequestVoterCode;

public record RequestVoterCodeCommand(RequestVoterCodeRequest Request) : IRequest<ErrorOr<Success>>;
