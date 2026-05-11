namespace NFSeNacionalSdk.Contracts.Authentication;

public sealed class AuthenticationData
{
    public string? BearerToken { get; set; }

    public string? ApiKey { get; set; }

    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}