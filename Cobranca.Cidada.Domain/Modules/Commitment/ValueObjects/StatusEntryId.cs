using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

public class StatusEntryId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private StatusEntryId(Guid value)
    {
        Value = value;
    }

    public static StatusEntryId CreateIdentifier() => new(Guid.NewGuid());

    public static StatusEntryId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
