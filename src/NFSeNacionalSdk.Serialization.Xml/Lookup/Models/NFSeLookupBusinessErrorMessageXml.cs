using System.Xml.Serialization;

namespace NFSeNacionalSdk.Serialization.Xml.Lookup.Models;

[XmlRoot("MensagemRetorno")]
public sealed class NFSeLookupBusinessErrorMessageXml
{
    [XmlElement("Codigo")]
    public string? Code { get; set; }

    [XmlElement("codigo")]
    public string? CodeLower { get; set; }

    [XmlElement("cStat")]
    public string? StatusCode { get; set; }

    [XmlElement("Mensagem")]
    public string? Message { get; set; }

    [XmlElement("mensagem")]
    public string? MessageLower { get; set; }

    [XmlElement("Descricao")]
    public string? Description { get; set; }

    [XmlElement("descricao")]
    public string? DescriptionLower { get; set; }

    [XmlElement("xMotivo")]
    public string? Reason { get; set; }

    public string? GetResolvedCode()
    {
        return FirstNonEmpty(Code, CodeLower, StatusCode);
    }

    public string? GetResolvedDescription()
    {
        return FirstNonEmpty(Message, MessageLower, Description, DescriptionLower, Reason);
    }

    private static string? FirstNonEmpty(params string?[] candidates)
    {
        return candidates.FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate))?.Trim();
    }
}
