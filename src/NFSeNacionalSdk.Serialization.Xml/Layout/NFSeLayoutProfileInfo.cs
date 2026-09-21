using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Serialization.Xml.Layout;

internal sealed class NFSeLayoutProfileInfo
{
    private NFSeLayoutProfileInfo(
        NFSeLayoutProfile profile,
        string resourceSegment,
        bool supportsAlphanumericCnpj,
        bool supportsIbsCbs)
    {
        Profile = profile;
        ResourceSegment = resourceSegment;
        SupportsAlphanumericCnpj = supportsAlphanumericCnpj;
        SupportsIbsCbs = supportsIbsCbs;
    }

    public NFSeLayoutProfile Profile { get; }

    public string XmlVersion => "1.01";

    public string ResourceSegment { get; }

    public bool SupportsAlphanumericCnpj { get; }

    public bool SupportsIbsCbs { get; }

    public static NFSeLayoutProfileInfo Resolve(NFSeLayoutProfile profile)
    {
        return profile switch
        {
            NFSeLayoutProfile.LegacyV101_202602 => new NFSeLayoutProfileInfo(
                profile,
                "LegacyV101_202602",
                supportsAlphanumericCnpj: false,
                supportsIbsCbs: true),
            NFSeLayoutProfile.RtcV101_202607 => new NFSeLayoutProfileInfo(
                profile,
                "RtcV101_202607",
                supportsAlphanumericCnpj: true,
                supportsIbsCbs: true),
            _ => throw new ArgumentOutOfRangeException(nameof(profile), profile, "Unsupported NFS-e layout profile.")
        };
    }
}
