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

    [XmlElement("xLocEmi")]
    public string? IssuingMunicipalityName { get; set; }

    [XmlElement("xLocPrestacao")]
    public string? ServiceLocationMunicipalityName { get; set; }

    [XmlElement("nNFSe")]
    public string? Number { get; set; }

    [XmlElement("cLocIncid")]
    public string? IncidenceMunicipalityCode { get; set; }

    [XmlElement("xLocIncid")]
    public string? IncidenceMunicipalityName { get; set; }

    [XmlElement("xTribNac")]
    public string? NationalTaxationDescription { get; set; }

    [XmlElement("verAplic")]
    public string? ApplicationVersion { get; set; }

    [XmlElement("cStat")]
    public string? StatusCode { get; set; }

    [XmlElement("dhProc")]
    public string? ProcessedAt { get; set; }

    [XmlElement("nDFSe")]
    public string? DfseNumber { get; set; }

    [XmlElement("emit")]
    public NFSeLookupPartyXml? Issuer { get; set; }

    [XmlElement("valores")]
    public NFSeLookupNfseValuesXml? Values { get; set; }

    [XmlElement("DPS")]
    public NFSeLookupDpsXml? Dps { get; set; }
}

public sealed class NFSeLookupDpsXml
{
    [XmlAttribute("Id")]
    public string? LegacyId { get; set; }

    [XmlElement("infDPS")]
    public NFSeLookupDpsInfoXml? Info { get; set; }

    [XmlElement("dhEmi")]
    public string? LegacyIssuedAt { get; set; }

    [XmlElement("serie")]
    public string? LegacySeries { get; set; }

    [XmlElement("nDPS")]
    public string? LegacyNumber { get; set; }

    [XmlElement("dCompet")]
    public string? LegacyCompetenceDate { get; set; }

    [XmlElement("cLocEmi")]
    public string? LegacyMunicipalityCode { get; set; }

    [XmlElement("prest")]
    public NFSeLookupPartyXml? LegacyProvider { get; set; }

    [XmlElement("toma")]
    public NFSeLookupPartyXml? LegacyRecipient { get; set; }

    [XmlElement("serv")]
    public NFSeLookupServiceXml? LegacyService { get; set; }

    [XmlElement("valores")]
    public NFSeLookupValuesXml? LegacyValues { get; set; }
}

public sealed class NFSeLookupDpsInfoXml
{
    [XmlAttribute("Id")]
    public string? Id { get; set; }

    [XmlElement("dhEmi")]
    public string? IssuedAt { get; set; }

    [XmlElement("serie")]
    public string? Series { get; set; }

    [XmlElement("nDPS")]
    public string? Number { get; set; }

    [XmlElement("dCompet")]
    public string? CompetenceDate { get; set; }

    [XmlElement("cLocEmi")]
    public string? MunicipalityCode { get; set; }

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

    [XmlElement("fone")]
    public string? Phone { get; set; }

    [XmlElement("email")]
    public string? Email { get; set; }

    [XmlElement("enderNac")]
    public NFSeLookupNationalAddressXml? NationalAddress { get; set; }

    [XmlElement("end")]
    public NFSeLookupAddressXml? Address { get; set; }
}

public class NFSeLookupAddressXml
{
    [XmlElement("xLgr")]
    public string? Street { get; set; }

    [XmlElement("nro")]
    public string? Number { get; set; }

    [XmlElement("xCpl")]
    public string? Complement { get; set; }

    [XmlElement("xBairro")]
    public string? Neighborhood { get; set; }

    [XmlElement("cMun")]
    public string? MunicipalityCode { get; set; }

    [XmlElement("UF")]
    public string? State { get; set; }

    [XmlElement("CEP")]
    public string? ZipCode { get; set; }
}

public sealed class NFSeLookupNationalAddressXml : NFSeLookupAddressXml
{
}

public sealed class NFSeLookupServiceXml
{
    [XmlElement("locPrest")]
    public NFSeLookupServiceLocationXml? Location { get; set; }

    [XmlElement("cServ")]
    public NFSeLookupServiceCodeXml? Code { get; set; }

    [XmlElement("xDescServ")]
    public string? LegacyDescription { get; set; }
}

public sealed class NFSeLookupServiceCodeXml
{
    [XmlElement("cTribNac")]
    public string? NationalTaxCode { get; set; }

    [XmlElement("cTribMun")]
    public string? MunicipalTaxCode { get; set; }

    [XmlElement("xDescServ")]
    public string? Description { get; set; }

    [XmlElement("cNBS")]
    public string? NationalClassificationCode { get; set; }

    [XmlElement("cIntContrib")]
    public string? InternalContributorCode { get; set; }
}

public sealed class NFSeLookupServiceLocationXml
{
    [XmlElement("cLocPrestacao")]
    public string? MunicipalityCode { get; set; }
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

public sealed class NFSeLookupNfseValuesXml
{
    [XmlElement("vLiq")]
    public string? NetAmount { get; set; }
}
