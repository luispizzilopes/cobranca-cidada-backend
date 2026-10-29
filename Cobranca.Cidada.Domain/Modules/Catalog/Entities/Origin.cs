using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Catalog.Entities;

public class Origin : Entity<OriginId>
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    /// <summary>Indica se o texto do compromisso consta do documento oficial do plano de governo.</summary>
    public bool IsInOfficialPlan { get; private set; }

    private Origin() { }

    private Origin(string code, string name, bool isInOfficialPlan) : base(OriginId.CreateIdentifier())
    {
        Code = code;
        Name = name;
        IsInOfficialPlan = isInOfficialPlan;
    }

    public static Origin Create(string code, string name, bool isInOfficialPlan)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("O código da origem é obrigatório.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome da origem é obrigatório.");
        return new Origin(code.Trim().ToLowerInvariant(), name.Trim(), isInOfficialPlan);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome da origem é obrigatório.");
        Name = name.Trim();
    }
}
