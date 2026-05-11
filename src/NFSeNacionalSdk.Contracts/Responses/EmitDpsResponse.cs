using System.Net;
using NFSeNacionalSdk.Contracts.Documents;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class EmitDpsResponse : INFSeResponse
{
    public bool Success { get; set; }

    public string? DpsId { get; set; }

    public string? AccessKey { get; set; }

    public string SubmittedDpsXml { get; set; }

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public NFSeDocument? Document { get; set; }

    public string? JsonContent { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}
