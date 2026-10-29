using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.SetCommitmentPublication;

public class SetCommitmentPublicationUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AdminCommitmentReader _reader;

    public SetCommitmentPublicationUseCase(IUnitOfWork unitOfWork, AdminCommitmentReader reader)
    {
        _unitOfWork = unitOfWork;
        _reader = reader;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> ExecuteAsync(
        string slug,
        string actorId,
        bool published,
        CancellationToken cancellationToken = default)
    {
        var commitment = await _unitOfWork.CommitmentRepository.GetForUpdateAsync(slug.Trim(), cancellationToken);
        if (commitment is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        if (commitment.IsPublished != published)
        {
            if (published) commitment.Publish();
            else commitment.Unpublish();

            await _unitOfWork.AuditLogRepository.CreateAsync(
                AuditLog.Create(actorId, published ? AuditActions.CommitmentPublished : AuditActions.CommitmentUnpublished, nameof(CommitmentAggregate), commitment.Slug),
                cancellationToken);

            try
            {
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (ConcurrencyConflictException)
            {
                return Error.Conflict("Commitment.Concurrent", "O compromisso foi alterado ao mesmo tempo por outra pessoa. Recarregue e tente de novo.");
            }
        }

        return await _reader.GetAsync(commitment.Slug, cancellationToken);
    }
}
