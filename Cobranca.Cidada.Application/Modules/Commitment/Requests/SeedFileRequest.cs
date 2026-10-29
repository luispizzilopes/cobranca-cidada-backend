using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

/// <summary>Formato do arquivo JSON de carga inicial (somente seed; nunca lido em tempo de execução).</summary>
public class SeedFileRequest
{
    [JsonPropertyName("metadados")] public SeedMetadataRequest Metadata { get; set; } = new();
    [JsonPropertyName("promessas")] public List<SeedCommitmentRequest> Commitments { get; set; } = [];
}

public class SeedMetadataRequest
{
    [JsonPropertyName("candidato")] public string? Candidate { get; set; }
    [JsonPropertyName("eleicao")] public string? Election { get; set; }
}

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

public class SeedSourceRequest
{
    [JsonPropertyName("nome")] public string? Name { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("pagina")] public string? Page { get; set; }
}

public class SeedExecutionRequest
{
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("data_referencia")] public string? ReferenceDate { get; set; }
}
