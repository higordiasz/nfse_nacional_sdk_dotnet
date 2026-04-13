using System.Xml.Serialization;

namespace NFSeNacionalSdk.Serialization.Xml.Lookup.Models;

[XmlRoot("ListaMensagemRetorno")]
public sealed class NFSeLookupBusinessErrorEnvelopeXml
{
    [XmlElement("MensagemRetorno")]
    public List<NFSeLookupBusinessErrorMessageXml> Messages { get; set; } = [];
}
