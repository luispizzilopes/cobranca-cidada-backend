using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

public class SeedMetadataRequest
{
    [JsonPropertyName("candidato")] public string? Candidate { get; set; }
    [JsonPropertyName("eleicao")] public string? Election { get; set; }
}
