using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.RegisterCommitmentStatus;

public class RegisterCommitmentStatusUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AdminCommitmentReader _reader;
    private readonly TimeProvider _timeProvider;

    public RegisterCommitmentStatusUseCase(IUnitOfWork unitOfWork, AdminCommitmentReader reader, TimeProvider timeProvider)
    {
        _unitOfWork = unitOfWork;
        _reader = reader;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> ExecuteAsync(
        string slug,
        string actorId,
        RegisterStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var commitment = await _unitOfWork.CommitmentRepository.GetForUpdateAsync(slug.Trim(), cancellationToken);
        if (commitment is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        var status = CommitmentQueryParser.ParseStatus(request.Status)!.Value;
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            commitment.RegisterStatus(status, request.ReferenceDate, request.SourceUrl, request.Note, request.IsFormalAct, actorId, today);
        }
        catch (DomainException ex)
        {
            return Error.Validation("Status", ex.Message);
        }

        await _unitOfWork.AuditLogRepository.CreateAsync(
            AuditLog.Create(actorId, AuditActions.StatusRegistered, nameof(CommitmentAggregate), commitment.Slug, status.ToString()),
            cancellationToken);

        try
        {
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Error.Conflict("Commitment.Concurrent", "O compromisso foi alterado ao mesmo tempo por outra pessoa. Recarregue e tente de novo.");
        }

        return await _reader.GetAsync(commitment.Slug, cancellationToken);
    }
}
