using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalLookupApiEnvelope
{
    [JsonPropertyName("tipoAmbiente")]
    public int? EnvironmentType { get; init; }

    [JsonPropertyName("versaoAplicativo")]
    public string? ApplicationVersion { get; init; }

    [JsonPropertyName("dataHoraProcessamento")]
    public DateTimeOffset? ProcessedAt { get; init; }

    [JsonPropertyName("chaveAcesso")]
    public string? AccessKey { get; init; }

    [JsonPropertyName("nfseXmlGZipB64")]
    public string? NfseXmlGZipBase64 { get; init; }

    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; init; }
}
