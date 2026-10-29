using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;

public class OriginId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private OriginId(Guid value)
    {
        Value = value;
    }

    public static OriginId CreateIdentifier() => new(Guid.NewGuid());

    public static OriginId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
