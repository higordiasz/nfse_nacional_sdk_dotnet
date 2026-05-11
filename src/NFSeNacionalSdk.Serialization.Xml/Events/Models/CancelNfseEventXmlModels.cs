using System.Xml.Serialization;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml.Events.Models;

[XmlRoot("pedRegEvento", Namespace = NFSeLookupXmlNamespace.SpedNFSe)]
public sealed class CancelNfseEventEnvelopeXml
{
    [XmlAttribute("versao")]
    public string Version { get; set; } = "1.01";

    [XmlElement("infPedReg")]
    public CancelNfseEventInfoXml Info { get; set; }
}

public sealed class CancelNfseEventInfoXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; }

    [XmlElement("tpAmb")]
    public string EnvironmentType { get; set; }

    [XmlElement("verAplic")]
    public string ApplicationVersion { get; set; }

    [XmlElement("dhEvento")]
    public string EventAt { get; set; }

    [XmlElement("CNPJAutor")]
    public string? AuthorCnpj { get; set; }

    [XmlElement("CPFAutor")]
    public string? AuthorCpf { get; set; }

    [XmlElement("chNFSe")]
    public string AccessKey { get; set; }

    [XmlElement("e101101")]
    public CancelNfseEventDetailXml Cancellation { get; set; }

    public bool ShouldSerializeAuthorCnpj() => AuthorCnpj is not null;

    public bool ShouldSerializeAuthorCpf() => AuthorCpf is not null;
}

public sealed class CancelNfseEventDetailXml
{
    [XmlElement("xDesc")]
    public string Description { get; set; } = "Cancelamento de NFS-e";

    [XmlElement("cMotivo")]
    public string ReasonCode { get; set; }

    [XmlElement("xMotivo")]
    public string Reason { get; set; }
}
