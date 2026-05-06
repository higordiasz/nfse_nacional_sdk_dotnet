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
        ArgumentNullException.ThrowIfNull(services);

        var options = new NFSeSdkOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(_ => NFSeEndpointsOptions.For(options.Environment));
        if (options.ClientCertificate is not null)
        {
            services.AddSingleton(options.ClientCertificate);
        }
        else if (!string.IsNullOrWhiteSpace(options.CertificateFile?.Path))
        {
            services.AddSingleton(_ => NFSeCertificateLoader.LoadFromPfxFile(options.CertificateFile));
        }

        services.AddSingleton<INFSeSerializer, NFSeXmlSerializer>();
        services.AddSingleton<INFSeTransport>(serviceProvider =>
        {
            var endpoints = serviceProvider.GetRequiredService<NFSeEndpointsOptions>();
            var resolvedCertificate = serviceProvider.GetService<X509Certificate2>();

            return new NFSeHttpTransport(
                endpoints,
                new NFSeHttpTransportOptions
                {
                    Timeout = options.Timeout,
                    UserAgent = options.UserAgent,
                    ClientCertificate = resolvedCertificate
                });
        });
        services.AddSingleton<INFSeClient>(serviceProvider =>
        {
            var transport = serviceProvider.GetRequiredService<INFSeTransport>();
            var serializer = serviceProvider.GetRequiredService<INFSeSerializer>();
            var endpoints = serviceProvider.GetRequiredService<NFSeEndpointsOptions>();
            var resolvedCertificate = serviceProvider.GetService<X509Certificate2>();

            return new NFSeClient(transport, serializer, endpoints, resolvedCertificate);
        });
        services.AddSingleton(serviceProvider => (NFSeClient)serviceProvider.GetRequiredService<INFSeClient>());

        return services;
    }
}
