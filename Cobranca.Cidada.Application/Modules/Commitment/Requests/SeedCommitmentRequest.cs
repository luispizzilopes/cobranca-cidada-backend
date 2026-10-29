using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

public class SeedCommitmentRequest
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("area")] public string? Area { get; set; }
    [JsonPropertyName("area_codigo")] public string? AreaCode { get; set; }
    [JsonPropertyName("titulo")] public string? Title { get; set; }
    [JsonPropertyName("descricao")] public string? Description { get; set; }
    [JsonPropertyName("descricao_resumida")] public string? ShortDescription { get; set; }
    [JsonPropertyName("como_sera_feito")] public string? HowItWillBeDone { get; set; }
    [JsonPropertyName("detalhamento_no_plano")] public string? PlanDetail { get; set; }
    [JsonPropertyName("origem")] public string? Origin { get; set; }
    [JsonPropertyName("data")] public string? Date { get; set; }
    [JsonPropertyName("data_tipo")] public string? DateKind { get; set; }
    [JsonPropertyName("prazo_texto")] public string? DeadlineText { get; set; }
    [JsonPropertyName("meta_ou_condicao")] public string? GoalOrCondition { get; set; }
    [JsonPropertyName("classificacao")] public List<string>? Classification { get; set; }
    [JsonPropertyName("evidencia")] public List<string>? Evidence { get; set; }
    [JsonPropertyName("pagina_plano")] public string? PlanPage { get; set; }
    [JsonPropertyName("fontes")] public List<SeedSourceRequest>? Sources { get; set; }
    [JsonPropertyName("execucao")] public SeedExecutionRequest? Execution { get; set; }
}
