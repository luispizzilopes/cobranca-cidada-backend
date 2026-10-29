using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

public class SeedSourceRequest
{
    [JsonPropertyName("nome")] public string? Name { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("pagina")] public string? Page { get; set; }
}
