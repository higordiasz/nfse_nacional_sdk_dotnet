using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk.Core.Options;

namespace NFSeNacionalSdk;

public static class NFSeCertificateLoader
{
    public static X509Certificate2? Load(NFSeSdkOptions options)
    {
        if (options is null) { throw new ArgumentNullException(nameof(options)); }

        if (options.ClientCertificate is not null)
        {
            return options.ClientCertificate;
        }

        return options.CertificateFile is null
            ? null
            : LoadFromPfxFile(options.CertificateFile);
    }

    public static X509Certificate2 LoadFromPfxFile(NFSeCertificateFileOptions options)
    {
        if (options is null) { throw new ArgumentNullException(nameof(options)); }

        var path = options.Path;
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Certificate file path must be informed.", nameof(options));
        }

        return LoadFromPfxFile(
            path!,
            options.Password,
            options.StorageFlags);
    }

    public static X509Certificate2 LoadFromPfxFile(
        string path,
        string? password,
        X509KeyStorageFlags storageFlags =
            X509KeyStorageFlags.UserKeySet |
            X509KeyStorageFlags.PersistKeySet |
            X509KeyStorageFlags.Exportable)
    {
        if (string.IsNullOrWhiteSpace(path)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(path)); }

#if NET9_0_OR_GREATER
        return System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadPkcs12FromFile(
            path,
            password,
            storageFlags);
#else
        return new X509Certificate2(path, password, storageFlags);
#endif
    }
}
