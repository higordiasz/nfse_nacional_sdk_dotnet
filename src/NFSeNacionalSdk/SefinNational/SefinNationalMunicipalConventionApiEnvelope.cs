using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalMunicipalConventionApiEnvelope
{
    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; init; }

    [JsonPropertyName("erros")]
    public IReadOnlyList<SefinNationalApiMessage>? Errors { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; init; }
}
