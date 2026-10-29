using ValueObjectBase = Cobranca.Cidada.BuildingBlocks.Common.Class.ValueObject.ValueObject;

namespace Cobranca.Cidada.Domain.Modules.Audit.ValueObjects;

public class AuditLogId : ValueObjectBase
{
    public Guid Value { get; private set; }

    private AuditLogId(Guid value)
    {
        Value = value;
    }

    public static AuditLogId CreateIdentifier() => new(Guid.NewGuid());

    public static AuditLogId Create(Guid value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
