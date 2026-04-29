namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeValues
{
    public decimal? ServiceAmount { get; set; }

    public decimal? AmountReceivedByIntermediary { get; set; }

    public decimal? UnconditionalDiscountAmount { get; set; }

    public decimal? ConditionalDiscountAmount { get; set; }

    public decimal? NetAmount { get; set; }
}
