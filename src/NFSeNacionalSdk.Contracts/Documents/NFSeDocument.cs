namespace NFSeNacionalSdk.Contracts.Documents;

public sealed class NFSeDocument
{
    public string? AccessKey { get; set; }

    public string? Number { get; set; }

    public string? VerificationCode { get; set; }

    public DateTimeOffset? IssuedAt { get; set; }

    public NFSeParty? Issuer { get; set; }

    public NFSeParty? Recipient { get; set; }

    public NFSeService? Service { get; set; }
}
