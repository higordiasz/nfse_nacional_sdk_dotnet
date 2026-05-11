using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class CancelNfseRequest
{
    public string AccessKey { get; set; }

    public string AuthorTaxId { get; set; }

    public NFSeCancellationReasonCode ReasonCode { get; set; } = NFSeCancellationReasonCode.Other;

    public string Reason { get; set; }

    public DateTimeOffset? EventAt { get; set; }
}
