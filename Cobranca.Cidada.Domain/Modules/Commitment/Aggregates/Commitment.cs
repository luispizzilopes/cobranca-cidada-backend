using System.Text.Json;
using System.Text.Json.Serialization;
using Cobranca.Cidada.BuildingBlocks.Common.Class.AggregateRoot;
using Cobranca.Cidada.BuildingBlocks.Common.Interfaces;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Catalog.Entities;
using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.Entities;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.Rules;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Commitment.Aggregates;

public class Commitment : AggregateRoot<CommitmentId>, IAuditableEntity
{
    private static readonly JsonSerializerOptions SnapshotOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly List<CommitmentSource> _sources = new();
    private readonly List<StatusEntry> _statusEntries = new();
    private readonly List<CommitmentRevision> _revisions = new();

    /// <summary>Identificador legado/externo (ex.: LULA26-ECON-001). Nulo para compromissos criados no sistema.</summary>
    public string? ExternalId { get; private set; }
    public string Slug { get; private set; } = null!;

    public MandateId MandateId { get; private set; } = null!;
    public AreaId AreaId { get; private set; } = null!;
    public OriginId OriginId { get; private set; } = null!;
    public Area Area { get; private set; } = null!;
    public Origin Origin { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? ShortDescription { get; private set; }
    public string? HowItWillBeDone { get; private set; }
    public bool HasPlanDetail { get; private set; }
    public DateOnly? Date { get; private set; }
    public string? DateKind { get; private set; }
    public string? DeadlineText { get; private set; }

    /// <summary>Prazo estruturado, definido pela curadoria. Quando nulo, 'Descumprido' exige ato formal.</summary>
    public DateOnly? DeadlineDate { get; private set; }
    public string? GoalOrCondition { get; private set; }
    public string[] Classification { get; private set; } = [];
    public string[] Evidence { get; private set; } = [];
    public string? PlanPage { get; private set; }

    public CommitmentStatus CurrentStatus { get; private set; } = CommitmentStatus.NotEvaluated;
    public DateOnly? CurrentStatusDate { get; private set; }

    public bool IsPublished { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public int Version { get; private set; } = 1;
    public string CreatedBy { get; private set; } = null!;

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyList<CommitmentSource> Sources => _sources.AsReadOnly();
    public IReadOnlyList<StatusEntry> StatusEntries => _statusEntries.AsReadOnly();
    public IReadOnlyList<CommitmentRevision> Revisions => _revisions.AsReadOnly();

    private Commitment() { }

    private Commitment(
        string? externalId,
        string slug,
        MandateId mandateId,
        AreaId areaId,
        OriginId originId,
        CommitmentContent content,
        string createdBy) : base(CommitmentId.CreateIdentifier())
    {
        ExternalId = externalId;
        Slug = slug;
        MandateId = mandateId;
        AreaId = areaId;
        OriginId = originId;
        CreatedBy = createdBy;
        ApplyContent(content);
    }

    public static Commitment Create(
        string? externalId,
        string slug,
        MandateId mandateId,
        AreaId areaId,
        OriginId originId,
        CommitmentContent content,
        string createdBy,
        bool publish)
    {
        if (string.IsNullOrWhiteSpace(slug)) throw new DomainException("O slug do compromisso é obrigatório.");
        if (string.IsNullOrWhiteSpace(createdBy)) throw new DomainException("O autor é obrigatório.");

        var commitment = new Commitment(
            string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim(),
            slug.Trim(),
            mandateId,
            areaId,
            originId,
            content,
            createdBy);

        if (publish)
            commitment.Publish();

        return commitment;
    }

    /// <summary>Edita o conteúdo; o conteúdo anterior é guardado como revisão e a versão é incrementada.</summary>
    public void Update(CommitmentContent content, AreaId areaId, OriginId originId, string authorId, string reason)
    {
        if (string.IsNullOrWhiteSpace(authorId)) throw new DomainException("O autor da edição é obrigatório.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainException("O motivo da edição é obrigatório.");

        var snapshot = JsonSerializer.Serialize(ToContent(), SnapshotOptions);
        _revisions.Add(new CommitmentRevision(Version, snapshot, authorId, reason.Trim()));

        AreaId = areaId;
        OriginId = originId;
        ApplyContent(content);
        Version++;
    }

    public void Publish()
    {
        if (IsPublished) return;
        IsPublished = true;
        PublishedAtUtc = DateTime.UtcNow;
    }

    public void Unpublish() => IsPublished = false;

    /// <summary>Registra nova situação no histórico, aplicando as regras editoriais.</summary>
    public StatusEntry RegisterStatus(
        CommitmentStatus status,
        DateOnly referenceDate,
        string? sourceUrl,
        string? note,
        bool isFormalAct,
        string authorId,
        DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(authorId)) throw new DomainException("O autor do registro de situação é obrigatório.");

        var violation = StatusRules.Check(status, referenceDate, sourceUrl, DeadlineDate, isFormalAct, today);
        if (violation is not null)
            throw new DomainException(violation);

        var entry = new StatusEntry(status, referenceDate, sourceUrl?.Trim(), note?.Trim(), isFormalAct, authorId);
        _statusEntries.Add(entry);
        CurrentStatus = status;
        CurrentStatusDate = referenceDate;
        return entry;
    }

    public CommitmentContent ToContent() => new(
        Title, Description, ShortDescription, HowItWillBeDone, HasPlanDetail, Date, DateKind, DeadlineText,
        DeadlineDate, GoalOrCondition, Classification, Evidence, PlanPage,
        _sources.OrderBy(s => s.Position).Select(s => new SourceContent(s.Name, s.Url, s.Page, s.Kind)).ToList());

    private void ApplyContent(CommitmentContent content)
    {
        if (string.IsNullOrWhiteSpace(content.Title)) throw new DomainException("O título do compromisso é obrigatório.");
        if (string.IsNullOrWhiteSpace(content.Description)) throw new DomainException("A descrição do compromisso é obrigatória.");

        Title = content.Title.Trim();
        Description = content.Description.Trim();
        ShortDescription = content.ShortDescription;
        HowItWillBeDone = content.HowItWillBeDone;
        HasPlanDetail = content.HasPlanDetail;
        Date = content.Date;
        DateKind = content.DateKind;
        DeadlineText = content.DeadlineText;
        DeadlineDate = content.DeadlineDate;
        GoalOrCondition = content.GoalOrCondition;
        Classification = content.Classification.ToArray();
        Evidence = content.Evidence.ToArray();
        PlanPage = content.PlanPage;

        _sources.Clear();
        var position = 0;
        foreach (var source in content.Sources)
            _sources.Add(CommitmentSource.Create(source.Name, source.Url, source.Page, source.Kind, position++));
    }

    public void SetCreatedAt(DateTime timestampUtc) => CreatedAtUtc = timestampUtc;
    public void SetUpdatedAt(DateTime timestampUtc) => UpdatedAtUtc = timestampUtc;
}
