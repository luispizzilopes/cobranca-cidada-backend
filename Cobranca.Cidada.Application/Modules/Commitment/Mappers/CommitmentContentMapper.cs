using Cobranca.Cidada.Application.Modules.Commitment.Requests;
using Cobranca.Cidada.Domain.Modules.Commitment.Enums;
using Cobranca.Cidada.Domain.Modules.Commitment.ValueObjects;

namespace Cobranca.Cidada.Application.Modules.Commitment.Mappers;

public static class CommitmentContentMapper
{
    /// <summary>Converte a entrada (já validada) no conteúdo editável do agregado. Usado pelo seed e pelo painel.</summary>
    public static CommitmentContent ToContent(this CommitmentInput input) => new(
        input.Title,
        input.Description,
        input.ShortDescription,
        input.HowItWillBeDone,
        input.HasPlanDetail,
        input.Date,
        input.DateKind,
        input.DeadlineText,
        input.DeadlineDate,
        input.GoalOrCondition,
        input.Classification,
        input.Evidence,
        input.PlanPage,
        input.Sources.Select(s => new SourceContent(s.Name, s.Url, s.Page, s.Kind == "primary" ? SourceKind.Primary : SourceKind.Press)).ToList());
}
