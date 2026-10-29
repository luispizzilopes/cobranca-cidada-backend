using ErrorOr;
using Cobranca.Cidada.Application.Modules.Voting.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Voting.Commands.DeleteMyVotes;

/// <param name="ForgetVoterIdentity">Verdadeiro para quem votou só com e-mail: apaga também a verificação, o que revoga o token.</param>
public record DeleteMyVotesCommand(string VoterKey, bool ForgetVoterIdentity) : IRequest<ErrorOr<DeleteVotesResponse>>;
