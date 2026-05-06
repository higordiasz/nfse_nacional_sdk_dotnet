using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using NFSeNacionalSdk.Core.Options;

namespace NFSeNacionalSdk;

public static class NFSeClientFactory
{
    public static NFSeClient Create(
        Action<NFSeSdkOptions>? configure = null,
        HttpClient? httpClient = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var options = new NFSeSdkOptions();
        configure?.Invoke(options);

        return Create(options, httpClient, jsonSerializerOptions);
    }

    public static NFSeClient Create(
        NFSeSdkOptions options,
        HttpClient? httpClient = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new NFSeClient(
            options,
            clientCertificate: null,
            httpClient,
            jsonSerializerOptions);
    }

    public static NFSeClient Create(
        NFSeSdkOptions options,
        X509Certificate2 certificate,
        HttpClient? httpClient = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(certificate);

        return new NFSeClient(
            options,
            certificate,
            httpClient,
            jsonSerializerOptions);
    }
}
