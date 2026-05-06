using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Core.Options;

public sealed class NFSeSdkOptions
{
    public NFSeEnvironment Environment { get; set; } = NFSeEnvironment.ProductionRestricted;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public string UserAgent { get; set; } = "NFSeNacionalSdk";

    public X509Certificate2? ClientCertificate { get; set; }

    public NFSeCertificateFileOptions? CertificateFile { get; set; }
}

public sealed class NFSeCertificateFileOptions
{
    public string? Path { get; set; }

    public string? Password { get; set; }

    public X509KeyStorageFlags StorageFlags { get; set; } =
        X509KeyStorageFlags.UserKeySet |
        X509KeyStorageFlags.PersistKeySet |
        X509KeyStorageFlags.Exportable;
}
