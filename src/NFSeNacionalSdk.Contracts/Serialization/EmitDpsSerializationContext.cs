using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Contracts.Serialization;

public sealed class EmitDpsSerializationContext
{
    public NFSeEnvironment Environment { get; init; }

    public required X509Certificate2 SigningCertificate { get; init; }

    public string? ApplicationVersion { get; init; }
}
