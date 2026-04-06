namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class EmitDpsResponse
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public string? DpsId { get; init; }

    public string? AccessKey { get; init; }

    public string? RawResponse { get; init; }
}