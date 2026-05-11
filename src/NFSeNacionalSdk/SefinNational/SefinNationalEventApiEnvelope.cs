using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalEventApiEnvelope
{
    [JsonPropertyName("tipoAmbiente")]
    public int? EnvironmentType { get; set; }

    [JsonPropertyName("versaoAplicativo")]
    public string? ApplicationVersion { get; set; }

    [JsonPropertyName("dataHoraProcessamento")]
    public DateTimeOffset? ProcessedAt { get; set; }

    [JsonPropertyName("idPedidoRegistroEvento")]
    public string? EventRequestId { get; set; }

    [JsonPropertyName("chaveAcesso")]
    public string? AccessKey { get; set; }

    [JsonPropertyName("eventoXmlGZipB64")]
    public string? EventXmlGZipBase64 { get; set; }

    [JsonPropertyName("alertas")]
    public JsonElement? Alerts { get; set; }

    [JsonPropertyName("erros")]
    public JsonElement? Errors { get; set; }

    [JsonPropertyName("erro")]
    public JsonElement? Error { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }
}

internal sealed class SefinNationalEventRequest
{
    [JsonPropertyName("pedidoRegistroEventoXmlGZipB64")]
    public string EventRequestXmlGZipBase64 { get; set; } = string.Empty;
}
