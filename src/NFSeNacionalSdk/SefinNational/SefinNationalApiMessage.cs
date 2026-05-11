using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalApiMessage
{
    [JsonPropertyName("mensagem")]
    public string? Message { get; set; }

    [JsonPropertyName("codigo")]
    public string? Code { get; set; }

    [JsonPropertyName("descricao")]
    public string? Description { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complement { get; set; }

    public string? GetResolvedDescription()
    {
        return FirstNonEmpty(Description, Message, Complement);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }
}
