namespace NFSeNacionalSdk.Contracts.Serialization;

public sealed class CancelNfseSerializationResult
{
    public required string EventRequestId { get; init; }

    public required string XmlContent { get; init; }
}
