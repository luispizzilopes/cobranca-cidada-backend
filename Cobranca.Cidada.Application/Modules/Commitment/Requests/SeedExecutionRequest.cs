using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

public class SeedExecutionRequest
{
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("data_referencia")] public string? ReferenceDate { get; set; }
}
