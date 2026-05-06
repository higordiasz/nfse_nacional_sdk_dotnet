using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetMunicipalServiceParametersResult : INFSeResponse
{
    public required string MunicipalityCode { get; init; }

    public required string ServiceCode { get; init; }

    public DateOnly CompetenceDate { get; init; }

    public bool IsAvailable { get; init; }

    public bool Success => IsAvailable;

    public string? RawXml { get; init; }

    public string? RawJson { get; init; }

    public string? JsonContent { get; init; }

    public IReadOnlyList<NFSeMessage> Messages { get; init; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; init; }
}
