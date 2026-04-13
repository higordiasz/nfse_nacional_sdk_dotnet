using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Contracts.Responses;

namespace NFSeNacionalSdk.Contracts.Serialization;

public sealed class NFSeLookupDeserializationResult
{
    public bool Success { get; init; }

    public NFSeDocument? Document { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();
}
