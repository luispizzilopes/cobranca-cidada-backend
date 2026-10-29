using Cobranca.Cidada.BuildingBlocks.Common.Class.AggregateRoot;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Mandate.Aggregates;

public class Mandate : AggregateRoot<MandateId>
{
    public OfficialId OfficialId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }

    private Mandate() { }

    private Mandate(OfficialId officialId, string title, DateOnly startDate, DateOnly endDate)
        : base(MandateId.CreateIdentifier())
    {
        OfficialId = officialId;
        Title = title;
        StartDate = startDate;
        EndDate = endDate;
    }

    public static Mandate Create(OfficialId officialId, string title, DateOnly startDate, DateOnly endDate)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("O título do mandato é obrigatório.");
        if (endDate <= startDate) throw new DomainException("O fim do mandato deve ser posterior ao início.");
        return new Mandate(officialId, title.Trim(), startDate, endDate);
    }
}
