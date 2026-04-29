namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeDocument
{
    public string? AccessKey { get; set; }

    public string? Number { get; set; }

    public string? DfseNumber { get; set; }

    public string? VerificationCode { get; set; }

    public DateTimeOffset? IssuedAt { get; set; }

    public DateTimeOffset? DpsIssuedAt { get; set; }

    public DateOnly? CompetenceDate { get; set; }

    public string? StatusCode { get; set; }

    public string? ApplicationVersion { get; set; }

    public string? IssuingMunicipalityName { get; set; }

    public string? ServiceLocationMunicipalityName { get; set; }

    public string? IncidenceMunicipalityCode { get; set; }

    public string? IncidenceMunicipalityName { get; set; }

    public string? NationalTaxationDescription { get; set; }

    public string? DpsId { get; set; }

    public string? DpsSeries { get; set; }

    public string? DpsNumber { get; set; }

    public decimal? NetAmount { get; set; }

    public NFSeParty? Issuer { get; set; }

    public NFSeParty? Recipient { get; set; }

    public NFSeService? Service { get; set; }
}
