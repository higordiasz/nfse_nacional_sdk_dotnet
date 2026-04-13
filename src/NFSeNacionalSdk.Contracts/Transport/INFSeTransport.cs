namespace NFSeNacionalSdk.Contracts.Transport;

public interface INFSeTransport
{
    Task<TransportResponse> SendAsync(
        TransportRequest request,
        CancellationToken cancellationToken = default);
}
