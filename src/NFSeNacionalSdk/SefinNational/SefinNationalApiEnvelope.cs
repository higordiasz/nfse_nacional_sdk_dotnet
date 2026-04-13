using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalApiEnvelope
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

internal sealed class SefinNationalApiMessage
{
    [JsonPropertyName("mensagem")]
    public string? Message { get; init; }

    [JsonPropertyName("codigo")]
    public string? Code { get; init; }

    [JsonPropertyName("descricao")]
    public string? Description { get; init; }

    [JsonPropertyName("complemento")]
    public string? Complement { get; init; }

    public string? GetResolvedDescription()
    {
        return FirstNonEmpty(Description, Message, Complement);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }
}
