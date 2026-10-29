using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;

namespace Cobranca.Cidada.Application.Modules.Catalog.UseCases.RenameArea;

public class RenameAreaUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public RenameAreaUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<LookupResponse>> ExecuteAsync(string code, string actorId, string name, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.AreaRepository.GetByCodeAsync(code.Trim().ToUpperInvariant(), cancellationToken);
        if (item is null)
            return Error.NotFound("Catalog.AreaNotFound", "A área não foi encontrada.");

        var previous = item.Name;
        item.Rename(name);

        await _unitOfWork.AuditLogRepository.CreateAsync(
            AuditLog.Create(actorId, AuditActions.AreaRenamed, nameof(Area), item.Code, $"{previous} -> {item.Name}"),
            cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new LookupResponse(item.Code, item.Name);
    }
}
