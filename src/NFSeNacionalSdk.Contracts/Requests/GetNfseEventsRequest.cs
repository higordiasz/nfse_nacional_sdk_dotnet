namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class GetNfseEventsRequest
{
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>Optional six-digit event type code.</summary>
    public string? EventTypeCode { get; set; }

    /// <summary>Optional event sequence. Requires <see cref="EventTypeCode"/>.</summary>
    public int? SequenceNumber { get; set; }
}
