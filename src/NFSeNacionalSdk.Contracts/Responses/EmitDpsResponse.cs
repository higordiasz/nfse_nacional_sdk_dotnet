using System.Net;
using NFSeNacionalSdk.Contracts.Documents;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class EmitDpsResponse
{
    public bool Success { get; init; }

    public string? DpsId { get; init; }

    public string? AccessKey { get; init; }

    public required string SubmittedDpsXml { get; init; }

    public string? RawXml { get; init; }

    public NFSeDocument? Document { get; init; }

    public string? JsonContent { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
