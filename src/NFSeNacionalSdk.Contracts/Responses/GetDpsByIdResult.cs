using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetDpsByIdResult : INFSeResponse
{
    public string DpsId { get; set; }

    public string? AccessKey { get; set; }

    public bool Success { get; set; }

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}
