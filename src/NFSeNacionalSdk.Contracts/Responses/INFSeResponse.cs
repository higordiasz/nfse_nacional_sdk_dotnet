using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public interface INFSeResponse
{
    bool Success { get; }

    HttpStatusCode StatusCode { get; }

    IReadOnlyList<NFSeMessage> Messages { get; }

    string? RawXml { get; }

    string? RawJson { get; }
}
