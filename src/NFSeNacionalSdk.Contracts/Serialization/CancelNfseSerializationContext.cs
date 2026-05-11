using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Serialization;

public sealed class CancelNfseSerializationContext
{
    public NFSeEnvironment Environment { get; set; }

    public X509Certificate2 SigningCertificate { get; set; }

    public string? ApplicationVersion { get; set; }
}
