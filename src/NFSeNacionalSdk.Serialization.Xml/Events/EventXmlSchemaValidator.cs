using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Serialization.Xml.Layout;

namespace NFSeNacionalSdk.Serialization.Xml.Events;

internal sealed class EventXmlSchemaValidator
{
    private readonly string _rootSchemaFileName;
    private readonly string _documentName;

    public EventXmlSchemaValidator(string rootSchemaFileName, string documentName)
    {
        _rootSchemaFileName = rootSchemaFileName;
        _documentName = documentName;
    }

    public void Validate(string xmlContent, NFSeLayoutProfile profile)
    {
        new EmbeddedSchemaValidator(
            NFSeLayoutProfileInfo.Resolve(profile),
            _rootSchemaFileName,
            _documentName).Validate(xmlContent);
    }
}
