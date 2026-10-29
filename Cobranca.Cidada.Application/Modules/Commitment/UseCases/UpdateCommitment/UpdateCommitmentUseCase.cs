using ErrorOr;
using Cobranca.Cidada.Application.Common;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Application.Modules.Commitment.Services;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.Rules;
using FluentValidation;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.UpdateCommitment;

public class UpdateCommitmentUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CommitmentInput> _validator;
    private readonly AdminCommitmentReader _reader;
    private readonly TimeProvider _timeProvider;

    public UpdateCommitmentUseCase(
        IUnitOfWork unitOfWork,
        IValidator<CommitmentInput> validator,
        AdminCommitmentReader reader,
        TimeProvider timeProvider)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _reader = reader;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> ExecuteAsync(
        string slug,
        string actorId,
        UpdateCommitmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var commitment = await _unitOfWork.CommitmentRepository.GetForUpdateAsync(slug.Trim(), cancellationToken);
        if (commitment is null)
            return Error.NotFound("Commitment.NotFound", "Compromisso não encontrado.");

        if (commitment.Version != request.ExpectedVersion)
            return VersionConflict();

        var area = await _unitOfWork.AreaRepository.GetByCodeAsync(request.AreaCode.Trim().ToUpperInvariant(), cancellationToken);
        if (area is null)
            return Error.Validation("AreaCode", "Área inexistente.");

        var origin = await _unitOfWork.OriginRepository.GetByCodeAsync(request.OriginCode.Trim().ToLowerInvariant(), cancellationToken);
        if (origin is null)
            return Error.Validation("OriginCode", "Origem inexistente.");

        var input = ToInput(request, commitment, area.Name, origin.Name);
        var validation = await _validator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
            return validation.Errors.Select(e => Error.Validation(e.PropertyName, e.ErrorMessage)).DistinctBy(e => e.Description).ToList();

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        if (StillValidWithDeadline(commitment, input.DeadlineDate, today) is { } violation)
            return Error.Validation("DeadlineDate", violation);

        try
        {
            commitment.Update(input.ToContent(), area.Id, origin.Id, actorId, request.Reason);
        }
        catch (DomainException ex)
        {
            return Error.Validation("Commitment", ex.Message);
        }

        await _unitOfWork.AuditLogRepository.CreateAsync(
            AuditLog.Create(actorId, AuditActions.CommitmentUpdated, nameof(CommitmentAggregate), commitment.Slug, request.Reason.Trim()),
            cancellationToken);

        try
        {
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return VersionConflict();
        }

        return await _reader.GetAsync(commitment.Slug, cancellationToken);
    }

    /// <summary>
    /// Mover ou apagar o prazo não pode deixar um 'Descumprido' que já estava registrado sem sustentação.
    /// Reaplica a regra de situação à entrada que definiu a situação atual, com o prazo novo.
    /// </summary>
    private static string? StillValidWithDeadline(CommitmentAggregate commitment, DateOnly? newDeadline, DateOnly today)
    {
        if (commitment.CurrentStatus != CommitmentStatus.Unfulfilled)
            return null;

        var current = commitment.StatusEntries.OrderByDescending(e => e.CreatedAtUtc).FirstOrDefault();
        if (current is null)
            return null;

        var violation = StatusRules.Check(current.Status, current.ReferenceDate, current.SourceUrl, newDeadline, current.IsFormalAct, today);
        return violation is null
            ? null
            : $"A situação atual é 'Descumprido' e deixaria de ser válida com este prazo: {violation} Registre antes outra situação.";
    }

    private static CommitmentInput ToInput(UpdateCommitmentRequest r, CommitmentAggregate commitment, string areaName, string originName) => new(
        ExternalId: commitment.ExternalId,
        Slug: commitment.Slug,
        AreaCode: r.AreaCode.Trim().ToUpperInvariant(),
        AreaName: areaName,
        OriginName: originName,
        Title: r.Title?.Trim() ?? string.Empty,
        Description: r.Description?.Trim() ?? string.Empty,
        ShortDescription: CommitmentInputNormalizer.Blank(r.ShortDescription),
        HowItWillBeDone: CommitmentInputNormalizer.Blank(r.HowItWillBeDone),
        HasPlanDetail: r.HasPlanDetail,
        Date: r.Date,
        DateKind: CommitmentInputNormalizer.Blank(r.DateKind),
        DeadlineText: CommitmentInputNormalizer.Blank(r.DeadlineText),
        DeadlineDate: r.DeadlineDate,
        GoalOrCondition: CommitmentInputNormalizer.Blank(r.GoalOrCondition),
        Classification: CommitmentInputNormalizer.Clean(r.Classification),
        Evidence: CommitmentInputNormalizer.Clean(r.Evidence),
        PlanPage: CommitmentInputNormalizer.Blank(r.PlanPage),
        Sources: CommitmentInputNormalizer.Sources(r.Sources),
        InitialStatus: null);

    private static Error VersionConflict()
        => Error.Conflict("Commitment.VersionConflict", "Este compromisso foi alterado por outra pessoa depois que você abriu a edição. Recarregue para ver a versão atual.");
}
