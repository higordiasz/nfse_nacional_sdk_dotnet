using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Core.Options;

public sealed class NFSeSdkOptions
{
    public NFSeEnvironment Environment { get; set; } = NFSeEnvironment.ProductionRestricted;

    public NFSeLayoutProfile LayoutProfile { get; set; } = NFSeLayoutDefaults.Current;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public string UserAgent { get; set; } = "NFSeNacionalSdk";

    public X509Certificate2? ClientCertificate { get; set; }

    public NFSeCertificateFileOptions? CertificateFile { get; set; }

    /// <summary>Optional certificate used only for XML signatures. Falls back to the mTLS certificate.</summary>
    public X509Certificate2? SigningCertificate { get; set; }

    /// <summary>Optional certificate file used only for XML signatures.</summary>
    public NFSeCertificateFileOptions? SigningCertificateFile { get; set; }

    public string? ApplicationName { get; set; }

    public string? ApplicationVersion { get; set; }

    /// <summary>Validates received NFS-e and event XML against the selected official schema. Does not verify signatures.</summary>
    public bool ValidateResponseXml { get; set; } = true;
}

public sealed class NFSeCertificateFileOptions
{
    public string? Path { get; set; }

    public string? Password { get; set; }

    // EphemeralKeySet (32) is absent from the netstandard2.0 reference enum but supported by current runtimes.
    public X509KeyStorageFlags StorageFlags { get; set; } = (X509KeyStorageFlags)32;
}
