using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using NFSeNacionalSdk.Contracts.Clients;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Constants;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.SefinNational;
using NFSeNacionalSdk.Transport.Http;

namespace NFSeNacionalSdk;

public sealed class NFSeClient : INFSeClient, IDisposable
{
    private readonly INFSeTransport _transport;
    private readonly INFSeSerializer _serializer;
    private readonly NFSeEndpointsOptions _endpoints;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly X509Certificate2? _signingCertificate;
    private readonly string _applicationVersion;
    private readonly bool _disposeTransport;

    public NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(
            transport,
            serializer,
            endpoints,
            signingCertificate: null,
            jsonSerializerOptions,
            disposeTransport: false)
    {
    }

    public NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        X509Certificate2? signingCertificate,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(
            transport,
            serializer,
            endpoints,
            signingCertificate,
            jsonSerializerOptions,
            disposeTransport: false)
    {
    }

    public NFSeClient(
        NFSeSdkOptions? options = null,
        X509Certificate2? clientCertificate = null,
        HttpClient? httpClient = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(
            CreateDefaultDependencies(options, clientCertificate, httpClient),
            clientCertificate,
            jsonSerializerOptions)
    {
    }

    public async Task<EmitDpsResponse> EmitDpsAsync(
        EmitDpsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_signingCertificate is null)
        {
            throw new NFSeSerializationException(
                "A signing certificate must be configured on the NFSe client to generate and sign DPS XML.");
        }

        var serializationResult = _serializer.SerializeSignedDps(
            request,
            new EmitDpsSerializationContext
            {
                Environment = _endpoints.Environment,
                SigningCertificate = _signingCertificate,
                ApplicationVersion = _applicationVersion
            });

        var payload = JsonSerializer.Serialize(
            new SefinNationalTransmissionRequest
            {
                DpsXmlGZipBase64 = SefinNationalCompressedDocumentEncoder.EncodeGZipBase64(serializationResult.XmlContent)
            },
            _jsonSerializerOptions);

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Post,
                Path = _endpoints.NfsePath,
                Content = payload,
                ContentType = MediaTypes.ApplicationJson,
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            throw new NFSeTransportException(
                $"NFSe emission returned an empty payload with status code {(int)response.StatusCode}.");
        }

        var apiEnvelope = DeserializeTransmissionApiEnvelope(response.Content);
        var rawXml = TryDecodeXml(apiEnvelope);

        if (rawXml is not null)
        {
            var lookupResult = DeserializeLookupXml(rawXml, response.StatusCode);
            var document = lookupResult.Document;
            document?.AccessKey ??= apiEnvelope.AccessKey;

            var messages = BuildMessages(apiEnvelope.Alerts);
            if (lookupResult.Messages.Count > 0)
            {
                messages = [..messages, ..lookupResult.Messages];
            }

            return new EmitDpsResponse
            {
                Success = lookupResult.Success && document is not null && response.IsSuccessStatusCode,
                DpsId = apiEnvelope.GetResolvedDpsId() ?? serializationResult.DpsId,
                AccessKey = document?.AccessKey ?? apiEnvelope.AccessKey,
                SubmittedDpsXml = serializationResult.XmlContent,
                RawXml = rawXml,
                Document = document,
                JsonContent = document is null
                    ? null
                    : JsonSerializer.Serialize(document, _jsonSerializerOptions),
                Messages = messages,
                StatusCode = response.StatusCode
            };
        }

        var errorMessages = BuildMessages(apiEnvelope.Errors);
        if (errorMessages.Count == 0)
        {
            throw new NFSeTransportException(
                $"NFSe emission failed with status code {(int)response.StatusCode} and returned an unsupported JSON payload.");
        }

