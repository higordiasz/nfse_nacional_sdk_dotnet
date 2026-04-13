using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using NFSeNacionalSdk.Contracts.Clients;
using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Constants;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Transport.Http;

namespace NFSeNacionalSdk;

public sealed class NFSeClient : INFSeClient, IDisposable
{
    private readonly INFSeTransport _transport;
    private readonly INFSeSerializer _serializer;
    private readonly NFSeEndpointsOptions _endpoints;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly bool _disposeTransport;

    public NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(transport, serializer, endpoints, jsonSerializerOptions, disposeTransport: false)
    {
    }

    public NFSeClient(
        NFSeSdkOptions? options = null,
        X509Certificate2? clientCertificate = null,
        HttpClient? httpClient = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(CreateDefaultDependencies(options, clientCertificate, httpClient), jsonSerializerOptions)
    {
    }

    public async Task<GetNfseByAccessKeyResult> GetNfseByAccessKeyAsync(
        GetNfseByAccessKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var path = _endpoints.NfseByAccessKeyPath.Replace(
            "{chaveAcesso}",
            Uri.EscapeDataString(request.AccessKey),
            StringComparison.Ordinal);

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Get,
                Path = path,
                Accept = MediaTypes.ApplicationXml
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            throw new NFSeTransportException(
                $"NFSe consultation returned an empty XML payload with status code {(int)response.StatusCode}.");
        }

        Contracts.Serialization.NFSeLookupDeserializationResult lookupResult;

        try
        {
            lookupResult = _serializer.DeserializeLookupResponse(response.Content);
        }
        catch (NFSeSerializationException exception) when (!response.IsSuccessStatusCode)
        {
            throw new NFSeTransportException(
                $"NFSe consultation failed with status code {(int)response.StatusCode} and an unsupported error payload.",
                exception);
        }

        var document = lookupResult.Document;
        document?.AccessKey ??= request.AccessKey;

        return new GetNfseByAccessKeyResult
        {
            AccessKey = document?.AccessKey ?? request.AccessKey,
            Success = lookupResult.Success,
            RawXml = response.Content,
            Document = document,
            JsonContent = document is null
                ? null
                : JsonSerializer.Serialize(document, _jsonSerializerOptions),
            Messages = lookupResult.Messages,
            StatusCode = response.StatusCode
        };
    }

    public void Dispose()
    {
        if (_disposeTransport && _transport is IDisposable disposableTransport)
        {
            disposableTransport.Dispose();
        }
    }

    private static DefaultClientDependencies CreateDefaultDependencies(
        NFSeSdkOptions? options,
        X509Certificate2? clientCertificate,
        HttpClient? httpClient)
    {
        var resolvedOptions = options ?? new NFSeSdkOptions();
        var endpoints = NFSeEndpointsOptions.For(resolvedOptions.Environment);
        var transport = new NFSeHttpTransport(
            endpoints,
            new NFSeHttpTransportOptions
            {
                Timeout = resolvedOptions.Timeout,
                UserAgent = resolvedOptions.UserAgent,
                ClientCertificate = clientCertificate
            },
            httpClient);

        return new DefaultClientDependencies(
            transport,
            new NFSeXmlSerializer(),
            endpoints);
    }

    private static JsonSerializerOptions CreateDefaultJsonSerializerOptions(JsonSerializerOptions? options)
    {
        if (options is not null)
        {
            return options;
        }

        return new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }

    private NFSeClient(DefaultClientDependencies dependencies, JsonSerializerOptions? jsonSerializerOptions)
        : this(
            dependencies.Transport,
            dependencies.Serializer,
            dependencies.Endpoints,
            jsonSerializerOptions,
            disposeTransport: true)
    {
    }

    private NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        JsonSerializerOptions? jsonSerializerOptions,
        bool disposeTransport)
    {
        ArgumentNullException.ThrowIfNull(transport);
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(endpoints);

        _transport = transport;
        _serializer = serializer;
        _endpoints = endpoints;
        _jsonSerializerOptions = CreateDefaultJsonSerializerOptions(jsonSerializerOptions);
        _disposeTransport = disposeTransport;
    }

    private sealed record DefaultClientDependencies(
        INFSeTransport Transport,
        INFSeSerializer Serializer,
        NFSeEndpointsOptions Endpoints);
}
