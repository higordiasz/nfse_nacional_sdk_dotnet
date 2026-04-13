using System.Net;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Tests.TestData;

namespace NFSeNacionalSdk.Tests.Client;

public sealed class NFSeClientTests
{
    [Fact]
    public async Task GetNfseByAccessKeyAsync_ShouldReturnNormalizedSuccessResult()
    {
        using var client = new NFSeClient(
            new FakeTransport(HttpStatusCode.OK, NFSeLookupXmlFixtures.Success),
            CreateSerializer(),
            NFSeEndpointsOptions.For(Core.Enums.NFSeEnvironment.ProductionRestricted));

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
            new FakeTransport(HttpStatusCode.BadRequest, NFSeLookupXmlFixtures.BusinessError),
            CreateSerializer(),
            NFSeEndpointsOptions.For(Core.Enums.NFSeEnvironment.ProductionRestricted));

        var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
        {
            AccessKey = NFSeLookupXmlFixtures.AccessKey
        });

        Assert.False(result.Success);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.AccessKey);
        Assert.Equal(NFSeLookupXmlFixtures.BusinessError, result.RawXml);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Null(result.Document);
        Assert.Null(result.JsonContent);
        Assert.Collection(
            result.Messages,
            message =>
            {
                Assert.Equal("E160", message.Code);
                Assert.Equal("NFS-e nao encontrada para a chave de acesso informada.", message.Description);
            },
            message =>
            {
                Assert.Equal("E161", message.Code);
                Assert.Equal("Verifique se a chave pertence ao ambiente consultado.", message.Description);
            });
    }

    private static INFSeSerializer CreateSerializer() => new NFSeXmlSerializer();

    private sealed class FakeTransport(HttpStatusCode statusCode, string xmlContent) : INFSeTransport
    {
        public Task<TransportResponse> SendAsync(
            TransportRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = new TransportResponse
            {
                StatusCode = statusCode,
                Content = xmlContent,
                ContentType = "application/xml"
            };

            return Task.FromResult(response);
        }
    }
}
