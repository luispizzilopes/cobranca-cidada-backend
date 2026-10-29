using Cobranca.Cidada.Domain.Modules.Commitment.Enums;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Filters;

public enum CommitmentSort
{
    Area = 0,
    Code = 1,
    Deadline = 2,
    MostCharged = 3,
    BestRated = 4,
    WorstRated = 5
}

public record CommitmentFilters(
    string? Search,
    string? AreaCode,
    string? OriginCode,
    CommitmentStatus? Status,
    bool? OnlyWithDeadline,
    CommitmentSort Sort = CommitmentSort.Area,
    string? OnlyChargedByVoterKey = null);
