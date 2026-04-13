using System.Net.Http;

namespace NFSeNacionalSdk.Contracts.Transport;

public sealed class TransportRequest
{
    public HttpMethod Method { get; init; } = HttpMethod.Get;

    public string Path { get; init; } = string.Empty;

    public string? Content { get; init; }

    public string? ContentType { get; init; }

    public string? Accept { get; init; }

    public IDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
