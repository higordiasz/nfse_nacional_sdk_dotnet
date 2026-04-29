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
