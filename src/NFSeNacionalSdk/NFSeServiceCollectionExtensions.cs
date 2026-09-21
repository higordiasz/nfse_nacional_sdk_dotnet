using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using NFSeNacionalSdk.Contracts.Clients;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Transport.Http;

namespace NFSeNacionalSdk;

public static class NFSeServiceCollectionExtensions
{
    public static IServiceCollection AddNFSeNacionalSdk(
        this IServiceCollection services,
        Action<NFSeSdkOptions>? configure = null)
    {
        if (services is null) { throw new ArgumentNullException(nameof(services)); }

        var options = new NFSeSdkOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(_ => NFSeEndpointsOptions.For(options.Environment));
        services.AddSingleton(_ => CertificatePair.Create(options));

        services.AddSingleton<INFSeSerializer, NFSeXmlSerializer>();
        services.AddSingleton<INFSeTransport>(serviceProvider =>
        {
            var endpoints = serviceProvider.GetRequiredService<NFSeEndpointsOptions>();
            var certificates = serviceProvider.GetRequiredService<CertificatePair>();

            return new NFSeHttpTransport(
                endpoints,
                new NFSeHttpTransportOptions
                {
                    Timeout = options.Timeout,
                    UserAgent = options.UserAgent,
                    ClientCertificate = certificates.ClientCertificate
                });
        });
        services.AddSingleton<INFSeClient>(serviceProvider =>
        {
            var transport = serviceProvider.GetRequiredService<INFSeTransport>();
            var serializer = serviceProvider.GetRequiredService<INFSeSerializer>();
            var endpoints = serviceProvider.GetRequiredService<NFSeEndpointsOptions>();
            var certificates = serviceProvider.GetRequiredService<CertificatePair>();

            return new NFSeClient(transport, serializer, endpoints, certificates.SigningCertificate, options);
        });
        services.AddSingleton(serviceProvider => (NFSeClient)serviceProvider.GetRequiredService<INFSeClient>());

        return services;
    }

    private sealed class CertificatePair : IDisposable
    {
        private CertificatePair(
            X509Certificate2? clientCertificate,
            X509Certificate2? signingCertificate,
            bool ownsClientCertificate,
            bool ownsSigningCertificate)
        {
            ClientCertificate = clientCertificate;
            SigningCertificate = signingCertificate;
            _ownsClientCertificate = ownsClientCertificate;
            _ownsSigningCertificate = ownsSigningCertificate;
        }

        private readonly bool _ownsClientCertificate;
        private readonly bool _ownsSigningCertificate;

        public X509Certificate2? ClientCertificate { get; }

        public X509Certificate2? SigningCertificate { get; }

        public static CertificatePair Create(NFSeSdkOptions options)
        {
            var ownsClient = options.ClientCertificate is null && !string.IsNullOrWhiteSpace(options.CertificateFile?.Path);
            var client = NFSeCertificateLoader.Load(options);
            var ownsSigning = options.SigningCertificate is null &&
                !string.IsNullOrWhiteSpace(options.SigningCertificateFile?.Path);
            var signing = options.SigningCertificate ??
                (options.SigningCertificateFile is null
                    ? null
                    : NFSeCertificateLoader.LoadFromPfxFile(options.SigningCertificateFile)) ??
                client;
            return new CertificatePair(client, signing, ownsClient, ownsSigning);
        }

        public void Dispose()
        {
            if (_ownsSigningCertificate)
            {
                SigningCertificate?.Dispose();
            }

            if (_ownsClientCertificate && !ReferenceEquals(ClientCertificate, SigningCertificate))
            {
                ClientCertificate?.Dispose();
            }
        }
    }
}
