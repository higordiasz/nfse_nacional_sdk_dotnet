namespace NFSeNacionalSdk.Contracts.Transport;

public sealed class TransportResponse
{
    public int StatusCode { get; init; }

    public string? Content { get; init; }

    public IDictionary<string, IEnumerable<string>> Headers { get; init; } =
        new Dictionary<string, IEnumerable<string>>();

    public bool IsSuccessStatusCode => StatusCode >= 200 && StatusCode <= 299;
}