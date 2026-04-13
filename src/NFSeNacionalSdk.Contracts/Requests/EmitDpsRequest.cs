using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class EmitDpsRequest
{
    public required string Series { get; init; }

    public required string Number { get; init; }

    public required DateOnly CompetenceDate { get; init; }

    public required DateTimeOffset IssuedAt { get; init; }

    public required string MunicipalityCode { get; init; }

    public NFSeDpsEmitterType EmitterType { get; init; } = NFSeDpsEmitterType.Provider;

    public required EmitDpsProvider Provider { get; init; }

    public EmitDpsRecipient? Recipient { get; init; }

    public required EmitDpsService Service { get; init; }

    public required EmitDpsTaxation Taxation { get; init; }
}

public sealed class EmitDpsProvider
{
    public required string TaxId { get; init; }

    public string? MunicipalRegistration { get; init; }

    public string? Name { get; init; }

    public EmitDpsAddress? Address { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }

    public NFSeSimplesNationalOption SimplesNationalOption { get; init; }

    public NFSeSimplifiedNationalTaxRegime? SimplifiedNationalTaxRegime { get; init; }

    public NFSeSpecialTaxRegime SpecialTaxRegime { get; init; } = NFSeSpecialTaxRegime.None;
}

public sealed class EmitDpsRecipient
{
    public required string TaxId { get; init; }

    public required string Name { get; init; }

    public string? MunicipalRegistration { get; init; }

    public EmitDpsAddress? Address { get; init; }

    public string? Phone { get; init; }

    public string? Email { get; init; }
}

public sealed class EmitDpsAddress
{
    public required string MunicipalityCode { get; init; }

    public required string ZipCode { get; init; }

    public required string Street { get; init; }

    public required string Number { get; init; }

    public string? Complement { get; init; }

    public required string Neighborhood { get; init; }
}

public sealed class EmitDpsService
{
    public string? ServiceLocationMunicipalityCode { get; init; }

    public required string NationalTaxationCode { get; init; }

    public string? MunicipalTaxationCode { get; init; }

    public required string Description { get; init; }

    public string? NationalClassificationCode { get; init; }

    public string? InternalCode { get; init; }

    public required decimal Amount { get; init; }
}

public sealed class EmitDpsTaxation
{
    public NFSeIssTaxationType IssTaxationType { get; init; } = NFSeIssTaxationType.TaxableOperation;

    public NFSeIssWithholdingType IssWithholdingType { get; init; } = NFSeIssWithholdingType.NotWithheld;

    public NFSeTotalTaxIndicator TotalTaxIndicator { get; init; } = NFSeTotalTaxIndicator.NotInformed;
}
