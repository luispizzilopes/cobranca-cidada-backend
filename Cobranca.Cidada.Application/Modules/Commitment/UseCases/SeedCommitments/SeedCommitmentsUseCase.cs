using System.Text.RegularExpressions;
using ErrorOr;
using Cobranca.Cidada.Application.Interfaces.UseCases;
using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Application.Modules.Commitment.Responses;
using Cobranca.Cidada.Domain.Interfaces.UnitOfWork;
using Cobranca.Cidada.Domain.Modules.Audit;
using Cobranca.Cidada.Domain.Modules.Audit.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.Constants;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Mandate.Entities;
using Cobranca.Cidada.Application.Modules.Commitment.Mappers;
using Cobranca.Cidada.Application.Modules.Commitment.Normalizers;
using FluentValidation;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;
using MandateAggregate = Cobranca.Cidada.Domain.Modules.Mandate.Aggregates.Mandate;

namespace Cobranca.Cidada.Application.Modules.Commitment.UseCases.SeedCommitments;

public partial class SeedCommitmentsUseCase : IUseCase
{
    private const string OfficialPlanOriginCode = "plano-de-governo";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CommitmentInput> _validator;
    private readonly TimeProvider _timeProvider;

    public SeedCommitmentsUseCase(IUnitOfWork unitOfWork, IValidator<CommitmentInput> validator, TimeProvider timeProvider)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<SeedReportResponse>> ExecuteAsync(
        Cobranca.Cidada.Application.Modules.Commitment.Commands.SeedCommitments.SeedCommitmentsCommand command,
        CancellationToken cancellationToken = default)
    {
        var file = command.File;
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var mandate = await EnsureMandateAsync(file.Metadata.Candidate!, command.MandateStart, command.MandateEnd, cancellationToken);

        var areas = (await _unitOfWork.AreaRepository.GetAllAsync(cancellationToken)).ToDictionary(a => a.Code);
        var origins = (await _unitOfWork.OriginRepository.GetAllAsync(cancellationToken)).ToDictionary(o => o.Code);
        var existingSlugs = await _unitOfWork.CommitmentRepository.GetExistingSlugsAsync(cancellationToken);
        var existingIds = await _unitOfWork.CommitmentRepository.GetExistingExternalIdsAsync(
            file.Commitments.Select(c => c.Id).Where(id => !string.IsNullOrWhiteSpace(id))!, cancellationToken);

        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        var rejected = new List<SeedRejection>();
        int created = 0, overwritten = 0, skipped = 0, areasCreated = 0, originsCreated = 0;

        foreach (var item in file.Commitments)
        {
            var label = string.IsNullOrWhiteSpace(item.Id) ? "(sem id)" : item.Id!;

            if (string.IsNullOrWhiteSpace(item.Id))
            {
                rejected.Add(new SeedRejection(label, ["O campo 'id' é obrigatório."]));
                continue;
            }

            if (!seenIds.Add(item.Id))
            {
                rejected.Add(new SeedRejection(label, ["ID duplicado no arquivo."]));
                continue;
            }

            var input = SeedCommitmentMapper.ToInput(item);
            var validation = await _validator.ValidateAsync(input, cancellationToken);
            if (!validation.IsValid)
            {
                rejected.Add(new SeedRejection(label, validation.Errors.Select(e => e.ErrorMessage).Distinct().ToList()));
                continue;
            }

            var alreadyExists = existingIds.Contains(item.Id);
            if (alreadyExists && !command.Overwrite)
            {
                skipped++;
                continue;
            }

            var (area, areaIsNew) = await GetOrCreateAreaAsync(areas, input, cancellationToken);
            var (origin, originIsNew) = await GetOrCreateOriginAsync(origins, input, cancellationToken);
            if (areaIsNew) areasCreated++;
            if (originIsNew) originsCreated++;

            var content = input.ToContent();

            if (alreadyExists)
            {
                var existing = await _unitOfWork.CommitmentRepository.GetByExternalIdAsync(item.Id, cancellationToken);
                existing!.Update(content, area.Id, origin.Id, command.Actor, "Reimportação do arquivo de carga (--overwrite).");
                await _unitOfWork.CommitmentRepository.UpdateAsync(existing, cancellationToken);
                await _unitOfWork.AuditLogRepository.CreateAsync(
                    AuditLog.Create(command.Actor, AuditActions.SeedOverwrite, nameof(CommitmentAggregate), existing.Slug), cancellationToken);
                overwritten++;
                continue;
            }

            var slug = input.Slug ?? TextNormalizer.Slugify(input.Title);
            if (!existingSlugs.Add(slug))
            {
                rejected.Add(new SeedRejection(label, [$"O identificador público '{slug}' já está em uso por outro compromisso."]));
                continue;
            }

            var commitment = CommitmentAggregate.Create(input.ExternalId, slug, mandate.Id, area.Id, origin.Id, content, command.Actor, publish: true);

            if (input.InitialStatus is { } initial && CommitmentStatusLabels.TryParse(initial.Status, out var status))
            {
                commitment.RegisterStatus(status, initial.ReferenceDate, initial.SourceUrl, initial.Note, initial.IsFormalAct, command.Actor, today);
            }

            await _unitOfWork.CommitmentRepository.CreateAsync(commitment, cancellationToken);
            await _unitOfWork.AuditLogRepository.CreateAsync(
                AuditLog.Create(command.Actor, AuditActions.CommitmentCreated, nameof(CommitmentAggregate), slug, "Carga inicial."), cancellationToken);
            created++;
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return new SeedReportResponse(file.Commitments.Count, created, overwritten, skipped, rejected, areasCreated, originsCreated);
    }

    private async Task<MandateAggregate> EnsureMandateAsync(string candidate, DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        var (name, party) = ParseCandidate(candidate);

        var official = await _unitOfWork.MandateRepository.GetOfficialByNameAsync(name, cancellationToken);
        if (official is null)
        {
            official = Official.Create(name, party);
            await _unitOfWork.MandateRepository.CreateOfficialAsync(official, cancellationToken);
        }

        var title = $"Presidência da República {start.Year}–{end.Year}";
        var mandate = await _unitOfWork.MandateRepository.GetByTitleAsync(title, cancellationToken);
        if (mandate is null)
        {
            mandate = MandateAggregate.Create(official.Id, title, start, end);
            await _unitOfWork.MandateRepository.CreateAsync(mandate, cancellationToken);
        }

        return mandate;
    }

    private async Task<(Area Area, bool IsNew)> GetOrCreateAreaAsync(Dictionary<string, Area> areas, CommitmentInput input, CancellationToken cancellationToken)
    {
        var code = input.AreaCode.Trim().ToUpperInvariant();
        if (areas.TryGetValue(code, out var area)) return (area, false);

        area = Area.Create(code, input.AreaName);
        areas[code] = area;
        await _unitOfWork.AreaRepository.CreateAsync(area, cancellationToken);
        return (area, true);
    }

    private async Task<(Origin Origin, bool IsNew)> GetOrCreateOriginAsync(Dictionary<string, Origin> origins, CommitmentInput input, CancellationToken cancellationToken)
    {
        var code = TextNormalizer.Slugify(input.OriginName);
        if (origins.TryGetValue(code, out var origin)) return (origin, false);

        origin = Origin.Create(code, input.OriginName, code == OfficialPlanOriginCode);
        origins[code] = origin;
        await _unitOfWork.OriginRepository.CreateAsync(origin, cancellationToken);
        return (origin, true);
    }

    private static (string Name, string? Party) ParseCandidate(string candidate)
    {
        var match = CandidateRegex().Match(candidate.Trim());
        return match.Success ? (match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim()) : (candidate.Trim(), null);
    }

    [GeneratedRegex(@"^(.*?)\s*\(([^)]*)\)\s*$")]
    private static partial Regex CandidateRegex();
}
