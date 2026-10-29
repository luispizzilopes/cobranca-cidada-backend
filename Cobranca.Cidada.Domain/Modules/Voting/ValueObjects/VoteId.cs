using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Voting.ValueObjects;

public class VoteId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private VoteId(Guid value)
    {
        Value = value;
    }

    public static VoteId CreateIdentifier() => new(Guid.NewGuid());

    public static VoteId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
