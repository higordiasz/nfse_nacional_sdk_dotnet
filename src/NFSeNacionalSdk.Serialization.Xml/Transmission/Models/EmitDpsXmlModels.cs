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

    [XmlElement("IBSCBS", Order = 12)]
    public EmitDpsIbsCbsXml? IbsCbs { get; set; }
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

    [XmlElement("vDescCondIncond", Order = 1)]
    public EmitDpsDiscountValuesXml? DiscountValues { get; set; }

    [XmlElement("trib", Order = 3)]
    public EmitDpsTaxationXml Taxation { get; set; } = new();
}

public sealed class EmitDpsServiceValuesXml
{
    [XmlElement("vReceb", Order = 0)]
    public string? ReceivedAmount { get; set; }

    [XmlElement("vServ", Order = 1)]
    public string Amount { get; set; } = string.Empty;
}

public sealed class EmitDpsDiscountValuesXml
{
    [XmlElement("vDescIncond", Order = 0)]
    public string? UnconditionalAmount { get; set; }

    [XmlElement("vDescCond", Order = 1)]
    public string? ConditionalAmount { get; set; }
}

public sealed class EmitDpsTaxationXml
{
    [XmlElement("tribMun", Order = 0)]
    public EmitDpsMunicipalTaxationXml MunicipalTaxation { get; set; } = new();

    [XmlElement("tribFed", Order = 1)]
    public EmitDpsFederalTaxationXml? FederalTaxation { get; set; }

    [XmlElement("totTrib", Order = 2)]
    public EmitDpsTotalTaxXml TotalTax { get; set; } = new();
}

public sealed class EmitDpsFederalTaxationXml
{
    [XmlElement("piscofins", Order = 0)]
    public EmitDpsPisCofinsTaxationXml? PisCofins { get; set; }

    [XmlElement("vRetCP", Order = 1)]
    public string? SocialSecurityRetentionAmount { get; set; }

    [XmlElement("vRetIRRF", Order = 2)]
    public string? IncomeTaxRetentionAmount { get; set; }

    [XmlElement("vRetCSLL", Order = 3)]
    public string? SocialContributionRetentionAmount { get; set; }
}

public sealed class EmitDpsPisCofinsTaxationXml
{
    [XmlElement("CST", Order = 0)]
    public string TaxStatusCode { get; set; } = string.Empty;

    [XmlElement("vBCPisCofins", Order = 1)]
    public string? CalculationBase { get; set; }

    [XmlElement("pAliqPis", Order = 2)]
    public string? PisRate { get; set; }

    [XmlElement("pAliqCofins", Order = 3)]
    public string? CofinsRate { get; set; }

    [XmlElement("vPis", Order = 4)]
    public string? PisAmount { get; set; }

    [XmlElement("vCofins", Order = 5)]
    public string? CofinsAmount { get; set; }

    [XmlElement("tpRetPisCofins", Order = 6)]
    public string? WithholdingType { get; set; }
}

public sealed class EmitDpsMunicipalTaxationXml
{
    [XmlElement("tribISSQN", Order = 0)]
    public string IssTaxationType { get; set; } = string.Empty;

    [XmlElement("tpRetISSQN", Order = 1)]
    public string IssWithholdingType { get; set; } = string.Empty;

    [XmlElement("pAliq", Order = 2)]
    public string? IssRate { get; set; }
}

public sealed class EmitDpsTotalTaxXml
{
    [XmlElement("indTotTrib", Order = 0)]
    public string? Indicator { get; set; }

    [XmlElement("pTotTribSN", Order = 1)]
    public string? SimplesNationalRate { get; set; }
}

public sealed class EmitDpsIbsCbsXml
{
    [XmlElement("finNFSe", Order = 0)]
    public string Purpose { get; set; } = string.Empty;

    [XmlElement("indFinal", Order = 1)]
    public string? IsFinalConsumer { get; set; }

    [XmlElement("cIndOp", Order = 2)]
    public string OperationIndicatorCode { get; set; } = string.Empty;

    [XmlElement("tpOper", Order = 3)]
    public string? OperationTypeCode { get; set; }

    [XmlElement("indDest", Order = 4)]
    public string DestinationIndicator { get; set; } = string.Empty;

    [XmlElement("valores", Order = 5)]
    public EmitDpsIbsCbsValuesXml Values { get; set; } = new();
}

public sealed class EmitDpsIbsCbsValuesXml
{
    [XmlElement("trib", Order = 0)]
    public EmitDpsIbsCbsTaxXml Taxation { get; set; } = new();
}

public sealed class EmitDpsIbsCbsTaxXml
{
    [XmlElement("gIBSCBS", Order = 0)]
    public EmitDpsIbsCbsGroupXml Group { get; set; } = new();
}

public sealed class EmitDpsIbsCbsGroupXml
{
    [XmlElement("CST", Order = 0)]
    public string TaxStatusCode { get; set; } = string.Empty;

    [XmlElement("cClassTrib", Order = 1)]
    public string TaxClassificationCode { get; set; } = string.Empty;

    [XmlElement("cCredPres", Order = 2)]
    public string? PresumedCreditCode { get; set; }

    [XmlElement("gTribRegular", Order = 3)]
    public EmitDpsIbsCbsRegularXml? RegularTaxation { get; set; }

    [XmlElement("gDif", Order = 4)]
    public EmitDpsIbsCbsDeferralXml? Deferral { get; set; }
}

public sealed class EmitDpsIbsCbsRegularXml
{
    [XmlElement("CSTReg", Order = 0)]
    public string TaxStatusCode { get; set; } = string.Empty;

    [XmlElement("cClassTribReg", Order = 1)]
    public string TaxClassificationCode { get; set; } = string.Empty;
}

public sealed class EmitDpsIbsCbsDeferralXml
{
    [XmlElement("pDifUF", Order = 0)]
    public string StateIbsRate { get; set; } = string.Empty;

    [XmlElement("pDifMun", Order = 1)]
    public string MunicipalIbsRate { get; set; } = string.Empty;

    [XmlElement("pDifCBS", Order = 2)]
    public string CbsRate { get; set; } = string.Empty;
}
