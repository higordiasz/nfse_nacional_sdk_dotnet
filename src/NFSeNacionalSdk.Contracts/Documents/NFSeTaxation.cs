using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeTaxRegime
{
    public string? SimplesNationalOptionCode { get; set; }

    public NFSeSimplesNationalOption? SimplesNationalOption { get; set; }

    public string? SimplifiedNationalTaxRegimeCode { get; set; }

    public NFSeSimplifiedNationalTaxRegime? SimplifiedNationalTaxRegime { get; set; }

    public string? SpecialTaxRegimeCode { get; set; }

    public NFSeSpecialTaxRegime? SpecialTaxRegime { get; set; }
}

public sealed class NFSeTaxation
{
    public NFSeMunicipalTaxation? Municipal { get; set; }

    public NFSeFederalTaxation? Federal { get; set; }

    public NFSeTotalTax? Total { get; set; }

    public NFSeIbsCbsTaxation? IbsCbs { get; set; }
}

public sealed class NFSeMunicipalTaxation
{
    public string? IssTaxationTypeCode { get; set; }

    public NFSeIssTaxationType? IssTaxationType { get; set; }

    public string? IssWithholdingTypeCode { get; set; }

    public NFSeIssWithholdingType? IssWithholdingType { get; set; }

    public decimal? IssRate { get; set; }
}

public sealed class NFSeFederalTaxation
{
    public NFSePisCofinsTaxation? PisCofins { get; set; }

    public decimal? SocialSecurityRetentionAmount { get; set; }

    public decimal? IncomeTaxRetentionAmount { get; set; }

    public decimal? SocialContributionRetentionAmount { get; set; }
}

public sealed class NFSePisCofinsTaxation
{
    public string? TaxStatusCode { get; set; }

    public decimal? CalculationBase { get; set; }

    public decimal? PisRate { get; set; }

    public decimal? CofinsRate { get; set; }

    public decimal? PisAmount { get; set; }

    public decimal? CofinsAmount { get; set; }

    public string? WithholdingTypeCode { get; set; }

    public NFSePisCofinsWithholdingType? WithholdingType { get; set; }
}

public sealed class NFSeIbsCbsTaxation
{
    public string? PurposeCode { get; set; }

    public NFSeIbsCbsPurpose? Purpose { get; set; }

    public bool? IsFinalConsumer { get; set; }

    public string? OperationIndicatorCode { get; set; }

    public string? OperationTypeCode { get; set; }

    public string? DestinationIndicatorCode { get; set; }

    public NFSeIbsCbsDestinationIndicator? DestinationIndicator { get; set; }

    public string? TaxStatusCode { get; set; }

    public string? TaxClassificationCode { get; set; }

    public string? PresumedCreditCode { get; set; }

    public NFSeIbsCbsRegularTaxation? RegularTaxation { get; set; }

    public NFSeIbsCbsDeferral? Deferral { get; set; }
}

public sealed class NFSeIbsCbsRegularTaxation
{
    public string? TaxStatusCode { get; set; }

    public string? TaxClassificationCode { get; set; }
}

public sealed class NFSeIbsCbsDeferral
{
    public decimal? StateIbsRate { get; set; }

    public decimal? MunicipalIbsRate { get; set; }

    public decimal? CbsRate { get; set; }
}

public sealed class NFSeTotalTax
{
    public string? IndicatorCode { get; set; }

    public NFSeTotalTaxIndicator? Indicator { get; set; }

    public decimal? SimplesNationalRate { get; set; }

    public NFSeTaxBreakdown? Monetary { get; set; }

    public NFSeTaxBreakdown? Percentage { get; set; }
}

public sealed class NFSeTaxBreakdown
{
    public decimal? Federal { get; set; }

    public decimal? State { get; set; }

    public decimal? Municipal { get; set; }
}
