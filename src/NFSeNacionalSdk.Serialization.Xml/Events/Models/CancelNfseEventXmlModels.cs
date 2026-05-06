using System.Xml.Serialization;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml.Events.Models;

[XmlRoot("pedRegEvento", Namespace = NFSeLookupXmlNamespace.SpedNFSe)]
public sealed class CancelNfseEventEnvelopeXml
{
    [XmlAttribute("versao")]
    public string Version { get; init; } = "1.01";

    [XmlElement("infPedReg")]
    public required CancelNfseEventInfoXml Info { get; init; }
}

public sealed class CancelNfseEventInfoXml
{
    [XmlAttribute("Id")]
    public required string Id { get; init; }

    [XmlElement("tpAmb")]
    public required string EnvironmentType { get; init; }

    [XmlElement("verAplic")]
    public required string ApplicationVersion { get; init; }

    [XmlElement("dhEvento")]
    public required string EventAt { get; init; }

    [XmlElement("CNPJAutor")]
    public string? AuthorCnpj { get; init; }

    [XmlElement("CPFAutor")]
    public string? AuthorCpf { get; init; }

    [XmlElement("chNFSe")]
    public required string AccessKey { get; init; }

    [XmlElement("e101101")]
    public required CancelNfseEventDetailXml Cancellation { get; init; }

    public bool ShouldSerializeAuthorCnpj() => AuthorCnpj is not null;

    public bool ShouldSerializeAuthorCpf() => AuthorCpf is not null;
}

public sealed class CancelNfseEventDetailXml
{
    [XmlElement("xDesc")]
    public string Description { get; init; } = "Cancelamento de NFS-e";

    [XmlElement("cMotivo")]
    public required string ReasonCode { get; init; }

    [XmlElement("xMotivo")]
    public required string Reason { get; init; }
}
