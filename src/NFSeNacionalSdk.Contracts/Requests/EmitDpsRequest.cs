namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class EmitDpsRequest
{
    public required string MunicipalTaxpayerRegistration { get; init; }

    public required string ServiceCode { get; init; }

    public required decimal ServiceAmount { get; init; }

    public string Description { get; init; } = string.Empty;

    public string? ReferenceNumber { get; init; }
}