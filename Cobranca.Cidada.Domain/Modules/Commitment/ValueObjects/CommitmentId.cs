using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

public class CommitmentId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private CommitmentId(Guid value)
    {
        Value = value;
    }

    public static CommitmentId CreateIdentifier() => new(Guid.NewGuid());

    public static CommitmentId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
