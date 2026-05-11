using System.Security.Cryptography.X509Certificates;

namespace NFSeNacionalSdk.Transport.Http;

public sealed class NFSeHttpTransportOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public string UserAgent { get; set; } = "NFSeNacionalSdk";

    public X509Certificate2? ClientCertificate { get; set; }
}
