using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

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
