using System.Xml.Serialization;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml.Transmission.Models;

[XmlRoot("DPS", Namespace = NFSeLookupXmlNamespace.SpedNFSe)]
public sealed class EmitDpsEnvelopeXml
{
    [XmlAttribute("versao")]
    public string Version { get; set; } = "1.01";

    [XmlElement("infDPS", Order = 0)]
    public EmitDpsInfoXml Info { get; set; } = new();
}

public sealed class EmitDpsInfoXml
{
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("tpAmb", Order = 0)]
    public string EnvironmentType { get; set; } = string.Empty;

    [XmlElement("dhEmi", Order = 1)]
    public string IssuedAt { get; set; } = string.Empty;

    [XmlElement("verAplic", Order = 2)]
    public string ApplicationVersion { get; set; } = string.Empty;

    [XmlElement("serie", Order = 3)]
    public string Series { get; set; } = string.Empty;

    [XmlElement("nDPS", Order = 4)]
    public string Number { get; set; } = string.Empty;

    [XmlElement("dCompet", Order = 5)]
    public string CompetenceDate { get; set; } = string.Empty;

    [XmlElement("tpEmit", Order = 6)]
    public string EmitterType { get; set; } = string.Empty;

    [XmlElement("cLocEmi", Order = 7)]
    public string MunicipalityCode { get; set; } = string.Empty;

    [XmlElement("prest", Order = 8)]
    public EmitDpsProviderXml Provider { get; set; } = new();

    [XmlElement("toma", Order = 9)]
    public EmitDpsPersonXml? Recipient { get; set; }

    [XmlElement("serv", Order = 10)]
    public EmitDpsServiceXml Service { get; set; } = new();

    [XmlElement("valores", Order = 11)]
    public EmitDpsValuesXml Values { get; set; } = new();
}

public sealed class EmitDpsProviderXml
{
    [XmlElement("CNPJ", Order = 0)]
    public string? Cnpj { get; set; }

    [XmlElement("CPF", Order = 1)]
    public string? Cpf { get; set; }

    [XmlElement("IM", Order = 2)]
    public string? MunicipalRegistration { get; set; }

    [XmlElement("xNome", Order = 3)]
    public string? Name { get; set; }

    [XmlElement("end", Order = 4)]
    public EmitDpsAddressXml? Address { get; set; }

    [XmlElement("fone", Order = 5)]
    public string? Phone { get; set; }

    [XmlElement("email", Order = 6)]
    public string? Email { get; set; }

    [XmlElement("regTrib", Order = 7)]
    public EmitDpsProviderTaxRegimeXml TaxRegime { get; set; } = new();
}

public sealed class EmitDpsProviderTaxRegimeXml
{
    [XmlElement("opSimpNac", Order = 0)]
    public string SimplesNationalOption { get; set; } = string.Empty;

    [XmlElement("regApTribSN", Order = 1)]
    public string? SimplifiedNationalTaxRegime { get; set; }

    [XmlElement("regEspTrib", Order = 2)]
    public string SpecialTaxRegime { get; set; } = string.Empty;
}

public sealed class EmitDpsPersonXml
{
    [XmlElement("CNPJ", Order = 0)]
    public string? Cnpj { get; set; }

    [XmlElement("CPF", Order = 1)]
    public string? Cpf { get; set; }

    [XmlElement("IM", Order = 2)]
    public string? MunicipalRegistration { get; set; }

    [XmlElement("xNome", Order = 3)]
    public string Name { get; set; } = string.Empty;

    [XmlElement("end", Order = 4)]
    public EmitDpsAddressXml? Address { get; set; }

    [XmlElement("fone", Order = 5)]
    public string? Phone { get; set; }

    [XmlElement("email", Order = 6)]
    public string? Email { get; set; }
}

public sealed class EmitDpsAddressXml
{
    [XmlElement("endNac", Order = 0)]
    public EmitDpsNationalAddressXml NationalAddress { get; set; } = new();

    [XmlElement("xLgr", Order = 1)]
    public string Street { get; set; } = string.Empty;

    [XmlElement("nro", Order = 2)]
    public string Number { get; set; } = string.Empty;

    [XmlElement("xCpl", Order = 3)]
    public string? Complement { get; set; }

    [XmlElement("xBairro", Order = 4)]
    public string Neighborhood { get; set; } = string.Empty;
}

public sealed class EmitDpsNationalAddressXml
{
    [XmlElement("cMun", Order = 0)]
    public string MunicipalityCode { get; set; } = string.Empty;

    [XmlElement("CEP", Order = 1)]
    public string ZipCode { get; set; } = string.Empty;
}

public sealed class EmitDpsServiceXml
{
    [XmlElement("locPrest", Order = 0)]
    public EmitDpsServiceLocationXml Location { get; set; } = new();

    [XmlElement("cServ", Order = 1)]
    public EmitDpsServiceCodeXml Code { get; set; } = new();
}

public sealed class EmitDpsServiceLocationXml
{
    [XmlElement("cLocPrestacao", Order = 0)]
    public string MunicipalityCode { get; set; } = string.Empty;
}

public sealed class EmitDpsServiceCodeXml
{
    [XmlElement("cTribNac", Order = 0)]
    public string NationalTaxationCode { get; set; } = string.Empty;

    [XmlElement("cTribMun", Order = 1)]
    public string? MunicipalTaxationCode { get; set; }

    [XmlElement("xDescServ", Order = 2)]
    public string Description { get; set; } = string.Empty;

    [XmlElement("cNBS", Order = 3)]
    public string? NationalClassificationCode { get; set; }

    [XmlElement("cIntContrib", Order = 4)]
    public string? InternalCode { get; set; }
}

public sealed class EmitDpsValuesXml
{
    [XmlElement("vServPrest", Order = 0)]
    public EmitDpsServiceValuesXml ServiceValues { get; set; } = new();

    [XmlElement("trib", Order = 1)]
    public EmitDpsTaxationXml Taxation { get; set; } = new();
}

public sealed class EmitDpsServiceValuesXml
{
    [XmlElement("vServ", Order = 1)]
    public decimal Amount { get; set; }
}

public sealed class EmitDpsTaxationXml
{
    [XmlElement("tribMun", Order = 0)]
    public EmitDpsMunicipalTaxationXml MunicipalTaxation { get; set; } = new();

    [XmlElement("totTrib", Order = 1)]
    public EmitDpsTotalTaxXml TotalTax { get; set; } = new();
}

public sealed class EmitDpsMunicipalTaxationXml
{
    [XmlElement("tribISSQN", Order = 0)]
    public string IssTaxationType { get; set; } = string.Empty;

    [XmlElement("tpRetISSQN", Order = 1)]
    public string IssWithholdingType { get; set; } = string.Empty;
}

public sealed class EmitDpsTotalTaxXml
{
    [XmlElement("indTotTrib", Order = 0)]
    public string Indicator { get; set; } = string.Empty;
}
