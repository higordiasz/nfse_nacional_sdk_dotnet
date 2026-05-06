using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml.Events;

internal sealed class EventXmlSchemaValidator
{
    private const string ResourcePrefix = "NFSeNacionalSdk.Serialization.Xml.Schemas.1.01.";
    private static readonly Uri BaseSchemaUri = new("nfse-schema://1.01/");

    private readonly Lazy<XmlSchemaSet> _schemaSet;
    private readonly string _documentName;

    public EventXmlSchemaValidator(string rootSchemaFileName, string documentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootSchemaFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentName);

        _schemaSet = new Lazy<XmlSchemaSet>(
            () => CreateSchemaSet(rootSchemaFileName),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _documentName = documentName;
    }

    public void Validate(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new NFSeSerializationException($"{_documentName} XML content cannot be null or empty.");
        }

        try
        {
            var document = XDocument.Parse(
                xmlContent,
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var errors = new List<string>();

            document.Validate(
                _schemaSet.Value,
                (sender, args) =>
                {
                    if (args.Severity != XmlSeverityType.Error)
                    {
                        return;
                    }

                    errors.Add(FormatValidationError(sender, args.Exception));
                },
                addSchemaInfo: false);

            if (errors.Count > 0)
            {
                throw new NFSeSerializationException(
                    $"{_documentName} XML is not valid against the official NFS-e 1.01 schema. " +
                    string.Join(" ", errors));
            }
        }
        catch (NFSeSerializationException)
        {
            throw;
        }
        catch (XmlException exception)
        {
            throw new NFSeSerializationException($"{_documentName} XML is malformed.", exception);
        }
        catch (XmlSchemaException exception)
        {
            throw new NFSeSerializationException(
                $"Failed to validate {_documentName} XML against the official NFS-e 1.01 schema.",
                exception);
        }
    }

    private static XmlSchemaSet CreateSchemaSet(string rootSchemaFileName)
    {
        var resolver = new EmbeddedSchemaResolver();
        var schemaSet = new XmlSchemaSet
        {
            XmlResolver = resolver
        };

        using var signatureStream = resolver.OpenSchema("xmldsig-core-schema.xsd");
        using var signatureReader = XmlReader.Create(
            signatureStream,
            new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = null
            },
            new Uri(BaseSchemaUri, "xmldsig-core-schema.xsd").ToString());

        schemaSet.Add("http://www.w3.org/2000/09/xmldsig#", signatureReader);

        using var stream = resolver.OpenSchema(rootSchemaFileName);
        using var reader = XmlReader.Create(
            stream,
            new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = null
            },
            new Uri(BaseSchemaUri, rootSchemaFileName).ToString());

        schemaSet.Add(NFSeLookupXmlNamespace.SpedNFSe, reader);
        schemaSet.Compile();

        return schemaSet;
    }

    private static string FormatValidationError(object? sender, XmlSchemaException exception)
    {
        var path = sender switch
        {
            XElement element => GetElementPath(element),
            XAttribute attribute => $"{GetElementPath(attribute.Parent)}/@{attribute.Name.LocalName}",
            _ => null
        };

        var location = exception.LineNumber > 0
            ? $"line {exception.LineNumber}, position {exception.LinePosition}"
            : null;

        return string.Join(
            " ",
            new[]
            {
                path is null ? null : $"Path '{path}':",
                location is null ? null : $"({location})",
                exception.Message
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string? GetElementPath(XElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var names = element
            .AncestorsAndSelf()
            .Reverse()
            .Select(current => current.Name.LocalName);

        return "/" + string.Join("/", names);
    }

    private sealed class EmbeddedSchemaResolver : XmlResolver
    {
        private readonly Assembly _assembly = typeof(EventXmlSchemaValidator).Assembly;

        public override ICredentials? Credentials
        {
            set { }
        }

        public Stream OpenSchema(string fileName)
        {
            var resourceName = ResourcePrefix + fileName;
            return _assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Embedded NFS-e schema resource was not found: {resourceName}");
        }

        public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn)
        {
            if (ofObjectToReturn is not null && ofObjectToReturn != typeof(Stream))
            {
                throw new XmlException($"Unsupported schema resource type requested: {ofObjectToReturn.FullName}");
            }

            return OpenSchema(Path.GetFileName(absoluteUri.LocalPath));
        }

        public override Uri ResolveUri(Uri? baseUri, string? relativeUri)
        {
            if (string.IsNullOrWhiteSpace(relativeUri))
            {
                return baseUri ?? BaseSchemaUri;
            }

            if (Uri.TryCreate(relativeUri, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri;
            }

            return new Uri(baseUri ?? BaseSchemaUri, relativeUri);
        }
    }
}
