using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetMunicipalServiceParametersResult : INFSeResponse
{
    public string MunicipalityCode { get; set; }

    public string ServiceCode { get; set; }

    public DateOnly CompetenceDate { get; set; }

    public bool IsAvailable { get; set; }

    public bool Success => IsAvailable;

    public IReadOnlyDictionary<string, IReadOnlyList<MunicipalServiceTaxRate>> TaxRates { get; set; } =
        new Dictionary<string, IReadOnlyList<MunicipalServiceTaxRate>>();

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public string? JsonContent { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}

public sealed class MunicipalServiceTaxRate
{
    public string? Incidence { get; set; }

    public decimal Rate { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }
}
