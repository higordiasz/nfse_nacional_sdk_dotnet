using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Serialization.Xml.Layout;

namespace NFSeNacionalSdk.Serialization.Xml;

/// <summary>Validates received XML structure against embedded official XSDs.</summary>
/// <remarks>This validator deliberately does not establish trust or verify XML signatures.</remarks>
public sealed class NFSeReceivedXmlValidator
{
    public void ValidateNfse(string xmlContent) => ValidateNfse(xmlContent, NFSeLayoutDefaults.Current);

    public void ValidateNfse(string xmlContent, NFSeLayoutProfile profile)
    {
        new EmbeddedSchemaValidator(
            NFSeLayoutProfileInfo.Resolve(profile),
            "NFSe_v1.01.xsd",
            "NFS-e response").Validate(xmlContent);
    }

    public void ValidateEvent(string xmlContent) => ValidateEvent(xmlContent, NFSeLayoutDefaults.Current);

    public void ValidateEvent(string xmlContent, NFSeLayoutProfile profile)
    {
        new EmbeddedSchemaValidator(
            NFSeLayoutProfileInfo.Resolve(profile),
            "evento_v1.01.xsd",
            "NFS-e event response").Validate(xmlContent);
    }
}
