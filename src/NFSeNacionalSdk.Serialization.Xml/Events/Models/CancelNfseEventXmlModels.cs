using System.Xml.Serialization;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml.Events.Models;

[XmlRoot("pedRegEvento", Namespace = NFSeLookupXmlNamespace.SpedNFSe)]
public sealed class CancelNfseEventEnvelopeXml
{
    [XmlAttribute("versao")]
    public string Version { get; set; } = "1.01";

    [XmlElement("infPedReg")]
    public CancelNfseEventInfoXml Info { get; set; } = new();
}

public sealed class CancelNfseEventInfoXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("tpAmb")]
    public string EnvironmentType { get; set; } = string.Empty;

    [XmlElement("verAplic")]
    public string ApplicationVersion { get; set; } = string.Empty;

    [XmlElement("dhEvento")]
    public string EventAt { get; set; } = string.Empty;

    [XmlElement("CNPJAutor")]
    public string? AuthorCnpj { get; set; }

    [XmlElement("CPFAutor")]
    public string? AuthorCpf { get; set; }

    [XmlElement("chNFSe")]
    public string AccessKey { get; set; } = string.Empty;

    [XmlElement("e101101")]
    public CancelNfseEventDetailXml Cancellation { get; set; } = new();

    public bool ShouldSerializeAuthorCnpj() => AuthorCnpj is not null;

    public bool ShouldSerializeAuthorCpf() => AuthorCpf is not null;
}

public sealed class CancelNfseEventDetailXml
{
    [XmlElement("xDesc")]
    public string Description { get; set; } = "Cancelamento de NFS-e";

    [XmlElement("cMotivo")]
    public string ReasonCode { get; set; } = string.Empty;

    [XmlElement("xMotivo")]
    public string Reason { get; set; } = string.Empty;
}
