using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml.Lookup;

namespace NFSeNacionalSdk.Serialization.Xml.Layout;

internal sealed class EmbeddedSchemaValidator
{
    private static readonly ConcurrentDictionary<string, Lazy<XmlSchemaSet>> SchemaSets = new();

    private readonly Lazy<XmlSchemaSet> _schemaSet;
    private readonly string _documentName;
    private readonly NFSeLayoutProfileInfo _profile;

    public EmbeddedSchemaValidator(NFSeLayoutProfileInfo profile, string rootSchemaFileName, string documentName)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _documentName = string.IsNullOrWhiteSpace(documentName)
            ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(documentName))
            : documentName;
        var cacheKey = $"{profile.ResourceSegment}/{rootSchemaFileName}";
        _schemaSet = SchemaSets.GetOrAdd(
            cacheKey,
            _ => new Lazy<XmlSchemaSet>(
                () => CreateSchemaSet(profile, rootSchemaFileName),
                LazyThreadSafetyMode.ExecutionAndPublication));
    }

    public void Validate(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new NFSeSerializationException($"{_documentName} XML content cannot be null or empty.");
        }

        try
        {
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });
            var document = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var errors = new List<string>();
            document.Validate(
                _schemaSet.Value,
                (sender, args) =>
                {
                    if (args.Severity == XmlSeverityType.Error)
                    {
                        errors.Add(FormatValidationError(sender, args.Exception));
                    }
                },
                addSchemaInfo: false);

            if (errors.Count > 0)
            {
                throw new NFSeSerializationException(
                    $"{_documentName} XML is not valid against layout profile {_profile.Profile} (XML {_profile.XmlVersion}). " +
                    string.Join(" ", errors));
            }
        }
        catch (NFSeSerializationException) { throw; }
        catch (XmlException exception)
        {
            throw new NFSeSerializationException($"{_documentName} XML is malformed.", exception);
        }
        catch (XmlSchemaException exception)
        {
            throw new NFSeSerializationException(
                $"Failed to validate {_documentName} XML against layout profile {_profile.Profile}.",
                exception);
        }
    }

    private static XmlSchemaSet CreateSchemaSet(NFSeLayoutProfileInfo profile, string rootSchemaFileName)
    {
        var resolver = new EmbeddedSchemaResolver(profile);
        var schemaSet = new XmlSchemaSet { XmlResolver = resolver };
        AddSchema(schemaSet, resolver, "xmldsig-core-schema.xsd", "http://www.w3.org/2000/09/xmldsig#");
        AddSchema(schemaSet, resolver, rootSchemaFileName, NFSeLookupXmlNamespace.SpedNFSe);
        schemaSet.Compile();
        return schemaSet;
    }

    private static void AddSchema(
        XmlSchemaSet schemaSet,
        EmbeddedSchemaResolver resolver,
        string fileName,
        string targetNamespace)
    {
        using var stream = resolver.OpenSchema(fileName);
        using var reader = XmlReader.Create(
            stream,
            // The official XMLDSig XSD contains internal DTD declarations. External resolution remains disabled.
            new XmlReaderSettings { DtdProcessing = DtdProcessing.Parse, XmlResolver = null },
            new Uri(resolver.BaseSchemaUri, fileName).ToString());
        schemaSet.Add(targetNamespace, reader);
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
        return string.Join(" ", new[]
        {
            path is null ? null : $"Path '{path}':",
            location is null ? null : $"({location})",
            exception.Message
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string? GetElementPath(XElement? element) => element is null
        ? null
        : "/" + string.Join("/", element.AncestorsAndSelf().Reverse().Select(current => current.Name.LocalName));

    private sealed class EmbeddedSchemaResolver : XmlResolver
    {
        private readonly Assembly _assembly = typeof(EmbeddedSchemaValidator).Assembly;
        private readonly string _resourcePrefix;

        public EmbeddedSchemaResolver(NFSeLayoutProfileInfo profile)
        {
            _resourcePrefix = $"NFSeNacionalSdk.Serialization.Xml.Schemas.{profile.ResourceSegment}.";
            BaseSchemaUri = new Uri($"nfse-schema://{profile.ResourceSegment}/");
        }

        public Uri BaseSchemaUri { get; }

        public override ICredentials? Credentials { set { } }

        public Stream OpenSchema(string fileName)
        {
            var resourceName = _resourcePrefix + fileName;
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

            return Uri.TryCreate(relativeUri, UriKind.Absolute, out var absoluteUri)
                ? absoluteUri
                : new Uri(baseUri ?? BaseSchemaUri, relativeUri);
        }
    }
}
