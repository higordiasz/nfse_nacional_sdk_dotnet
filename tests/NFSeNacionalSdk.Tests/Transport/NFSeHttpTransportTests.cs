using System.Net;
using System.Net.Http.Headers;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Core.Options;
using NFSeNacionalSdk.Tests.TestData;
using NFSeNacionalSdk.Transport.Http;

namespace NFSeNacionalSdk.Tests.Transport;

public sealed class NFSeHttpTransportTests
{
    [Fact]
    public async Task SendAsync_ShouldComposeRequestAgainstConfiguredBaseUrl()
    {
        HttpRequestMessage? capturedRequest = null;

        using var httpClient = new HttpClient(new CaptureHandler(message =>
        {
            capturedRequest = message;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(NFSeLookupXmlFixtures.SuccessApiResponseJson)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
        }));

        using var transport = new NFSeHttpTransport(
            NFSeEndpointsOptions.For(Core.Enums.NFSeEnvironment.ProductionRestricted),
            new NFSeHttpTransportOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                UserAgent = "NFSeNacionalSdk.Tests"
            },
            httpClient);

        var response = await transport.SendAsync(new TransportRequest
        {
            Method = HttpMethod.Get,
            Path = "/nfse/teste",
            Accept = "application/json"
        });

        Assert.NotNull(capturedRequest);
        Assert.Equal("https://sefin.producaorestrita.nfse.gov.br/SefinNacional/nfse/teste", capturedRequest!.RequestUri?.ToString());
        Assert.Contains(capturedRequest.Headers.Accept, item => item.MediaType == "application/json");
        Assert.Contains(capturedRequest.Headers.UserAgent, item => item.ToString() == "NFSeNacionalSdk.Tests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.ContentType);
    }

    [Fact]
    public async Task SendAsync_ShouldMapInternalCancellationToTimeoutException()
    {
        using var httpClient = new HttpClient(new AsyncHandler((_, _) =>
            Task.FromException<HttpResponseMessage>(new TaskCanceledException())));
        using var transport = new NFSeHttpTransport(
            NFSeEndpointsOptions.For(Core.Enums.NFSeEnvironment.ProductionRestricted),
            new NFSeHttpTransportOptions(),
            httpClient);

        var exception = await Assert.ThrowsAsync<NFSeTransportException>(() => transport.SendAsync(
            new TransportRequest { Method = HttpMethod.Get, Path = "/nfse/teste" }));

        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendAsync_ShouldPreserveCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var httpClient = new HttpClient(new AsyncHandler((_, token) =>
            Task.FromCanceled<HttpResponseMessage>(token)));
        using var transport = new NFSeHttpTransport(
            NFSeEndpointsOptions.For(Core.Enums.NFSeEnvironment.ProductionRestricted),
            new NFSeHttpTransportOptions(),
            httpClient);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => transport.SendAsync(
            new TransportRequest { Method = HttpMethod.Get, Path = "/nfse/teste" },
            cancellation.Token));
    }

    private sealed class CaptureHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class AsyncHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => responseFactory(request, cancellationToken);
    }
}
