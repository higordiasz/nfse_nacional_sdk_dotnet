using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalEventApiEnvelope
{
    [JsonPropertyName("tipoAmbiente")]
    public int? EnvironmentType { get; init; }

    [JsonPropertyName("versaoAplicativo")]
    public string? ApplicationVersion { get; init; }

    [JsonPropertyName("dataHoraProcessamento")]
    public DateTimeOffset? ProcessedAt { get; init; }

    [JsonPropertyName("idPedidoRegistroEvento")]
    public string? EventRequestId { get; init; }

    [JsonPropertyName("chaveAcesso")]
    public string? AccessKey { get; init; }

    [JsonPropertyName("eventoXmlGZipB64")]
    public string? EventXmlGZipBase64 { get; init; }

    [JsonPropertyName("alertas")]
    public JsonElement? Alerts { get; init; }

    [JsonPropertyName("erros")]
    public JsonElement? Errors { get; init; }

    [JsonPropertyName("erro")]
    public JsonElement? Error { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; init; }
}

internal sealed class SefinNationalEventRequest
{
    [JsonPropertyName("pedidoRegistroEventoXmlGZipB64")]
    public required string EventRequestXmlGZipBase64 { get; init; }
}
