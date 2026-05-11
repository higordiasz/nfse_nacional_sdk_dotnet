using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalMunicipalConventionApiEnvelope
{
    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; set; }

    [JsonPropertyName("erros")]
    public IReadOnlyList<SefinNationalApiMessage>? Errors { get; set; }

    [JsonPropertyName("mensagem")]
    public string? Message { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }
}
