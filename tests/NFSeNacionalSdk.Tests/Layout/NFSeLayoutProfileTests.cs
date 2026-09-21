using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

namespace NFSeNacionalSdk.Tests.Layout;

public sealed class NFSeLayoutProfileTests
{
    [Theory]
    [InlineData(NFSeEnvironment.Production)]
    [InlineData(NFSeEnvironment.ProductionRestricted)]
    public void Options_ShouldUseSingleExplicitCurrentProfileForEveryEnvironment(NFSeEnvironment environment)
    {
        var options = new NFSeSdkOptions { Environment = environment };

        Assert.Equal(NFSeLayoutProfile.RtcV101_202607, NFSeLayoutDefaults.Current);
        Assert.Equal(NFSeLayoutDefaults.Current, options.LayoutProfile);
    }
}
