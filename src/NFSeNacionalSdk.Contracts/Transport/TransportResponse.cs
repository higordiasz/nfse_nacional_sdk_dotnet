using System.Net;

namespace NFSeNacionalSdk.Contracts.Transport;

public sealed class TransportResponse
{
    public HttpStatusCode StatusCode { get; init; }

    public string? Content { get; init; }

    public string? ContentType { get; init; }

    public IDictionary<string, IEnumerable<string>> Headers { get; init; } =
        new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;
}
