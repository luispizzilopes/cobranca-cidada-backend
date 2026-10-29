using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.VerifyVoterCode;

public record VerifyVoterCodeCommand(VerifyVoterCodeRequest Request) : IRequest<ErrorOr<VoterTokenResponse>>;
