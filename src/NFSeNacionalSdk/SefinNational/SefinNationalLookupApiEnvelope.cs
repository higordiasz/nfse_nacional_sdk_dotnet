using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalLookupApiEnvelope
{
    [JsonPropertyName("tipoAmbiente")]
    public int? EnvironmentType { get; set; }

    [JsonPropertyName("versaoAplicativo")]
    public string? ApplicationVersion { get; set; }

    [JsonPropertyName("dataHoraProcessamento")]
    public DateTimeOffset? ProcessedAt { get; set; }

    [JsonPropertyName("chaveAcesso")]
    public string? AccessKey { get; set; }

    [JsonPropertyName("nfseXmlGZipB64")]
    public string? NfseXmlGZipBase64 { get; set; }

    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; set; }
}
