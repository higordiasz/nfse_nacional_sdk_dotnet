namespace NFSeNacionalSdk.Contracts.Serialization;

public sealed class EmitDpsSerializationResult
{
    public required string DpsId { get; init; }

    public required string XmlContent { get; init; }
}
