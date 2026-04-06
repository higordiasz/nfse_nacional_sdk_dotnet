namespace NFSeNacionalSdk.Contracts.Authentication;

public interface INFSeAuthenticationProvider
{
    Task<AuthenticationData> GetAuthenticationAsync(CancellationToken cancellationToken = default);
}