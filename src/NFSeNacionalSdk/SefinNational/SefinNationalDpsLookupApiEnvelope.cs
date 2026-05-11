using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalDpsLookupApiEnvelope
{
    [JsonPropertyName("tipoAmbiente")]
    public int? EnvironmentType { get; set; }

    [JsonPropertyName("versaoAplicativo")]
    public string? ApplicationVersion { get; set; }

    [JsonPropertyName("dataHoraProcessamento")]
    public DateTimeOffset? ProcessedAt { get; set; }

    [JsonPropertyName("idDps")]
    public string? DpsId { get; set; }

    [JsonPropertyName("chaveAcesso")]
    public string? AccessKey { get; set; }

    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; set; }

    [JsonPropertyName("erros")]
    public IReadOnlyList<SefinNationalApiMessage>? Errors { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }

    public string? GetResolvedDpsId()
    {
        if (AdditionalData is not null &&
            AdditionalData.TryGetValue("idDPS", out var value) &&
            value.ValueKind == JsonValueKind.String)
        {
            return FirstNonEmpty(DpsId, value.GetString());
        }

        return DpsId;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }
}
