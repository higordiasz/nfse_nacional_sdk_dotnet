using System.Net;
using System.Text.Json;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Tests.TestData;

namespace NFSeNacionalSdk.Tests.Client;

public sealed class NFSeClientTests
{
    [Fact]
    public async Task EmitDpsAsync_ShouldSendSignedCompressedXmlAndReturnNormalizedSuccessResult()
    {
        var transport = new CapturingTransport(HttpStatusCode.Created, NFSeTransmissionFixtures.SuccessApiResponseJson);
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted),
            certificate);

        var result = await client.EmitDpsAsync(NFSeTransmissionFixtures.CreateRequest());

        Assert.True(result.Success);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.AccessKey);
        Assert.Equal(NFSeLookupXmlFixtures.Success, result.RawXml);
        Assert.NotNull(result.Document);
        Assert.NotNull(result.JsonContent);
        Assert.Equal(result.SubmittedDpsXml, DecodePostedDpsXml(transport.LastRequest));
        Assert.Collection(
            result.Messages,
            message =>
            {
                Assert.Equal("A100", message.Code);
                Assert.Equal("Emitida com alerta de homologacao.", message.Description);
            });

        Assert.NotNull(transport.LastRequest);
        Assert.Equal(HttpMethod.Post, transport.LastRequest!.Method);
        Assert.Equal("/nfse", transport.LastRequest.Path);
        Assert.Equal("application/json", transport.LastRequest.ContentType);
        Assert.Equal("application/json", transport.LastRequest.Accept);
        Assert.Single(JsonDocument.Parse(transport.LastRequest.Content!).RootElement.EnumerateObject());
        Assert.Contains("<Signature", result.SubmittedDpsXml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EmitDpsAsync_ShouldReturnNormalizedBusinessErrorResult()
    {
        var transport = new CapturingTransport(HttpStatusCode.BadRequest, NFSeTransmissionFixtures.ErrorApiResponseJson);
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted),
            certificate);

        var result = await client.EmitDpsAsync(NFSeTransmissionFixtures.CreateRequest());

        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);
        Assert.Null(result.AccessKey);
        Assert.Null(result.RawXml);
        Assert.Null(result.Document);
        Assert.Null(result.JsonContent);
        Assert.False(string.IsNullOrWhiteSpace(result.SubmittedDpsXml));
        Assert.Collection(
            result.Messages,
            message =>
            {
                Assert.Equal("E3001", message.Code);
                Assert.Equal("DPS invalido.", message.Description);
            },
            message =>
            {
                Assert.Null(message.Code);
                Assert.Equal("Revise os dados do tomador.", message.Description);
            });
    }

    [Fact]
    public async Task GetNfseByAccessKeyAsync_ShouldReturnNormalizedSuccessResult()
    {
        using var client = new NFSeClient(
            new CapturingTransport(HttpStatusCode.OK, NFSeLookupXmlFixtures.SuccessApiResponseJson),
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
        {
            AccessKey = NFSeLookupXmlFixtures.AccessKey
        });

        Assert.True(result.Success);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.AccessKey);
        Assert.Equal(NFSeLookupXmlFixtures.Success, result.RawXml);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Document);
        Assert.Empty(result.Messages);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.Document!.AccessKey);
        Assert.Equal("2024000000001", result.Document.Number);
        Assert.NotNull(result.JsonContent);
        Assert.Contains(NFSeLookupXmlFixtures.AccessKey, result.JsonContent!, StringComparison.Ordinal);
        Assert.Contains("2024000000001", result.JsonContent!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetNfseByAccessKeyAsync_ShouldReturnNormalizedBusinessErrorResult()
    {
        using var client = new NFSeClient(
            new CapturingTransport(HttpStatusCode.NotFound, NFSeLookupXmlFixtures.NotFoundApiResponseJson),
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
        {
            AccessKey = NFSeLookupXmlFixtures.AccessKey
        });

        Assert.False(result.Success);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.AccessKey);
        Assert.Null(result.RawXml);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Document);
        Assert.Null(result.JsonContent);
        Assert.Collection(result.Messages, message =>
        {
            Assert.Equal("E2401", message.Code);
            Assert.Equal("Chave de acesso não encontrada.", message.Description);
        });
    }

    [Fact]
    public async Task GetDpsByIdAsync_ShouldReturnAccessKeyForGeneratedDps()
    {
        var transport = new CapturingTransport(HttpStatusCode.OK, NFSeTransmissionFixtures.DpsLookupSuccessApiResponseJson);
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetDpsByIdAsync(new GetDpsByIdRequest
        {
            DpsId = NFSeTransmissionFixtures.ExpectedDpsId
        });

        Assert.True(result.Success);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.AccessKey);
        Assert.Empty(result.Messages);

        Assert.NotNull(transport.LastRequest);
        Assert.Equal(HttpMethod.Get, transport.LastRequest!.Method);
        Assert.Equal($"/dps/{NFSeTransmissionFixtures.ExpectedDpsId}", transport.LastRequest.Path);
        Assert.Equal("application/json", transport.LastRequest.Accept);
    }

    [Fact]
    public async Task EmitDpsAsync_ShouldReturnNormalizedStandardErrorResult()
    {
        var transport = new CapturingTransport(
            HttpStatusCode.Forbidden,
            """
            {
              "tipoAmbiente": 2,
              "versaoAplicativo": "SefinNacional",
              "dataHoraProcessamento": "2026-04-29T08:50:10.0836516-03:00",
              "erro": {
                "codigo": "E1600",
                "descricao": "Certificado digital da transmissao invalido."
              }
            }
            """);
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted),
            certificate);

        var result = await client.EmitDpsAsync(NFSeTransmissionFixtures.CreateRequest());

        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        Assert.Collection(result.Messages, message =>
        {
            Assert.Equal("E1600", message.Code);
            Assert.Equal("Certificado digital da transmissao invalido.", message.Description);
        });
    }

    [Fact]
    public async Task GetDpsByIdAsync_ShouldReturnBusinessErrorWhenDpsIsNotFound()
    {
        using var client = new NFSeClient(
            new CapturingTransport(HttpStatusCode.NotFound, NFSeTransmissionFixtures.DpsLookupErrorApiResponseJson),
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetDpsByIdAsync(new GetDpsByIdRequest
        {
            DpsId = NFSeTransmissionFixtures.ExpectedDpsId
        });

        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);
        Assert.Null(result.AccessKey);
        Assert.Collection(result.Messages, message =>
        {
            Assert.Equal("E2501", message.Code);
            Assert.Equal("DPS nao encontrada.", message.Description);
        });
    }

    [Fact]
    public async Task CheckDpsByIdAsync_ShouldSendHeadAndReturnGeneratedStatus()
    {
        var transport = new CapturingTransport(HttpStatusCode.OK, string.Empty);
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.CheckDpsByIdAsync(new GetDpsByIdRequest
        {
            DpsId = NFSeTransmissionFixtures.ExpectedDpsId
        });

        Assert.True(result.Generated);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);

        Assert.NotNull(transport.LastRequest);
        Assert.Equal(HttpMethod.Head, transport.LastRequest!.Method);
        Assert.Equal($"/dps/{NFSeTransmissionFixtures.ExpectedDpsId}", transport.LastRequest.Path);
    }

    [Fact]
    public async Task CheckDpsByIdAsync_ShouldReturnNotGeneratedForNotFoundStatus()
    {
        using var client = new NFSeClient(
            new CapturingTransport(HttpStatusCode.NotFound, string.Empty),
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.CheckDpsByIdAsync(new GetDpsByIdRequest
        {
            DpsId = NFSeTransmissionFixtures.ExpectedDpsId
        });

        Assert.False(result.Generated);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);
    }

    [Fact]
    public async Task GetMunicipalConventionAsync_ShouldCallParametrizationApiAndReturnAvailableResult()
    {
        const string municipalityCode = "3204005";
        var transport = new CapturingTransport(
            HttpStatusCode.OK,
            """
            {
              "parametrosConvenio": {
                "aderenteAmbienteNacional": 1,
                "aderenteEmissorNacional": 0
              },
              "mensagem": "Parametros do convenio recuperados com sucesso."
            }
            """);
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetMunicipalConventionAsync(new GetMunicipalConventionRequest
        {
            MunicipalityCode = municipalityCode
        });

        Assert.True(result.IsAvailable);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(municipalityCode, result.MunicipalityCode);
        Assert.NotNull(result.JsonContent);
        Assert.Empty(result.Messages);

        Assert.NotNull(transport.LastRequest);
        Assert.Equal(HttpMethod.Get, transport.LastRequest!.Method);
        Assert.Equal(
            "https://adn.producaorestrita.nfse.gov.br/parametrizacao/3204005/convenio",
            transport.LastRequest.Path);
        Assert.Equal("application/json", transport.LastRequest.Accept);
    }

    [Fact]
    public async Task GetMunicipalConventionAsync_ShouldReturnUnavailableBusinessErrorResult()
    {
        const string municipalityCode = "3204005";
        using var client = new NFSeClient(
            new CapturingTransport(
                HttpStatusCode.NotFound,
                """
                {
                  "mensagem": "Parametros do convenio nao encontrados."
                }
                """),
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetMunicipalConventionAsync(new GetMunicipalConventionRequest
        {
            MunicipalityCode = municipalityCode
        });

        Assert.False(result.IsAvailable);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(municipalityCode, result.MunicipalityCode);
        Assert.NotNull(result.JsonContent);
        Assert.Collection(result.Messages, message =>
        {
            Assert.Null(message.Code);
            Assert.Equal("Parametros do convenio nao encontrados.", message.Description);
        });
    }

    [Fact]
    public async Task GetMunicipalServiceParametersAsync_ShouldCallParametrizationApiAndReturnAvailableResult()
    {
        const string municipalityCode = "3204005";
        const string serviceCode = "01.01.01.000";
        var competenceDate = new DateOnly(2026, 4, 29);
        var transport = new CapturingTransport(
            HttpStatusCode.OK,
            """
            {
              "aliquotas": {
                "01.01.01.000": [
                  {
                    "Incidencia": "SIM",
                    "Aliq": 3.00,
                    "DtIni": "2025-11-20T00:00:00",
                    "DtFim": null
                  }
                ]
              },
              "mensagem": "Aliquotas recuperadas com sucesso."
            }
            """);
        using var client = new NFSeClient(
            transport,
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetMunicipalServiceParametersAsync(new GetMunicipalServiceParametersRequest
        {
            MunicipalityCode = municipalityCode,
            ServiceCode = "010101",
            CompetenceDate = competenceDate
        });

        Assert.True(result.IsAvailable);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(municipalityCode, result.MunicipalityCode);
        Assert.Equal(serviceCode, result.ServiceCode);
        Assert.Equal(competenceDate, result.CompetenceDate);
        Assert.NotNull(result.JsonContent);
        Assert.Empty(result.Messages);

        Assert.NotNull(transport.LastRequest);
        Assert.Equal(HttpMethod.Get, transport.LastRequest!.Method);
        Assert.Equal(
            "https://adn.producaorestrita.nfse.gov.br/parametrizacao/3204005/01.01.01.000/2026-04-29/aliquota",
            transport.LastRequest.Path);
        Assert.Equal("application/json", transport.LastRequest.Accept);
    }

    [Fact]
    public async Task GetMunicipalServiceParametersAsync_ShouldReturnUnavailableBusinessErrorResult()
    {
        const string municipalityCode = "3204005";
        const string serviceCode = "01.01.01.000";
        var competenceDate = new DateOnly(2026, 4, 29);
        using var client = new NFSeClient(
            new CapturingTransport(
                HttpStatusCode.NotFound,
                """
                {
                  "aliquotas": null,
                  "mensagem": "Aliquotas nao encontradas."
                }
                """),
            CreateSerializer(),
            NFSeEndpointsOptions.For(NFSeEnvironment.ProductionRestricted));

        var result = await client.GetMunicipalServiceParametersAsync(new GetMunicipalServiceParametersRequest
        {
            MunicipalityCode = municipalityCode,
            ServiceCode = serviceCode,
            CompetenceDate = competenceDate
        });

        Assert.False(result.IsAvailable);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(municipalityCode, result.MunicipalityCode);
        Assert.Equal(serviceCode, result.ServiceCode);
        Assert.Equal(competenceDate, result.CompetenceDate);
        Assert.NotNull(result.JsonContent);
        Assert.Collection(result.Messages, message =>
        {
            Assert.Null(message.Code);
            Assert.Equal("Aliquotas nao encontradas.", message.Description);
        });
    }

    private static INFSeSerializer CreateSerializer() => new NFSeXmlSerializer();

    private static string DecodePostedDpsXml(TransportRequest? request)
    {
        Assert.NotNull(request);
        Assert.False(string.IsNullOrWhiteSpace(request!.Content));

        using var document = JsonDocument.Parse(request.Content!);
        var compressedXml = document.RootElement.GetProperty("dpsXmlGZipB64").GetString();

        Assert.False(string.IsNullOrWhiteSpace(compressedXml));
        return NFSeTransmissionFixtures.DecodeGZipBase64(compressedXml!);
    }

    private sealed class CapturingTransport(HttpStatusCode statusCode, string content) : INFSeTransport
    {
        public TransportRequest? LastRequest { get; private set; }

        public Task<TransportResponse> SendAsync(
            TransportRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;

            var response = new TransportResponse
            {
                StatusCode = statusCode,
                Content = content,
                ContentType = "application/json"
            };

            return Task.FromResult(response);
        }
    }
}
