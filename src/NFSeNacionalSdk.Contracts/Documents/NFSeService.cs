namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeService
{
    public string? Description { get; set; }

    public string? ServiceCode { get; set; }

    public string? MunicipalServiceCode { get; set; }

    public string? NationalClassificationCode { get; set; }

    public string? InternalCode { get; set; }

    public string? NationalTaxationDescription { get; set; }

    public string? LocationMunicipalityCode { get; set; }

    public string? LocationMunicipalityName { get; set; }

    public decimal? ServiceAmount { get; set; }
}
