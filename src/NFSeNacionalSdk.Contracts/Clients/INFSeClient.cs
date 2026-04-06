using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Responses;

namespace NFSeNacionalSdk.Contracts.Clients;

public interface INFSeClient
{
    Task<EmitDpsResponse> EmitAsync(
        EmitDpsRequest request,
        CancellationToken cancellationToken = default);
}