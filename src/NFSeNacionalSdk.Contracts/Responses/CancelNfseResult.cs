using System.Net;
using NFSeNacionalSdk.Contracts.Documents;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class CancelNfseResult : INFSeResponse
{
    public required string AccessKey { get; init; }

    public bool Success { get; init; }

    public string? EventId { get; init; }

    public string? SubmittedEventXml { get; init; }

    public string? RawXml { get; init; }

    public string? RawJson { get; init; }

    public NFSeEventDocument? Event { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
