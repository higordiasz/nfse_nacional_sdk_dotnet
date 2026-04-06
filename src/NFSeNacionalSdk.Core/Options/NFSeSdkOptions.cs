using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Core.Options;

public sealed class NFSeSdkOptions
{
    public NFSeEnvironment Environment { get; set; } = NFSeEnvironment.ProductionRestricted;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    public string UserAgent { get; set; } = "NFSeNacionalSdk";
}