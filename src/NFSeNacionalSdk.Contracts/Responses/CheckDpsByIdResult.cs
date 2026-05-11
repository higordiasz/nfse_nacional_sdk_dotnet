using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class CheckDpsByIdResult : INFSeResponse
{
    public string DpsId { get; set; }

    public bool Generated { get; set; }

    public bool Success => Generated;

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}
