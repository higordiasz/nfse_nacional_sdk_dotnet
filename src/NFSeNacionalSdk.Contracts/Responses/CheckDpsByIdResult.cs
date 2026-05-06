using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class CheckDpsByIdResult : INFSeResponse
{
    public required string DpsId { get; init; }

    public bool Generated { get; init; }

    public bool Success => Generated;

    public string? RawXml { get; init; }

    public string? RawJson { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
