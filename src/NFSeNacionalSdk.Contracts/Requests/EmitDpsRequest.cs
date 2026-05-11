using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class EmitDpsRequest
{
    public string Series { get; set; }

    public string Number { get; set; }

    public DateOnly CompetenceDate { get; set; }

    public DateTimeOffset IssuedAt { get; set; }

    public string MunicipalityCode { get; set; }

    public NFSeDpsEmitterType EmitterType { get; set; } = NFSeDpsEmitterType.Provider;

    public EmitDpsProvider Provider { get; set; }

    public EmitDpsRecipient? Recipient { get; set; }

    public EmitDpsService Service { get; set; }

    public EmitDpsTaxation Taxation { get; set; }
}

public sealed class EmitDpsProvider
{
    public string TaxId { get; set; }

    public string? MunicipalRegistration { get; set; }

    public string? Name { get; set; }

    public EmitDpsAddress? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public NFSeSimplesNationalOption SimplesNationalOption { get; set; }

    public NFSeSimplifiedNationalTaxRegime? SimplifiedNationalTaxRegime { get; set; }

    public NFSeSpecialTaxRegime SpecialTaxRegime { get; set; } = NFSeSpecialTaxRegime.None;
}

public sealed class EmitDpsRecipient
{
    public string TaxId { get; set; }

    public string Name { get; set; }

    public string? MunicipalRegistration { get; set; }

    public EmitDpsAddress? Address { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}

public sealed class EmitDpsAddress
{
    public string MunicipalityCode { get; set; }

    public string ZipCode { get; set; }

    public string Street { get; set; }

    public string Number { get; set; }

    public string? Complement { get; set; }

    public string Neighborhood { get; set; }
}

public sealed class EmitDpsService
{
    public string? ServiceLocationMunicipalityCode { get; set; }

    public string NationalTaxationCode { get; set; }

    public string? MunicipalTaxationCode { get; set; }

    public string Description { get; set; }

    public string? NationalClassificationCode { get; set; }

    public string? InternalCode { get; set; }

    public decimal Amount { get; set; }

    public decimal? AmountReceivedByIntermediary { get; set; }

    public decimal? UnconditionalDiscountAmount { get; set; }

    public decimal? ConditionalDiscountAmount { get; set; }
}

public sealed class EmitDpsTaxation
{
    public NFSeIssTaxationType IssTaxationType { get; set; } = NFSeIssTaxationType.TaxableOperation;

    public NFSeIssWithholdingType IssWithholdingType { get; set; } = NFSeIssWithholdingType.NotWithheld;

    public decimal? IssRate { get; set; }

    public NFSeTotalTaxIndicator? TotalTaxIndicator { get; set; } = NFSeTotalTaxIndicator.NotInformed;

    public decimal? SimplesNationalTotalTaxRate { get; set; }
}
