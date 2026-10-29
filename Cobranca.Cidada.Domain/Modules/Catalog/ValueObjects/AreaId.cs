using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;

public class AreaId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private AreaId(Guid value)
    {
        Value = value;
    }

    public static AreaId CreateIdentifier() => new(Guid.NewGuid());

    public static AreaId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
