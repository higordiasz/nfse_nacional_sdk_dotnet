using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetDpsByIdResult
{
    public required string DpsId { get; init; }

    public string? AccessKey { get; init; }

    public bool Success { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
