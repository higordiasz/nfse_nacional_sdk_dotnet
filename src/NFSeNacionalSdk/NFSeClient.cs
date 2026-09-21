using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using NFSeNacionalSdk.Contracts.Clients;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Constants;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Serialization.Xml.Events;
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
    private readonly NFSeLayoutProfile _layoutProfile;
    private readonly bool _validateResponseXml;
    private readonly X509Certificate2? _ownedClientCertificate;
    private readonly bool _disposeTransport;
    private readonly bool _disposeSigningCertificate;
    private readonly bool _disposeClientCertificate;

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
            NFSeLayoutDefaults.Current,
            BuildApplicationVersion(),
            validateResponseXml: false,
            disposeTransport: false,
            disposeSigningCertificate: false,
            ownedClientCertificate: null,
            disposeClientCertificate: false)
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
            NFSeLayoutDefaults.Current,
            BuildApplicationVersion(),
            validateResponseXml: false,
            disposeTransport: false,
            disposeSigningCertificate: false,
            ownedClientCertificate: null,
            disposeClientCertificate: false)
    {
    }

    public NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        X509Certificate2? signingCertificate,
        NFSeSdkOptions sdkOptions,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(
            transport,
            serializer,
            endpoints,
            signingCertificate,
            jsonSerializerOptions,
            sdkOptions?.LayoutProfile ?? throw new ArgumentNullException(nameof(sdkOptions)),
            BuildApplicationVersion(sdkOptions),
            sdkOptions.ValidateResponseXml,
            disposeTransport: false,
            disposeSigningCertificate: false,
            ownedClientCertificate: null,
            disposeClientCertificate: false)
    {
    }

    public NFSeClient(
        NFSeSdkOptions? options = null,
        X509Certificate2? clientCertificate = null,
        HttpClient? httpClient = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
        : this(
            CreateDefaultDependencies(options, clientCertificate, httpClient),
            jsonSerializerOptions)
    {
    }

    public async Task<CancelNfseResult> CancelNfseAsync(
        CancelNfseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

        if (_signingCertificate is null)
        {
            throw new NFSeSerializationException(
                "A signing certificate must be configured on the NFSe client to generate and sign cancellation event XML.");
        }

        var serializationResult = _serializer.SerializeSignedCancellation(
            request,
            new CancelNfseSerializationContext
            {
                Environment = _endpoints.Environment,
                LayoutProfile = _layoutProfile,
                SigningCertificate = _signingCertificate,
                ApplicationVersion = _applicationVersion
            });
        var normalizedAccessKey = ExtractAccessKeyFromEventRequestId(serializationResult.EventRequestId);

        var payload = JsonSerializer.Serialize(
            new SefinNationalEventRequest
            {
                EventRequestXmlGZipBase64 = SefinNationalCompressedDocumentEncoder.EncodeGZipBase64(serializationResult.XmlContent)
            },
            _jsonSerializerOptions);

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Post,
                Path = BuildNfseEventsPath(normalizedAccessKey),
                Content = payload,
                ContentType = MediaTypes.ApplicationJson,
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            var emptyPayloadMessages = new NFSeMessage[]
                {
                    new NFSeMessage
                    {
                        Description = $"NFS-e cancellation returned an empty payload with status code {(int)response.StatusCode}."
                    }
                };

            return new CancelNfseResult
            {
                AccessKey = normalizedAccessKey,
                Success = false,
                EventId = serializationResult.EventRequestId,
                SubmittedEventXml = serializationResult.XmlContent,
                RawJson = null,
                RawXml = null,
                Event = null,
                Messages = emptyPayloadMessages,
                StatusCode = response.StatusCode
            };
        }

        var apiEnvelope = DeserializeEventApiEnvelope(response.Content!);
        var rawXml = TryDecodeEventXml(apiEnvelope);
        if (rawXml is not null && _validateResponseXml)
        {
            new NFSeReceivedXmlValidator().ValidateEvent(rawXml, _layoutProfile);
        }

        var eventDocument = rawXml is null
            ? null
            : new NFSeEventXmlResponseParser().Deserialize(rawXml);
        var errorMessages = BuildMessages(apiEnvelope.Errors);
        var standardErrorMessages = BuildMessages(apiEnvelope.Error);
        var alertMessages = BuildMessages(apiEnvelope.Alerts);
        var messages = errorMessages
            .Concat(standardErrorMessages)
            .Concat(alertMessages)
            .ToArray();

        if (response.IsSuccessStatusCode && rawXml is null && errorMessages.Count == 0 && standardErrorMessages.Count == 0)
        {
            messages =
            [
                ..messages,
                new NFSeMessage
                {
                    Description = "NFS-e cancellation succeeded at HTTP level but did not return eventoXmlGZipB64."
                }
            ];
        }

        return new CancelNfseResult
        {
            AccessKey = eventDocument?.AccessKey ?? apiEnvelope.AccessKey ?? normalizedAccessKey,
            Success = response.IsSuccessStatusCode &&
                rawXml is not null &&
                errorMessages.Count == 0 &&
                standardErrorMessages.Count == 0,
            EventId = eventDocument?.Id ?? apiEnvelope.EventRequestId ?? serializationResult.EventRequestId,
            SubmittedEventXml = serializationResult.XmlContent,
            RawJson = response.Content,
            RawXml = rawXml,
            Event = eventDocument,
            Messages = messages,
            StatusCode = response.StatusCode
        };
    }

    public async Task<EmitDpsResponse> EmitDpsAsync(
        EmitDpsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

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
                LayoutProfile = _layoutProfile,
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

        var apiEnvelope = DeserializeTransmissionApiEnvelope(response.Content!);
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
                RawJson = response.Content,
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
        if (apiEnvelope.Error is not null)
        {
            errorMessages = [..errorMessages, CreateMessage(apiEnvelope.Error)];
        }

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
            RawJson = response.Content,
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
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

        var path = _endpoints.NfseByAccessKeyPath.Replace(
            "{chaveAcesso}",
            Uri.EscapeDataString(request.AccessKey));

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

        var apiEnvelope = DeserializeLookupApiEnvelope(response.Content!);
        var rawXml = TryDecodeXml(apiEnvelope);
        var lookupResult = rawXml is null
            ? CreateBusinessErrorResult(apiEnvelope, response.StatusCode)
            : DeserializeLookupXml(rawXml, response.StatusCode);

        var document = lookupResult.Document;
        document?.AccessKey ??= apiEnvelope.AccessKey ?? request.AccessKey;

        return new GetNfseByAccessKeyResult
        {
            AccessKey = document?.AccessKey ?? apiEnvelope.AccessKey ?? request.AccessKey,
            Success = response.IsSuccessStatusCode && lookupResult.Success && document is not null,
            RawJson = response.Content,
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
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

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

        var apiEnvelope = DeserializeDpsLookupApiEnvelope(response.Content!);
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
            RawJson = response.Content,
            Messages = messages,
            StatusCode = response.StatusCode
        };
    }

    public async Task<GetNfseEventsResult> GetNfseEventsAsync(
        GetNfseEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }
        if (request.SequenceNumber is not null && string.IsNullOrWhiteSpace(request.EventTypeCode))
        {
            throw new ArgumentException("EventTypeCode must be informed when SequenceNumber is used.", nameof(request));
        }

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Get,
                Path = BuildNfseEventsLookupPath(request),
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return new GetNfseEventsResult
            {
                AccessKey = request.AccessKey,
                Success = false,
                Messages = [new NFSeMessage
                {
                    Description = $"NFS-e event lookup returned an empty payload with status code {(int)response.StatusCode}."
                }],
                StatusCode = response.StatusCode
            };
        }

        JsonDocument jsonDocument;
        try
        {
            jsonDocument = JsonDocument.Parse(response.Content!);
        }
        catch (JsonException exception)
        {
            throw new NFSeSerializationException("Failed to deserialize the NFS-e event lookup JSON payload.", exception);
        }

        using (jsonDocument)
        {
            var rawXmlDocuments = FindCompressedEventDocuments(jsonDocument.RootElement)
                .Select(SefinNationalCompressedDocumentDecoder.DecodeGZipBase64)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var events = new List<Contracts.Documents.NFSeEventDocument>(rawXmlDocuments.Length);
            var validator = new NFSeReceivedXmlValidator();
            var parser = new NFSeEventXmlResponseParser();
            foreach (var rawXml in rawXmlDocuments)
            {
                if (_validateResponseXml)
                {
                    validator.ValidateEvent(rawXml, _layoutProfile);
                }

                events.Add(parser.Deserialize(rawXml));
            }

            var errorMessages = FindMessageElements(jsonDocument.RootElement, "erro", "erros")
                .SelectMany(BuildMessagesFromElement)
                .ToArray();
            var alertMessages = FindMessageElements(jsonDocument.RootElement, "alertas")
                .SelectMany(BuildMessagesFromElement)
                .ToArray();
            var messages = errorMessages.Concat(alertMessages).ToList();
            if (response.IsSuccessStatusCode && events.Count == 0 && errorMessages.Length == 0)
            {
                messages.Add(new NFSeMessage
                {
                    Description = "NFS-e event lookup succeeded at HTTP level but returned no eventoXmlGZipB64 document."
                });
            }

            return new GetNfseEventsResult
            {
                AccessKey = events.FirstOrDefault()?.AccessKey ?? request.AccessKey,
                Success = response.IsSuccessStatusCode && events.Count > 0 && errorMessages.Length == 0,
                Events = events,
                RawXmlDocuments = rawXmlDocuments,
                RawJson = response.Content,
                JsonContent = events.Count == 0 ? null : JsonSerializer.Serialize(events, _jsonSerializerOptions),
                Messages = messages,
                StatusCode = response.StatusCode
            };
        }
    }

    public async Task<CheckDpsByIdResult> CheckDpsByIdAsync(
        GetDpsByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

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
            RawJson = response.Content,
            StatusCode = response.StatusCode
        };
    }

    public async Task<GetMunicipalConventionResult> GetMunicipalConventionAsync(
        GetMunicipalConventionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

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
                IsAvailable = false,
                JsonContent = null,
                RawJson = null,
                Messages = new NFSeMessage[]
                    {
                        new NFSeMessage
                        {
                            Description = $"Municipal convention lookup returned an empty payload with status code {(int)response.StatusCode}."
                        }
                    },
                StatusCode = response.StatusCode
            };
        }

        var apiEnvelope = DeserializeMunicipalConventionApiEnvelope(response.Content!);
        var messages = BuildMessages(apiEnvelope.Errors);

        if (apiEnvelope.Error is not null)
        {
            messages = [..messages, CreateMessage(apiEnvelope.Error)];
        }

        if (!response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(apiEnvelope.Message))
        {
            messages = [..messages, new NFSeMessage { Description = apiEnvelope.Message! }];
        }

        var parameters = apiEnvelope.Parameters is null
            ? null
            : new MunicipalConventionParameters
            {
                ConventionType = apiEnvelope.Parameters.ConventionType,
                UsesNationalEnvironment = apiEnvelope.Parameters.UsesNationalEnvironment,
                UsesNationalIssuer = apiEnvelope.Parameters.UsesNationalIssuer,
                DefaultFederalTaxpayerIssuanceStatus = apiEnvelope.Parameters.DefaultFederalTaxpayerIssuanceStatus,
                UsesNationalSupportModule = apiEnvelope.Parameters.UsesNationalSupportModule,
                AllowsTaxCredits = apiEnvelope.Parameters.AllowsTaxCredits
            };

        if (response.IsSuccessStatusCode && parameters is null && messages.Count == 0)
        {
            messages = [..messages, new NFSeMessage { Description = "Municipal convention lookup returned no parametrosConvenio data." }];
        }

        return new GetMunicipalConventionResult
        {
            MunicipalityCode = request.MunicipalityCode,
            IsAvailable = response.IsSuccessStatusCode && parameters is not null && messages.Count == 0,
            Parameters = parameters,
            JsonContent = response.Content,
            RawJson = response.Content,
            Messages = messages,
            StatusCode = response.StatusCode
        };
    }

    public async Task<GetMunicipalServiceParametersResult> GetMunicipalServiceParametersAsync(
        GetMunicipalServiceParametersRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }

        var response = await _transport.SendAsync(
            new TransportRequest
            {
                Method = HttpMethod.Get,
                Path = BuildMunicipalServiceParametersPath(
                    request.MunicipalityCode,
                    request.ServiceCode,
                    request.CompetenceDate),
                Accept = MediaTypes.ApplicationJson
            },
            cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.Content))
        {
            return new GetMunicipalServiceParametersResult
            {
                MunicipalityCode = request.MunicipalityCode,
                ServiceCode = request.ServiceCode,
                CompetenceDate = request.CompetenceDate,
                IsAvailable = false,
                JsonContent = null,
                RawJson = null,
                Messages = new NFSeMessage[]
                    {
                        new NFSeMessage
                        {
                            Description = $"Municipal service parameters lookup returned an empty payload with status code {(int)response.StatusCode}."
                        }
                    },
                StatusCode = response.StatusCode
            };
        }

        var apiEnvelope = DeserializeMunicipalServiceParametersApiEnvelope(response.Content!);
        var messages = BuildMessages(apiEnvelope.Errors);

        if (apiEnvelope.Error is not null)
        {
            messages = [..messages, CreateMessage(apiEnvelope.Error)];
        }

        if (!response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(apiEnvelope.Message))
        {
            messages = [..messages, new NFSeMessage { Description = apiEnvelope.Message! }];
        }

        var taxRates = apiEnvelope.TaxRates is null
            ? new Dictionary<string, IReadOnlyList<MunicipalServiceTaxRate>>()
            : apiEnvelope.TaxRates.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<MunicipalServiceTaxRate>)pair.Value.Select(rate => new MunicipalServiceTaxRate
                {
                    Incidence = rate.Incidence,
                    Rate = rate.Rate,
                    ValidFrom = rate.ValidFrom,
                    ValidTo = rate.ValidTo
                }).ToArray(),
                StringComparer.Ordinal);

        if (response.IsSuccessStatusCode && taxRates.Count == 0 && messages.Count == 0)
        {
            messages = [..messages, new NFSeMessage { Description = "Municipal service parameters lookup returned no aliquotas data." }];
        }

        return new GetMunicipalServiceParametersResult
        {
            MunicipalityCode = request.MunicipalityCode,
            ServiceCode = request.ServiceCode,
            CompetenceDate = request.CompetenceDate,
            IsAvailable = response.IsSuccessStatusCode && taxRates.Count > 0 && messages.Count == 0,
            TaxRates = taxRates,
            JsonContent = response.Content,
            RawJson = response.Content,
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

        if (_disposeSigningCertificate)
        {
            _signingCertificate?.Dispose();
        }

        if (_disposeClientCertificate && !ReferenceEquals(_ownedClientCertificate, _signingCertificate))
        {
            _ownedClientCertificate?.Dispose();
        }
    }

    private static DefaultClientDependencies CreateDefaultDependencies(
        NFSeSdkOptions? options,
        X509Certificate2? clientCertificate,
        HttpClient? httpClient)
    {
        var resolvedOptions = options ?? new NFSeSdkOptions();
        var shouldDisposeClientCertificate = clientCertificate is null &&
            resolvedOptions.ClientCertificate is null &&
            !string.IsNullOrWhiteSpace(resolvedOptions.CertificateFile?.Path);
        var resolvedClientCertificate = clientCertificate ?? NFSeCertificateLoader.Load(resolvedOptions);
        var shouldDisposeSigningCertificate = resolvedOptions.SigningCertificate is null &&
            !string.IsNullOrWhiteSpace(resolvedOptions.SigningCertificateFile?.Path);
        var resolvedSigningCertificate = resolvedOptions.SigningCertificate ??
            (resolvedOptions.SigningCertificateFile is null
                ? null
                : NFSeCertificateLoader.LoadFromPfxFile(resolvedOptions.SigningCertificateFile)) ??
            resolvedClientCertificate;
        var endpoints = NFSeEndpointsOptions.For(resolvedOptions.Environment);
        var transport = new NFSeHttpTransport(
            endpoints,
            new NFSeHttpTransportOptions
            {
                Timeout = resolvedOptions.Timeout,
                UserAgent = resolvedOptions.UserAgent,
                ClientCertificate = resolvedClientCertificate
            },
            httpClient);

        return new DefaultClientDependencies(
            transport,
            new NFSeXmlSerializer(),
            endpoints,
            resolvedSigningCertificate,
            resolvedOptions.LayoutProfile,
            BuildApplicationVersion(resolvedOptions),
            resolvedOptions.ValidateResponseXml,
            shouldDisposeSigningCertificate,
            resolvedClientCertificate,
            shouldDisposeClientCertificate);
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

        var metadataSeparatorIndex = version?.IndexOf('+') ?? -1;
        if (metadataSeparatorIndex >= 0)
        {
            version = version?.Substring(0, metadataSeparatorIndex);
        }

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
            : value.Substring(0, maxLength);
    }

    private NFSeClient(
        DefaultClientDependencies dependencies,
        JsonSerializerOptions? jsonSerializerOptions)
        : this(
            dependencies.Transport,
            dependencies.Serializer,
            dependencies.Endpoints,
            dependencies.SigningCertificate,
            jsonSerializerOptions,
            dependencies.LayoutProfile,
            dependencies.ApplicationVersion,
            dependencies.ValidateResponseXml,
            disposeTransport: true,
            disposeSigningCertificate: dependencies.DisposeSigningCertificate,
            ownedClientCertificate: dependencies.ClientCertificate,
            disposeClientCertificate: dependencies.DisposeClientCertificate)
    {
    }

    private NFSeClient(
        INFSeTransport transport,
        INFSeSerializer serializer,
        NFSeEndpointsOptions endpoints,
        X509Certificate2? signingCertificate,
        JsonSerializerOptions? jsonSerializerOptions,
        NFSeLayoutProfile layoutProfile,
        string applicationVersion,
        bool validateResponseXml,
        bool disposeTransport,
        bool disposeSigningCertificate,
        X509Certificate2? ownedClientCertificate,
        bool disposeClientCertificate)
    {
        if (transport is null) { throw new ArgumentNullException(nameof(transport)); }
        if (serializer is null) { throw new ArgumentNullException(nameof(serializer)); }
        if (endpoints is null) { throw new ArgumentNullException(nameof(endpoints)); }

        _transport = transport;
        _serializer = serializer;
        _endpoints = endpoints;
        _jsonSerializerOptions = CreateDefaultJsonSerializerOptions(jsonSerializerOptions);
        _signingCertificate = signingCertificate;
        _layoutProfile = layoutProfile;
        _applicationVersion = applicationVersion;
        _validateResponseXml = validateResponseXml;
        _ownedClientCertificate = ownedClientCertificate;
        _disposeTransport = disposeTransport;
        _disposeSigningCertificate = disposeSigningCertificate;
        _disposeClientCertificate = disposeClientCertificate;
    }

    private Contracts.Serialization.NFSeLookupDeserializationResult DeserializeLookupXml(
        string rawXml,
        HttpStatusCode statusCode)
    {
        try
        {
            if (_validateResponseXml && GetRootElementLocalName(rawXml) == "NFSe")
            {
                new NFSeReceivedXmlValidator().ValidateNfse(rawXml, _layoutProfile);
            }

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
            : SefinNationalCompressedDocumentDecoder.DecodeGZipBase64(envelope.NfseXmlGZipBase64!);
    }

    private static string? TryDecodeXml(SefinNationalTransmissionApiEnvelope envelope)
    {
        return string.IsNullOrWhiteSpace(envelope.NfseXmlGZipBase64)
            ? null
            : SefinNationalCompressedDocumentDecoder.DecodeGZipBase64(envelope.NfseXmlGZipBase64!);
    }

    private static string? TryDecodeEventXml(SefinNationalEventApiEnvelope envelope)
    {
        var compressedXml = envelope.EventXmlGZipBase64;

        if (string.IsNullOrWhiteSpace(compressedXml) &&
            envelope.AdditionalData is not null &&
            envelope.AdditionalData.TryGetValue("xmlGZipB64", out var xmlGZipBase64) &&
            xmlGZipBase64.ValueKind == JsonValueKind.String)
        {
            compressedXml = xmlGZipBase64.GetString();
        }

        return string.IsNullOrWhiteSpace(compressedXml)
            ? null
            : SefinNationalCompressedDocumentDecoder.DecodeGZipBase64(compressedXml!);
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

    private static IReadOnlyList<NFSeMessage> BuildMessages(JsonElement? element)
    {
        if (element is null ||
            element.Value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return Array.Empty<NFSeMessage>();
        }

        return element.Value.ValueKind switch
        {
            JsonValueKind.Array => [..element.Value.EnumerateArray().SelectMany(BuildMessagesFromElement)],
            _ => BuildMessagesFromElement(element.Value)
        };
    }

    private static IReadOnlyList<NFSeMessage> BuildMessagesFromElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null ||
            element.ValueKind == JsonValueKind.Undefined)
        {
            return Array.Empty<NFSeMessage>();
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var stringDescription = NormalizeOptionalText(element.GetString());
            return stringDescription is null
                ? Array.Empty<NFSeMessage>()
                : [new NFSeMessage { Description = stringDescription }];
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return Array.Empty<NFSeMessage>();
        }

        var code = GetJsonString(element, "codigo") ?? GetJsonString(element, "Codigo");
        var description = GetJsonString(element, "descricao") ??
            GetJsonString(element, "Descricao") ??
            GetJsonString(element, "mensagem") ??
            GetJsonString(element, "Mensagem") ??
            GetJsonString(element, "complemento") ??
            GetJsonString(element, "Complemento");

        return code is null && description is null
            ? Array.Empty<NFSeMessage>()
            : [new NFSeMessage
            {
                Code = code,
                Description = description ?? "The SEFIN API returned a message without description."
            }];
    }

    private static string? GetJsonString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return NormalizeOptionalText(property.GetString());
    }

    private string BuildDpsByIdPath(string dpsId)
    {
        return _endpoints.DpsByIdPath.Replace(
            "{id}",
            Uri.EscapeDataString(dpsId));
    }

    private string BuildNfseEventsPath(string accessKey)
    {
        return _endpoints.NfseEventsPath.Replace(
            "{chaveAcesso}",
            Uri.EscapeDataString(accessKey));
    }

    private static string BuildApplicationVersion(NFSeSdkOptions options)
    {
        var name = NormalizeOptionalText(options.ApplicationName);
        var version = NormalizeOptionalText(options.ApplicationVersion);
        if (name is null && version is null)
        {
            return BuildApplicationVersion();
        }

        return LimitApplicationVersion(string.Join("_", new[] { name, version }.Where(value => value is not null)));
    }

    private string BuildNfseEventsLookupPath(GetNfseEventsRequest request)
    {
        var path = request.SequenceNumber is not null
            ? _endpoints.NfseEventByTypeAndSequencePath
            : string.IsNullOrWhiteSpace(request.EventTypeCode)
                ? _endpoints.NfseEventsPath
                : _endpoints.NfseEventByTypePath;

        path = path.Replace("{chaveAcesso}", Uri.EscapeDataString(request.AccessKey));
        if (!string.IsNullOrWhiteSpace(request.EventTypeCode))
        {
            path = path.Replace("{tipoEvento}", Uri.EscapeDataString(request.EventTypeCode!));
        }

        if (request.SequenceNumber is not null)
        {
            path = path.Replace(
                "{numSeqEvento}",
                request.SequenceNumber.Value.ToString(CultureInfo.InvariantCulture));
        }

        return path;
    }

    private static IEnumerable<string> FindCompressedEventDocuments(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if ((property.Name.Equals("eventoXmlGZipB64", StringComparison.OrdinalIgnoreCase) ||
                     property.Name.Equals("xmlGZipB64", StringComparison.OrdinalIgnoreCase)) &&
                    property.Value.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(property.Value.GetString()))
                {
                    yield return property.Value.GetString()!;
                }

                foreach (var nested in FindCompressedEventDocuments(property.Value))
                {
                    yield return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var nested in FindCompressedEventDocuments(item))
                {
                    yield return nested;
                }
            }
        }
    }

    private static IEnumerable<JsonElement> FindMessageElements(JsonElement element, params string[] names)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (names.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in property.Value.EnumerateArray())
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        yield return property.Value;
                    }

                    continue;
                }

                foreach (var nested in FindMessageElements(property.Value, names))
                {
                    yield return nested;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var nested in FindMessageElements(item, names))
                {
                    yield return nested;
                }
            }
        }
    }

    private static string ExtractAccessKeyFromEventRequestId(string eventRequestId)
    {
        const int prefixLength = 3;
        const int accessKeyLength = 50;

        return eventRequestId.Length >= prefixLength + accessKeyLength
            ? eventRequestId.Substring(prefixLength, accessKeyLength)
            : eventRequestId;
    }

    private string BuildMunicipalConventionPath(string municipalityCode)
    {
        var path = _endpoints.MunicipalParametersByConventionPath.Replace(
            "{codigoMunicipio}",
            Uri.EscapeDataString(municipalityCode));

        return new Uri(
            new Uri(_endpoints.ParametrizationBaseUrl, UriKind.Absolute),
            path.TrimStart('/')).ToString();
    }

    private string BuildMunicipalServiceParametersPath(
        string municipalityCode,
        string serviceCode,
        DateOnly competenceDate)
    {
        var path = _endpoints.MunicipalParametersByServiceCodePath
            .Replace(
                "{codigoMunicipio}",
                Uri.EscapeDataString(municipalityCode))
            .Replace(
                "{codigoServico}",
                Uri.EscapeDataString(serviceCode))
            .Replace(
                "{competencia}",
                Uri.EscapeDataString(competenceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

        return new Uri(
            new Uri(_endpoints.ParametrizationBaseUrl, UriKind.Absolute),
            path.TrimStart('/')).ToString();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? GetRootElementLocalName(string xmlContent)
    {
        try
        {
            using var stringReader = new StringReader(xmlContent);
            using var reader = XmlReader.Create(stringReader, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    return reader.LocalName;
                }
            }

            return null;
        }
        catch (XmlException exception)
        {
            throw new NFSeSerializationException("The NFSe API returned malformed or unsafe XML.", exception);
        }
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

    private SefinNationalMunicipalServiceParametersApiEnvelope DeserializeMunicipalServiceParametersApiEnvelope(
        string content)
    {
        return DeserializeJson<SefinNationalMunicipalServiceParametersApiEnvelope>(
            content,
            "The NFSe API returned an empty JSON object for the municipal service parameters lookup.",
            "Failed to deserialize the JSON payload returned by the NFSe API for the municipal service parameters lookup.");
    }

    private SefinNationalTransmissionApiEnvelope DeserializeTransmissionApiEnvelope(string content)
    {
        return DeserializeJson<SefinNationalTransmissionApiEnvelope>(
            content,
            "The SEFIN API returned an empty JSON object for the DPS emission.",
            "Failed to deserialize the JSON payload returned by the SEFIN API for the DPS emission.");
    }

    private SefinNationalEventApiEnvelope DeserializeEventApiEnvelope(string content)
    {
        return DeserializeJson<SefinNationalEventApiEnvelope>(
            content,
            "The SEFIN API returned an empty JSON object for the NFS-e event registration.",
            "Failed to deserialize the JSON payload returned by the SEFIN API for the NFS-e event registration.");
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

    private sealed class DefaultClientDependencies
    {
        public DefaultClientDependencies(
            INFSeTransport transport,
            INFSeSerializer serializer,
            NFSeEndpointsOptions endpoints,
            X509Certificate2? signingCertificate,
            NFSeLayoutProfile layoutProfile,
            string applicationVersion,
            bool validateResponseXml,
            bool disposeSigningCertificate,
            X509Certificate2? clientCertificate,
            bool disposeClientCertificate)
        {
            Transport = transport;
            Serializer = serializer;
            Endpoints = endpoints;
            SigningCertificate = signingCertificate;
            LayoutProfile = layoutProfile;
            ApplicationVersion = applicationVersion;
            ValidateResponseXml = validateResponseXml;
            DisposeSigningCertificate = disposeSigningCertificate;
            ClientCertificate = clientCertificate;
            DisposeClientCertificate = disposeClientCertificate;
        }

        public INFSeTransport Transport { get; }

        public INFSeSerializer Serializer { get; }

        public NFSeEndpointsOptions Endpoints { get; }

        public X509Certificate2? SigningCertificate { get; }

        public NFSeLayoutProfile LayoutProfile { get; }

        public string ApplicationVersion { get; }

        public bool ValidateResponseXml { get; }

        public bool DisposeSigningCertificate { get; }

        public X509Certificate2? ClientCertificate { get; }

        public bool DisposeClientCertificate { get; }
    }
}
