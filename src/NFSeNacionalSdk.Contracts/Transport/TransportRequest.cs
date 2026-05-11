using System.Net.Http;

namespace NFSeNacionalSdk.Contracts.Transport;

public sealed class TransportRequest
{
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    public string Path { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? ContentType { get; set; }

    public string? Accept { get; set; }

    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
