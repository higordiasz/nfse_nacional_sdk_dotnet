using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetMunicipalServiceParametersResult : INFSeResponse
{
    public string MunicipalityCode { get; set; }

    public string ServiceCode { get; set; }

    public DateOnly CompetenceDate { get; set; }

    public bool IsAvailable { get; set; }

    public bool Success => IsAvailable;

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public string? JsonContent { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}
