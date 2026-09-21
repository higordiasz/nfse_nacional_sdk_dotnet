using System.Net;
using NFSeNacionalSdk.Contracts.Documents;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetNfseEventsResult : INFSeResponse
{
    public string AccessKey { get; set; } = string.Empty;

    public bool Success { get; set; }

    public IReadOnlyList<NFSeEventDocument> Events { get; set; } = Array.Empty<NFSeEventDocument>();

    public IReadOnlyList<string> RawXmlDocuments { get; set; } = Array.Empty<string>();

    public string? RawXml => RawXmlDocuments.FirstOrDefault();

    public string? RawJson { get; set; }

    public string? JsonContent { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}
