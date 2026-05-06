using System.Globalization;
using System.Xml.Linq;
using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Core.Exceptions;

namespace NFSeNacionalSdk.Serialization.Xml.Events;

public sealed class NFSeEventXmlResponseParser
{
    public NFSeEventDocument Deserialize(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new NFSeSerializationException("Event XML content cannot be null or empty.");
        }

        try
        {
            var document = XDocument.Parse(content, LoadOptions.PreserveWhitespace);
            var info = FindFirst(document.Root, "infEvento");
            var eventRequestInfo = FindFirst(document.Root, "infPedReg");
            var cancellation = FindFirst(document.Root, "e101101");
            var id = GetAttribute(info, "Id");
            var accessKey = GetElementValue(eventRequestInfo, "chNFSe") ?? ExtractAccessKey(id);
            var typeCode = ExtractTypeCode(id) ?? (cancellation is null ? null : "101101");

            return new NFSeEventDocument
            {
                Id = id,
                AccessKey = accessKey,
                TypeCode = typeCode,
                Description = GetElementValue(cancellation, "xDesc"),
                SequenceNumber = ParseInt(GetElementValue(info, "nSeqEvento")),
                ProcessedAt = ParseDateTimeOffset(GetElementValue(info, "dhProc")),
                AuthorTaxId = GetElementValue(eventRequestInfo, "CNPJAutor") ?? GetElementValue(eventRequestInfo, "CPFAutor"),
                ReasonCode = GetElementValue(cancellation, "cMotivo"),
                Reason = GetElementValue(cancellation, "xMotivo")
            };
        }
        catch (NFSeSerializationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or InvalidOperationException)
        {
            throw new NFSeSerializationException("Failed to deserialize the NFS-e event XML content.", exception);
        }
    }

    private static XElement? FindFirst(XElement? element, string localName)
    {
        return element?
            .DescendantsAndSelf()
            .FirstOrDefault(current => string.Equals(current.Name.LocalName, localName, StringComparison.Ordinal));
    }

    private static string? GetAttribute(XElement? element, string localName)
    {
        var value = element?
            .Attributes()
            .FirstOrDefault(attribute => string.Equals(attribute.Name.LocalName, localName, StringComparison.Ordinal))?
            .Value
            .Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? GetElementValue(XElement? element, string localName)
    {
        var value = element?
            .Elements()
            .FirstOrDefault(current => string.Equals(current.Name.LocalName, localName, StringComparison.Ordinal))?
            .Value
            .Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int? ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : null;
    }

    private static string? ExtractAccessKey(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || eventId.Length < 59)
        {
            return null;
        }

        if (!eventId.StartsWith("EVT", StringComparison.Ordinal) &&
            !eventId.StartsWith("PRE", StringComparison.Ordinal))
        {
            return null;
        }

        return eventId.Substring(3, 50);
    }

    private static string? ExtractTypeCode(string? eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId) || eventId.Length < 59)
        {
            return null;
        }

        if (!eventId.StartsWith("EVT", StringComparison.Ordinal) &&
            !eventId.StartsWith("PRE", StringComparison.Ordinal))
        {
            return null;
        }

        return eventId.Substring(53, 6);
    }
}
