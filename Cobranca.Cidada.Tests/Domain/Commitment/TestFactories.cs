using Cobranca.Cidada.Domain.Modules.Catalog.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;
using Cobranca.Cidada.Domain.Modules.Mandate.ValueObjects;
using CommitmentAggregate = Cobranca.Cidada.Domain.Modules.Commitment.Aggregates.Commitment;

namespace Cobranca.Cidada.Tests.Domain.Commitment;

internal static class TestFactories
{
    public static CommitmentContent Content(string title = "Título", DateOnly? deadlineDate = null) => new(
        title,
        "Descrição do compromisso.",
        null,
        null,
        false,
        null,
        null,
        deadlineDate is null ? null : "\"em 2027\"",
        deadlineDate,
        null,
        ["Proposta formal"],
        ["alta"],
        "12",
        [new SourceContent("Plano (PDF TSE)", "https://www.tse.jus.br/plano.pdf", "12", SourceKind.Primary)]);

    public static CommitmentAggregate Commitment(DateOnly? deadlineDate = null, bool publish = true) => CommitmentAggregate.Create(
        "LULA26-TEST-001",
        "TEST-001",
        MandateId.CreateIdentifier(),
        AreaId.CreateIdentifier(),
        OriginId.CreateIdentifier(),
        Content(deadlineDate: deadlineDate),
        "tester",
        publish);
}
