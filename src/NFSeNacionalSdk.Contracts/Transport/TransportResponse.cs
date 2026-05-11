using System.Net;

namespace NFSeNacionalSdk.Contracts.Transport;

public sealed class TransportResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Content { get; set; }

    public string? ContentType { get; set; }

    public IDictionary<string, IEnumerable<string>> Headers { get; set; } =
        new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;
}
