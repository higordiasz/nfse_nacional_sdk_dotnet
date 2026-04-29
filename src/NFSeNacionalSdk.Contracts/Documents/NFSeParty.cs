namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeParty
{
    public string? Name { get; set; }

    public string? TaxId { get; set; }

    public string? MunicipalRegistration { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public NFSeAddress? Address { get; set; }

    public NFSeTaxRegime? TaxRegime { get; set; }
}

public sealed class NFSeAddress
{
    public string? Street { get; set; }

    public string? Number { get; set; }

    public string? Complement { get; set; }

    public string? Neighborhood { get; set; }

    public string? MunicipalityCode { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }
}
