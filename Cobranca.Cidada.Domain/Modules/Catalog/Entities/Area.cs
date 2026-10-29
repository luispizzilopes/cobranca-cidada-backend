using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Catalog.Entities;

public class Area : Entity<AreaId>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private Area() { }

    private Area(string code, string name) : base(AreaId.CreateIdentifier())
    {
        Code = code;
        Name = name;
    }

    public static Area Create(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("O código da área é obrigatório.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome da área é obrigatório.");
        return new Area(code.Trim().ToUpperInvariant(), name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome da área é obrigatório.");
        Name = name.Trim();
    }
}
