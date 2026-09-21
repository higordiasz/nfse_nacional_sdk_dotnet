using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

namespace NFSeNacionalSdk.IntegrationTests;

public sealed class RestrictedEnvironmentSmokeTests
{
    [NfseIntegrationFact]
    public async Task MunicipalConvention_ShouldReachRestrictedEnvironment()
    {
        var certificatePath = GetRequiredEnvironmentVariable("NFSE_INTEGRATION_CERTIFICATE_PATH");
        var municipalityCode = GetRequiredEnvironmentVariable("NFSE_INTEGRATION_MUNICIPALITY_CODE");
        using var client = NFSeClientFactory.Create(options =>
        {
            options.Environment = NFSeEnvironment.ProductionRestricted;
            options.LayoutProfile = NFSeLayoutDefaults.Current;
            options.CertificateFile = new NFSeCertificateFileOptions
            {
                Path = certificatePath,
                Password = Environment.GetEnvironmentVariable("NFSE_INTEGRATION_CERTIFICATE_PASSWORD"),
                StorageFlags = System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.EphemeralKeySet
            };
        });

        var result = await client.GetMunicipalConventionAsync(new GetMunicipalConventionRequest
        {
            MunicipalityCode = municipalityCode
        });

        Assert.True((int)result.StatusCode > 0);
        Assert.NotNull(result.RawJson);
    }

    private static string GetRequiredEnvironmentVariable(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"Integration environment variable '{name}' is required.");
}

public sealed class NfseIntegrationFactAttribute : FactAttribute
{
    public NfseIntegrationFactAttribute()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("NFSE_INTEGRATION_ENABLED"), "1", StringComparison.Ordinal))
        {
            Skip = "Set NFSE_INTEGRATION_ENABLED=1 and the documented certificate variables to run restricted-environment tests.";
        }
    }
}
