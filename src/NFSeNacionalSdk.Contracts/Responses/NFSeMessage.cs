namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class NFSeMessage
{
    public string? Code { get; init; }

    public required string Description { get; init; }
}
