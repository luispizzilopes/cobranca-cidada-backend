namespace Cobranca.Cidada.Domain.Modules.Commitment.Enums;

public enum CommitmentStatus
{
    NotEvaluated = 0,
    NoEvidenceFound = 1,
    InPreparation = 2,
    InProgress = 3,
    PartiallyFulfilled = 4,
    Fulfilled = 5,
    Unfulfilled = 6,
    ChangedOrReplaced = 7,
    ImpairedOrConditioned = 8
}
