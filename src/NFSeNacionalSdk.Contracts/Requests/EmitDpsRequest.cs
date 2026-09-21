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

    public EmitDpsFederalTaxation? Federal { get; set; }

    public EmitDpsIbsCbsTaxation? IbsCbs { get; set; }
}

public sealed class EmitDpsFederalTaxation
{
    public EmitDpsPisCofinsTaxation? PisCofins { get; set; }

    public decimal? SocialSecurityRetentionAmount { get; set; }

    public decimal? IncomeTaxRetentionAmount { get; set; }

    public decimal? SocialContributionRetentionAmount { get; set; }
}

public sealed class EmitDpsPisCofinsTaxation
{
    /// <summary>Official two-digit CST code. Unknown future codes can be supplied without an SDK upgrade.</summary>
    public string TaxStatusCode { get; set; } = string.Empty;

    public decimal? CalculationBase { get; set; }

    public decimal? PisRate { get; set; }

    public decimal? CofinsRate { get; set; }

    public decimal? PisAmount { get; set; }

    public decimal? CofinsAmount { get; set; }

    public NFSePisCofinsWithholdingType? WithholdingType { get; set; }
}

public sealed class EmitDpsIbsCbsTaxation
{
    public NFSeIbsCbsPurpose Purpose { get; set; } = NFSeIbsCbsPurpose.Regular;

    public bool? IsFinalConsumer { get; set; }

    public string OperationIndicatorCode { get; set; } = string.Empty;

    public string? OperationTypeCode { get; set; }

    public NFSeIbsCbsDestinationIndicator DestinationIndicator { get; set; }

    public string TaxStatusCode { get; set; } = string.Empty;

    public string TaxClassificationCode { get; set; } = string.Empty;

    public string? PresumedCreditCode { get; set; }

    public EmitDpsIbsCbsRegularTaxation? RegularTaxation { get; set; }

    public EmitDpsIbsCbsDeferral? Deferral { get; set; }
}

public sealed class EmitDpsIbsCbsRegularTaxation
{
    public string TaxStatusCode { get; set; } = string.Empty;

    public string TaxClassificationCode { get; set; } = string.Empty;
}

public sealed class EmitDpsIbsCbsDeferral
{
    public decimal? StateIbsRate { get; set; }

    public decimal? MunicipalIbsRate { get; set; }

    public decimal? CbsRate { get; set; }
}
