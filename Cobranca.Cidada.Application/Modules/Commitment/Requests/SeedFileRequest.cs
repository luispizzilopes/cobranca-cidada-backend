using System.Text.Json.Serialization;

namespace Cobranca.Cidada.Application.Modules.Commitment.Requests;

/// <summary>Formato do arquivo JSON de carga inicial (somente seed; nunca lido em tempo de execução).</summary>
public class SeedFileRequest
{
    [JsonPropertyName("metadados")] public SeedMetadataRequest Metadata { get; set; } = new();
    [JsonPropertyName("promessas")] public List<SeedCommitmentRequest> Commitments { get; set; } = [];
}
