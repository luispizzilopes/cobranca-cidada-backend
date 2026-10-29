using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;

namespace Cobranca.Cidada.BuildingBlocks.Common.Class.AggregateRoot;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }
}
