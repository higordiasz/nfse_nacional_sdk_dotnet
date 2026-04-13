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
