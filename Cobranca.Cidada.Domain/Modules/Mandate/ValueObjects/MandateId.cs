using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;

public class MandateId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private MandateId(Guid value)
    {
        Value = value;
    }

    public static MandateId CreateIdentifier() => new(Guid.NewGuid());

    public static MandateId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
