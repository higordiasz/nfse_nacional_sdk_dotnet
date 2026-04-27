using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetMunicipalConventionResult
{
    public required string MunicipalityCode { get; init; }

    public bool IsAvailable { get; init; }

    public string? JsonContent { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
