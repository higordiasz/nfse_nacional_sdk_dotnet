using System.Xml.Serialization;

namespace NFSeNacionalSdk.Serialization.Xml.Lookup.Models;

[XmlRoot("NFSe", Namespace = NFSeNacionalSdk.Serialization.Xml.Lookup.NFSeLookupXmlNamespace.SpedNFSe)]
public sealed class NFSeLookupSuccessEnvelopeXml
{
    [XmlAttribute("versao")]
    public string? Version { get; set; }

    [XmlElement("infNFSe")]
    public NFSeLookupInfoXml? Info { get; set; }
}

public sealed class NFSeLookupInfoXml
{
    [XmlAttribute("Id")]
    public string? Id { get; set; }

    [XmlElement("nNFSe")]
    public string? Number { get; set; }

    [XmlElement("cStat")]
    public string? StatusCode { get; set; }

    [XmlElement("dhProc")]
    public string? ProcessedAt { get; set; }

    [XmlElement("emit")]
    public NFSeLookupPartyXml? Issuer { get; set; }

    [XmlElement("DPS")]
    public NFSeLookupDpsXml? Dps { get; set; }
}

public sealed class NFSeLookupDpsXml
{
    [XmlElement("dhEmi")]
    public string? IssuedAt { get; set; }

    [XmlElement("prest")]
    public NFSeLookupPartyXml? Provider { get; set; }

    [XmlElement("toma")]
    public NFSeLookupPartyXml? Recipient { get; set; }

    [XmlElement("serv")]
    public NFSeLookupServiceXml? Service { get; set; }

    [XmlElement("valores")]
    public NFSeLookupValuesXml? Values { get; set; }
}

public sealed class NFSeLookupPartyXml
{
    [XmlElement("CNPJ")]
    public string? Cnpj { get; set; }

    [XmlElement("CPF")]
    public string? Cpf { get; set; }

    [XmlElement("IM")]
    public string? MunicipalRegistration { get; set; }

    [XmlElement("xNome")]
    public string? Name { get; set; }

    [XmlElement("email")]
    public string? Email { get; set; }
}

public sealed class NFSeLookupServiceXml
{
    [XmlElement("cServ")]
    public NFSeLookupServiceCodeXml? Code { get; set; }

    [XmlElement("xDescServ")]
    public string? Description { get; set; }
}

public sealed class NFSeLookupServiceCodeXml
{
    [XmlElement("cTribNac")]
    public string? NationalTaxCode { get; set; }
}

public sealed class NFSeLookupValuesXml
{
    [XmlElement("vServPrest")]
    public NFSeLookupServiceValuesXml? ServiceValues { get; set; }
}

public sealed class NFSeLookupServiceValuesXml
{
    [XmlElement("vServ")]
    public string? ServiceAmount { get; set; }
}
