using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NFSeNacionalSdk.Contracts.Transport;
using NFSeNacionalSdk.Core.Constants;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Core.Options;

namespace NFSeNacionalSdk.Transport.Http;

public sealed class NFSeHttpTransport : INFSeTransport, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public NFSeHttpTransport(
        NFSeEndpointsOptions endpoints,
        NFSeHttpTransportOptions? options = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        if (string.IsNullOrWhiteSpace(endpoints.BaseUrl))
        {
            throw new ArgumentException("A valid NFSe base URL must be configured.", nameof(endpoints));
        }

        options ??= new NFSeHttpTransportOptions();

        if (httpClient is not null && options.ClientCertificate is not null)
        {
            throw new ArgumentException(
                "ClientCertificate cannot be used together with an externally provided HttpClient. Configure the certificate on the supplied HttpClient handler instead.",
                nameof(options));
        }

        _ownsHttpClient = httpClient is null;
        _httpClient = httpClient ?? CreateHttpClient(endpoints.BaseUrl, options.ClientCertificate);

        ConfigureHttpClient(_httpClient, endpoints.BaseUrl, options);
    }

    public async Task<TransportResponse> SendAsync(
        TransportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Path))
        {
            throw new ArgumentException("Transport request path must be provided.", nameof(request));
        }

        var requestUri = Uri.IsWellFormedUriString(request.Path, UriKind.Absolute)
            ? request.Path
            : request.Path.TrimStart('/');

        using var message = new HttpRequestMessage(request.Method, requestUri);

        if (!string.IsNullOrWhiteSpace(request.Accept))
        {
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(request.Accept));
        }

        foreach (var header in request.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            message.Content = new StringContent(
                request.Content,
                Encoding.UTF8,
                request.ContentType ?? MediaTypes.ApplicationXml);
        }

        try
        {
            using var response = await _httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseContentRead,
                cancellationToken).ConfigureAwait(false);

            var content = response.Content is null
                ? null
                : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            return new TransportResponse
            {
                StatusCode = response.StatusCode,
                Content = content,
                ContentType = response.Content?.Headers.ContentType?.MediaType,
                Headers = BuildHeaders(response)
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new NFSeTransportException("The HTTP request timed out while calling the NFSe API.");
        }
        catch (HttpRequestException exception)
        {
            throw new NFSeTransportException("The HTTP transport failed while calling the NFSe API.", exception);
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private static HttpClient CreateHttpClient(string baseUrl, X509Certificate2? clientCertificate)
    {
        var handler = new HttpClientHandler();

        if (clientCertificate is not null)
        {
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ClientCertificates.Add(clientCertificate);
        }

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute)
        };
    }

    private static void ConfigureHttpClient(
        HttpClient httpClient,
        string baseUrl,
        NFSeHttpTransportOptions options)
    {
        if (httpClient.BaseAddress is null)
        {
            httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        }

        httpClient.Timeout = options.Timeout;

        if (!httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
        }

        if (!httpClient.DefaultRequestHeaders.Accept.Any())
        {
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue(MediaTypes.ApplicationXml));
        }
    }

    private static IDictionary<string, IEnumerable<string>> BuildHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in response.Headers)
        {
            headers[header.Key] = header.Value;
        }

        if (response.Content is not null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        return headers;
    }
}
