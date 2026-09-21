using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Serialization.Xml.Layout;

namespace NFSeNacionalSdk.Serialization.Xml.Transmission;

internal sealed class EmitDpsXmlSchemaValidator
{
    public void Validate(string xmlContent, NFSeLayoutProfile profile)
    {
        new EmbeddedSchemaValidator(
            NFSeLayoutProfileInfo.Resolve(profile),
            "DPS_v1.01.xsd",
            "DPS").Validate(xmlContent);
    }
}
