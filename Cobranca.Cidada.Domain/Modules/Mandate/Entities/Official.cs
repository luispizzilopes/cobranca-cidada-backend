using Cobranca.Cidada.BuildingBlocks.Common.Class.Base;
using Cobranca.Cidada.Domain.Exceptions;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;

namespace Cobranca.Cidada.Domain.Modules.Mandate.Entities;

public class Official : Entity<OfficialId>
{
    public string Name { get; private set; } = null!;
    public string? Party { get; private set; }

    private Official() { }

    private Official(string name, string? party) : base(OfficialId.CreateIdentifier())
    {
        Name = name;
        Party = party;
    }

    public static Official Create(string name, string? party)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome do agente político é obrigatório.");
        return new Official(name.Trim(), string.IsNullOrWhiteSpace(party) ? null : party.Trim());
    }
}
