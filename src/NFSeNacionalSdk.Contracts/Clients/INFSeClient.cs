using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;

namespace NFSeNacionalSdk.Contracts.Clients;

public interface INFSeClient
{
    Task<CancelNfseResult> CancelNfseAsync(
        CancelNfseRequest request,
        CancellationToken cancellationToken = default);

    Task<EmitDpsResponse> EmitDpsAsync(
        EmitDpsRequest request,
        CancellationToken cancellationToken = default);

    Task<GetDpsByIdResult> GetDpsByIdAsync(
        GetDpsByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<CheckDpsByIdResult> CheckDpsByIdAsync(
        GetDpsByIdRequest request,
        CancellationToken cancellationToken = default);

    Task<GetMunicipalConventionResult> GetMunicipalConventionAsync(
        GetMunicipalConventionRequest request,
        CancellationToken cancellationToken = default);

    Task<GetMunicipalServiceParametersResult> GetMunicipalServiceParametersAsync(
        GetMunicipalServiceParametersRequest request,
        CancellationToken cancellationToken = default);

    Task<GetNfseByAccessKeyResult> GetNfseByAccessKeyAsync(
        GetNfseByAccessKeyRequest request,
        CancellationToken cancellationToken = default);
}
