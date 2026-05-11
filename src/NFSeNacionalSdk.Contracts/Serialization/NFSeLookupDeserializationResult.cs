using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Contracts.Responses;

namespace NFSeNacionalSdk.Contracts.Serialization;

public sealed class NFSeLookupDeserializationResult
{
    public bool Success { get; set; }

    public NFSeDocument? Document { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();
}
