using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class CancelNfseRequest
{
    public required string AccessKey { get; init; }

    public required string AuthorTaxId { get; init; }

    public NFSeCancellationReasonCode ReasonCode { get; init; } = NFSeCancellationReasonCode.Other;

    public required string Reason { get; init; }

    public DateTimeOffset? EventAt { get; init; }
}
