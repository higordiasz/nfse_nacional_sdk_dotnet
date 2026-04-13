using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;

namespace NFSeNacionalSdk.Contracts.Clients;

public interface INFSeClient
{
    Task<EmitDpsResponse> EmitDpsAsync(
        EmitDpsRequest request,
        CancellationToken cancellationToken = default);

    Task<GetNfseByAccessKeyResult> GetNfseByAccessKeyAsync(
        GetNfseByAccessKeyRequest request,
        CancellationToken cancellationToken = default);
}
