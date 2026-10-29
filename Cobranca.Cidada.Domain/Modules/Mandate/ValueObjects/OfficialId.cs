using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;

public class OfficialId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private OfficialId(Guid value)
    {
        Value = value;
    }

    public static OfficialId CreateIdentifier() => new(Guid.NewGuid());

    public static OfficialId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
