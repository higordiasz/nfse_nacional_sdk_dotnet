using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class GetMunicipalConventionResult : INFSeResponse
{
    public string MunicipalityCode { get; set; }

    public bool IsAvailable { get; set; }

    public bool Success => IsAvailable;

    public MunicipalConventionParameters? Parameters { get; set; }

    public string? RawXml { get; set; }

    public string? RawJson { get; set; }

    public string? JsonContent { get; set; }

    public IReadOnlyList<NFSeMessage> Messages { get; set; } = Array.Empty<NFSeMessage>();

    public HttpStatusCode StatusCode { get; set; }
}

public sealed class MunicipalConventionParameters
{
    public int? ConventionType { get; set; }

    public int? UsesNationalEnvironment { get; set; }

    public int? UsesNationalIssuer { get; set; }

    public int? DefaultFederalTaxpayerIssuanceStatus { get; set; }

    public int? UsesNationalSupportModule { get; set; }

    public bool? AllowsTaxCredits { get; set; }
}
