using Cobranca.Cidada.Application.Modules.Commitment.Responses;

namespace Cobranca.Cidada.Application.Modules.Catalog.Responses;

public record CatalogResponse(
    IReadOnlyList<LookupResponse> Areas,
    IReadOnlyList<LookupResponse> Origins,
    IReadOnlyList<StatusResponse> Statuses,
    VotingInfoResponse Voting);

public record VotingInfoResponse(int MinimumPublicVotes, string Disclaimer);
