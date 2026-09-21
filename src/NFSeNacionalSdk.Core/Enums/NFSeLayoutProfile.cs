namespace NFSeNacionalSdk.Core.Enums;

/// <summary>Immutable NFS-e layout profiles supported by the SDK.</summary>
public enum NFSeLayoutProfile
{
    /// <summary>Production XSD bundle published on 2026-02-09 (numeric CNPJ).</summary>
    LegacyV101_202602 = 1,

    /// <summary>RTC XSD bundle published for restricted production on 2026-07-27 (alphanumeric CNPJ).</summary>
    RtcV101_202607 = 2
}

public static class NFSeLayoutDefaults
{
    public static NFSeLayoutProfile Current => NFSeLayoutProfile.RtcV101_202607;
}
