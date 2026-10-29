using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

public class CommitmentRevisionId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private CommitmentRevisionId(Guid value)
    {
        Value = value;
    }

    public static CommitmentRevisionId CreateIdentifier() => new(Guid.NewGuid());

    public static CommitmentRevisionId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
