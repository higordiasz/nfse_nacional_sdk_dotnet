namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeEventDocument
{
    public string? Id { get; init; }

    public string? AccessKey { get; init; }

    public string? TypeCode { get; init; }

    public string? Description { get; init; }

    public int? SequenceNumber { get; init; }

    public DateTimeOffset? ProcessedAt { get; init; }

    public string? AuthorTaxId { get; init; }

    public string? ReasonCode { get; init; }

    public string? Reason { get; init; }
}
