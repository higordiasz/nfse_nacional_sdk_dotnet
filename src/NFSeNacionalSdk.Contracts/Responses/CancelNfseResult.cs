using System.Net;
using NFSeNacionalSdk.Contracts.Documents;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class CancelNfseResult : INFSeResponse
{
    public string AccessKey { get; set; }

    public bool Success { get; set; }

    public string? EventId { get; set; }

    public string? SubmittedEventXml { get; set; }

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public NFSeEventDocument? Event { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}
