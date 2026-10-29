using System.Text.RegularExpressions;
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
using FluentValidation;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.CreateCommitment;

public partial class CreateCommitmentUseCase : IUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CommitmentInput> _validator;
    private readonly AdminCommitmentReader _reader;

    public CreateCommitmentUseCase(IUnitOfWork unitOfWork, IValidator<CommitmentInput> validator, AdminCommitmentReader reader)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _reader = reader;
    }

    public async Task<ErrorOr<AdminCommitmentDetailResponse>> ExecuteAsync(
        string actorId,
        CreateCommitmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var area = await _unitOfWork.AreaRepository.GetByCodeAsync(request.AreaCode.Trim().ToUpperInvariant(), cancellationToken);
        if (area is null)
            return Error.Validation("AreaCode", "Área inexistente.");

        var origin = await _unitOfWork.OriginRepository.GetByCodeAsync(request.OriginCode.Trim().ToLowerInvariant(), cancellationToken);
        if (origin is null)
            return Error.Validation("OriginCode", "Origem inexistente.");

        var mandate = await _unitOfWork.MandateRepository.GetCurrentAsync(cancellationToken);
        if (mandate is null)
            return Error.Failure("Mandate.NotFound", "Nenhum mandato cadastrado. Rode a carga inicial (seed) antes de criar compromissos.");

        var existingSlugs = await _unitOfWork.CommitmentRepository.GetExistingSlugsAsync(cancellationToken);
        var requested = CommitmentInputNormalizer.Blank(request.Slug);
        var slug = requested ?? NextSlug(area.Code, existingSlugs);
        if (existingSlugs.Contains(slug))
            return Error.Conflict("Commitment.SlugInUse", $"O identificador público '{slug}' já está em uso por outro compromisso.");

        var input = new CommitmentInput(
            ExternalId: null,
            Slug: slug,
            AreaCode: area.Code,
            AreaName: area.Name,
            OriginName: origin.Name,
            Title: request.Title?.Trim() ?? string.Empty,
            Description: request.Description?.Trim() ?? string.Empty,
            ShortDescription: CommitmentInputNormalizer.Blank(request.ShortDescription),
            HowItWillBeDone: CommitmentInputNormalizer.Blank(request.HowItWillBeDone),
            HasPlanDetail: request.HasPlanDetail,
            Date: request.Date,
            DateKind: CommitmentInputNormalizer.Blank(request.DateKind),
            DeadlineText: CommitmentInputNormalizer.Blank(request.DeadlineText),
            DeadlineDate: request.DeadlineDate,
            GoalOrCondition: CommitmentInputNormalizer.Blank(request.GoalOrCondition),
            Classification: CommitmentInputNormalizer.Clean(request.Classification),
            Evidence: CommitmentInputNormalizer.Clean(request.Evidence),
            PlanPage: CommitmentInputNormalizer.Blank(request.PlanPage),
            Sources: CommitmentInputNormalizer.Sources(request.Sources),
            InitialStatus: null);

        var validation = await _validator.ValidateAsync(input, cancellationToken);
        if (!validation.IsValid)
            return validation.Errors.Select(e => Error.Validation(e.PropertyName, e.ErrorMessage)).DistinctBy(e => e.Description).ToList();

        CommitmentAggregate commitment;
        try
        {
            commitment = CommitmentAggregate.Create(null, slug, mandate.Id, area.Id, origin.Id, input.ToContent(), actorId, request.Publish);
        }
        catch (DomainException ex)
        {
            return Error.Validation("Commitment", ex.Message);
        }

        await _unitOfWork.CommitmentRepository.CreateAsync(commitment, cancellationToken);
        await _unitOfWork.AuditLogRepository.CreateAsync(
            AuditLog.Create(actorId, AuditActions.CommitmentCreated, nameof(CommitmentAggregate), slug, "Criado pelo painel."),
            cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return await _reader.GetAsync(slug, cancellationToken);
    }

    /// <summary>Próximo identificador da área: maior número já usado no prefixo (ex.: ECON-049) mais um.</summary>
    internal static string NextSlug(string areaCode, IEnumerable<string> existingSlugs)
    {
        var prefix = areaCode.ToUpperInvariant() + "-";
        var highest = 0;
        foreach (var slug in existingSlugs)
        {
            if (!slug.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var match = NumberSuffix().Match(slug[prefix.Length..]);
            if (match.Success && int.TryParse(match.Value, out var number))
                highest = Math.Max(highest, number);
        }

        return $"{prefix}{highest + 1:000}";
    }

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex NumberSuffix();
}
