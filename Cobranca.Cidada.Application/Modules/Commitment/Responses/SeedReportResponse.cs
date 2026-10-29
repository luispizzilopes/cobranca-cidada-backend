namespace Cobranca.Cidada.Application.Modules.Commitment.Responses;

public record SeedRejection(string Id, IReadOnlyList<string> Reasons);

public record SeedReportResponse(
    int Total,
    int Created,
    int Overwritten,
    int Skipped,
    IReadOnlyList<SeedRejection> Rejected,
    int AreasCreated,
    int OriginsCreated);
