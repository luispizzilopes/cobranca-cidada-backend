using ErrorOr;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using MediatR;

namespace Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments;

/// <summary>Carga inicial idempotente. Por padrão ignora IDs já existentes; só atualiza com Overwrite.</summary>
public record SeedCommitmentsCommand(
    SeedFileRequest File,
    bool Overwrite,
    DateOnly MandateStart,
    DateOnly MandateEnd,
    string Actor = "seed") : IRequest<ErrorOr<SeedReportResponse>>;
