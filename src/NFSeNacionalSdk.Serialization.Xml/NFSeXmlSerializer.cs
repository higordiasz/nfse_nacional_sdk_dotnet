using System.Xml;
using System.Xml.Serialization;
using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml;

public sealed class NFSeXmlSerializer : INFSeSerializer
{
    private readonly NFSeLookupXmlResponseParser _lookupParser = new();

    public string Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var writer = new StringWriter();
            serializer.Serialize(writer, value);

            return writer.ToString();
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException)
        {
            throw new NFSeSerializationException(
                $"Failed to serialize value of type '{typeof(T).Name}'.",
                exception);
        }
    }

    public T Deserialize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new NFSeSerializationException("XML content cannot be null or empty.");
        }

        try
        {
            if (typeof(T) == typeof(NFSeDocument))
            {
                var lookupResult = DeserializeLookupResponse(content);
                if (!lookupResult.Success || lookupResult.Document is null)
                {
                    var description = lookupResult.Messages.FirstOrDefault()?.Description
                        ?? "XML content represents a business error and not an NFSe document.";

                    throw new NFSeSerializationException(description);
                }

                return (T)(object)lookupResult.Document;
            }

            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(content);
            var value = serializer.Deserialize(reader);

            if (value is null)
            {
                throw new NFSeSerializationException(
                    $"XML content for '{typeof(T).Name}' produced a null result.");
            }

            return (T)value;
        }
        catch (NFSeSerializationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is InvalidOperationException or XmlException)
        {
            throw new NFSeSerializationException(
                $"Failed to deserialize XML content into '{typeof(T).Name}'.",
                exception);
        }
    }

    public NFSeLookupDeserializationResult DeserializeLookupResponse(string content)
    {
        return _lookupParser.Deserialize(content);
    }
}
