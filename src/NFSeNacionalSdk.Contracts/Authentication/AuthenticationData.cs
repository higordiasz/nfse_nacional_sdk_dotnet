namespace NFSeNacionalSdk.Contracts.Authentication;

public sealed class AuthenticationData
{
    public string? BearerToken { get; init; }

    public string? ApiKey { get; init; }

    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}