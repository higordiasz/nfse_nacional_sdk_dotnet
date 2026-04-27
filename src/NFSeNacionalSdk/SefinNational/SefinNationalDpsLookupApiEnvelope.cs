using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalDpsLookupApiEnvelope
{
    [JsonPropertyName("tipoAmbiente")]
    public int? EnvironmentType { get; init; }

    [JsonPropertyName("versaoAplicativo")]
    public string? ApplicationVersion { get; init; }

    [JsonPropertyName("dataHoraProcessamento")]
    public DateTimeOffset? ProcessedAt { get; init; }

    [JsonPropertyName("idDps")]
    public string? DpsId { get; init; }

    [JsonPropertyName("chaveAcesso")]
    public string? AccessKey { get; init; }

    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; init; }

    [JsonPropertyName("erros")]
    public IReadOnlyList<SefinNationalApiMessage>? Errors { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; init; }

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
