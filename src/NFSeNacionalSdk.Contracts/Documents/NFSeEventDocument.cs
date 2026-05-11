namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeEventDocument
{
    public string? Id { get; set; }

    public string? AccessKey { get; set; }

    public string? TypeCode { get; set; }

    public string? Description { get; set; }

    public int? SequenceNumber { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public string? AuthorTaxId { get; set; }

    public string? ReasonCode { get; set; }

    public string? Reason { get; set; }
}
