using System.Net;

namespace NFSeNacionalSdk.Contracts.Responses;

public sealed class CheckDpsByIdResult
{
    public required string DpsId { get; init; }

    public bool Generated { get; init; }

    public HttpStatusCode StatusCode { get; init; }
}
