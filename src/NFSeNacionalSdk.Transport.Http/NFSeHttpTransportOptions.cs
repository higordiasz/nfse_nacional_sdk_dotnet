using System.Security.Cryptography.X509Certificates;

namespace NFSeNacionalSdk.Transport.Http;

public sealed class NFSeHttpTransportOptions
{
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);

    public string UserAgent { get; init; } = "NFSeNacionalSdk";

    public X509Certificate2? ClientCertificate { get; init; }
}