        return new EmitDpsResponse
        {
            Success = false,
            DpsId = apiEnvelope.GetResolvedDpsId() ?? serializationResult.DpsId,
            AccessKey = apiEnvelope.AccessKey,
            SubmittedDpsXml = serializationResult.XmlContent,
            RawXml = null,
            Document = null,
            JsonContent = null,
            Messages = errorMessages,
            StatusCode = response.StatusCode
        };
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
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            throw new NFSeTransportException(
                $"NFSe consultation returned an empty payload with status code {(int)response.StatusCode}.");
        }

        var apiEnvelope = DeserializeLookupApiEnvelope(response.Content);
        var rawXml = TryDecodeXml(apiEnvelope);
        var lookupResult = rawXml is null
            ? CreateBusinessErrorResult(apiEnvelope, response.StatusCode)
            : DeserializeLookupXml(rawXml, response.StatusCode);

        var document = lookupResult.Document;
        document?.AccessKey ??= apiEnvelope.AccessKey ?? request.AccessKey;

        return new GetNfseByAccessKeyResult
        {
            AccessKey = document?.AccessKey ?? apiEnvelope.AccessKey ?? request.AccessKey,
            Success = lookupResult.Success,
            RawXml = rawXml,
            Document = document,
            JsonContent = document is null
                ? null
                : JsonSerializer.Serialize(document, _jsonSerializerOptions),
            Messages = lookupResult.Messages,
            StatusCode = response.StatusCode
        };
    }

    public async Task<GetDpsByIdResult> GetDpsByIdAsync(
        GetDpsByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Get,
                Path = BuildDpsByIdPath(request.DpsId),
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            throw new NFSeTransportException(
                $"DPS consultation returned an empty payload with status code {(int)response.StatusCode}.");
        }

        var apiEnvelope = DeserializeDpsLookupApiEnvelope(response.Content);
        var messages = BuildMessages(apiEnvelope.Errors);

        if (apiEnvelope.Error is not null)
        {
            messages = [..messages, CreateMessage(apiEnvelope.Error)];
        }

        var accessKey = NormalizeOptionalText(apiEnvelope.AccessKey);
        if (accessKey is null && messages.Count == 0)
        {
            throw new NFSeTransportException(
                $"DPS consultation failed with status code {(int)response.StatusCode} and returned an unsupported JSON payload.");
        }

        return new GetDpsByIdResult
        {
            DpsId = NormalizeOptionalText(apiEnvelope.GetResolvedDpsId()) ?? request.DpsId,
            AccessKey = accessKey,
            Success = response.IsSuccessStatusCode && accessKey is not null,
            Messages = messages,
            StatusCode = response.StatusCode
        };
    }

    public async Task<CheckDpsByIdResult> CheckDpsByIdAsync(
        GetDpsByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Head,
                Path = BuildDpsByIdPath(request.DpsId),
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        return new CheckDpsByIdResult
        {
            DpsId = request.DpsId,
            Generated = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode
        };
    }

    public async Task<GetMunicipalConventionResult> GetMunicipalConventionAsync(
        GetMunicipalConventionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Get,
                Path = BuildMunicipalConventionPath(request.MunicipalityCode),
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return new GetMunicipalConventionResult
            {
                MunicipalityCode = request.MunicipalityCode,
                IsAvailable = response.IsSuccessStatusCode,
                JsonContent = null,
                Messages = response.IsSuccessStatusCode
                    ? Array.Empty<NFSeMessage>()
                    : new NFSeMessage[]
                    {
                        new NFSeMessage
                        {
                            Description = $"Municipal convention lookup returned an empty payload with status code {(int)response.StatusCode}."
                        }
                    },
                StatusCode = response.StatusCode
            };
        }

        var apiEnvelope = DeserializeMunicipalConventionApiEnvelope(response.Content);
        var messages = BuildMessages(apiEnvelope.Errors);

        if (apiEnvelope.Error is not null)
        {
            messages = [..messages, CreateMessage(apiEnvelope.Error)];
        }

        return new GetMunicipalConventionResult
        {
            MunicipalityCode = request.MunicipalityCode,
            IsAvailable = response.IsSuccessStatusCode && messages.Count == 0,
            JsonContent = response.Content,
            Messages = messages,
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

    private static string BuildApplicationVersion()
    {
        var version = typeof(NFSeClient).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        version = version?.Split('+', 2)[0];

        if (string.IsNullOrWhiteSpace(version))
        {
            version = typeof(NFSeClient).Assembly.GetName().Version?.ToString(3);
        }

        return string.IsNullOrWhiteSpace(version)
            ? "NFSeNacionalSdk"
            : LimitApplicationVersion($"NFSeSdk_{version}");
    }

    private static string LimitApplicationVersion(string value)
    {
        const int maxLength = 20;

        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }

    private NFSeClient(
        DefaultClientDependencies dependencies,
        X509Certificate2? signingCertificate,
        JsonSerializerOptions? jsonSerializerOptions)
        : this(
            dependencies.Transport,
            dependencies.Serializer,
            dependencies.Endpoints,
            signingCertificate,
            jsonSerializerOptions,
            disposeTransport: true)
    {
    }

    private NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        X509Certificate2? signingCertificate,
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
        _signingCertificate = signingCertificate;
        _applicationVersion = BuildApplicationVersion();
        _disposeTransport = disposeTransport;
    }

    private Contracts.Serialization.NFSeLookupDeserializationResult DeserializeLookupXml(
        string rawXml,
        HttpStatusCode statusCode)
    {
        try
        {
            return _serializer.DeserializeLookupResponse(rawXml);
        }
        catch (NFSeSerializationException exception) when ((int)statusCode >= 400)
        {
            throw new NFSeTransportException(
                $"NFSe operation failed with status code {(int)statusCode} and returned an unsupported XML payload.",
                exception);
        }
    }

    private static string? TryDecodeXml(SefinNationalLookupApiEnvelope envelope)
    {
        return string.IsNullOrWhiteSpace(envelope.NfseXmlGZipBase64)
            ? null
            : SefinNationalCompressedDocumentDecoder.DecodeGZipBase64(envelope.NfseXmlGZipBase64);
    }

    private static string? TryDecodeXml(SefinNationalTransmissionApiEnvelope envelope)
    {
        return string.IsNullOrWhiteSpace(envelope.NfseXmlGZipBase64)
            ? null
            : SefinNationalCompressedDocumentDecoder.DecodeGZipBase64(envelope.NfseXmlGZipBase64);
    }

    private static Contracts.Serialization.NFSeLookupDeserializationResult CreateBusinessErrorResult(
        SefinNationalLookupApiEnvelope envelope,
        HttpStatusCode statusCode)
    {
        if (envelope.Error is null)
        {
            throw new NFSeTransportException(
                $"NFSe consultation failed with status code {(int)statusCode} and returned an unsupported JSON payload.");
        }

        return new Contracts.Serialization.NFSeLookupDeserializationResult
        {
            Success = false,
            Messages =
            [
                CreateMessage(envelope.Error)
            ]
        };
    }

    private static NFSeMessage CreateMessage(SefinNationalApiMessage message)
    {
        return new NFSeMessage
        {
            Code = message.Code,
            Description = message.GetResolvedDescription()
                ?? "The SEFIN API returned a message without description."
        };
    }

    private static IReadOnlyList<NFSeMessage> BuildMessages(IReadOnlyList<SefinNationalApiMessage>? messages)
    {
        return messages is null || messages.Count == 0
            ? Array.Empty<NFSeMessage>()
            : [..messages.Select(CreateMessage)];
    }

    private string BuildDpsByIdPath(string dpsId)
    {
        return _endpoints.DpsByIdPath.Replace(
            "{id}",
            Uri.EscapeDataString(dpsId),
            StringComparison.Ordinal);
    }

    private string BuildMunicipalConventionPath(string municipalityCode)
    {
        var path = _endpoints.MunicipalParametersByConventionPath.Replace(
            "{codigoMunicipio}",
            Uri.EscapeDataString(municipalityCode),
            StringComparison.Ordinal);

        return new Uri(
            new Uri(_endpoints.ParametrizationBaseUrl, UriKind.Absolute),
            path.TrimStart('/')).ToString();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private SefinNationalLookupApiEnvelope DeserializeLookupApiEnvelope(string content)
    {
        return DeserializeJson<SefinNationalLookupApiEnvelope>(
            content,
            "The SEFIN API returned an empty JSON object for the NFS-e lookup.",
            "Failed to deserialize the JSON payload returned by the SEFIN API for the NFS-e lookup.");
    }

    private SefinNationalDpsLookupApiEnvelope DeserializeDpsLookupApiEnvelope(string content)
    {
        return DeserializeJson<SefinNationalDpsLookupApiEnvelope>(
            content,
            "The SEFIN API returned an empty JSON object for the DPS lookup.",
            "Failed to deserialize the JSON payload returned by the SEFIN API for the DPS lookup.");
    }

    private SefinNationalMunicipalConventionApiEnvelope DeserializeMunicipalConventionApiEnvelope(string content)
    {
        return DeserializeJson<SefinNationalMunicipalConventionApiEnvelope>(
            content,
            "The NFSe API returned an empty JSON object for the municipal convention lookup.",
            "Failed to deserialize the JSON payload returned by the NFSe API for the municipal convention lookup.");
    }

    private SefinNationalTransmissionApiEnvelope DeserializeTransmissionApiEnvelope(string content)
    {
        return DeserializeJson<SefinNationalTransmissionApiEnvelope>(
            content,
            "The SEFIN API returned an empty JSON object for the DPS emission.",
            "Failed to deserialize the JSON payload returned by the SEFIN API for the DPS emission.");
    }

    private T DeserializeJson<T>(string content, string nullMessage, string errorMessage)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions)
                ?? throw new NFSeSerializationException(nullMessage);
        }
        catch (JsonException exception)
        {
            throw new NFSeSerializationException(errorMessage, exception);
        }
    }

    private sealed record DefaultClientDependencies(
        INFSeTransport Transport,
        INFSeSerializer Serializer,
        NFSeEndpointsOptions Endpoints);
}
