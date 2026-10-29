using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Requests;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.SetVote;

public record SetVoteCommand(string VoterKey, string Slug, SetVoteRequest Request, string? ClientIp = null) : IRequest<ErrorOr<VoteStateResponse>>;
