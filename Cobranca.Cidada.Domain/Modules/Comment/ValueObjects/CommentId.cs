using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Comment.ValueObjects;

public class CommentId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private CommentId(Guid value)
    {
        Value = value;
    }

    public static CommentId CreateIdentifier() => new(Guid.NewGuid());

    public static CommentId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
