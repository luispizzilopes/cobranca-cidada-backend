using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Moderation.ValueObjects;

public class ProfanityWordId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private ProfanityWordId(Guid value)
    {
        Value = value;
    }

    public static ProfanityWordId CreateIdentifier() => new(Guid.NewGuid());

    public static ProfanityWordId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
