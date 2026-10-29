using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

public class CommitmentSourceId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private CommitmentSourceId(Guid value)
    {
        Value = value;
    }

    public static CommitmentSourceId CreateIdentifier() => new(Guid.NewGuid());

    public static CommitmentSourceId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
