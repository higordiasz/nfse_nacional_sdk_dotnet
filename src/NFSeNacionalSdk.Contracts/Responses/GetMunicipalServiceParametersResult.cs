using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetMunicipalServiceParametersResult
{
    public required string MunicipalityCode { get; init; }

    public required string ServiceCode { get; init; }

    public bool IsAvailable { get; init; }

    public string? JsonContent { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
