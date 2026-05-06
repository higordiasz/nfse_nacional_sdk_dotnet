using System.Net;
using NFSeNacionalSdk.Contracts.Documents;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetNfseByAccessKeyResult : INFSeResponse
{
    public required string AccessKey { get; init; }

    public bool Success { get; init; }

    public string? RawXml { get; init; }

    public string? RawJson { get; init; }

    public NFSeDocument? Document { get; init; }

    public string? JsonContent { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
