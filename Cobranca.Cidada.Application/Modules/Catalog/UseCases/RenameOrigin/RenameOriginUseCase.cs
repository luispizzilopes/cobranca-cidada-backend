using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;

namespace Cobranca.Cidada.Application.Modules.Catalog.UseCases.RenameOrigin;

public class RenameOriginUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public RenameOriginUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<LookupResponse>> ExecuteAsync(string code, string actorId, string name, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.OriginRepository.GetByCodeAsync(code.Trim().ToLowerInvariant(), cancellationToken);
        if (item is null)
            return Error.NotFound("Catalog.OriginNotFound", "A origem não foi encontrada.");

        var previous = item.Name;
        item.Rename(name);

        await _unitOfWork.AuditLogRepository.CreateAsync(
            AuditLog.Create(actorId, AuditActions.OriginRenamed, nameof(Origin), item.Code, $"{previous} -> {item.Name}"),
            cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new LookupResponse(item.Code, item.Name);
    }
}
