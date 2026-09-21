using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Serialization.Xml.Events.Models;
using NFSeNacionalSdk.Serialization.Xml.Lookup;
using NFSeNacionalSdk.Serialization.Xml.Layout;

namespace NFSeNacionalSdk.Serialization.Xml.Events;

internal sealed class CancelNfseEventXmlBuilder
{
    private const string LayoutVersion = "1.01";
    private const string CancellationEventTypeCode = "101101";
    private const int ApplicationVersionMaxLength = 20;

    private readonly NFSeXmlSigner _signer = new();
    private readonly EventXmlSchemaValidator _schemaValidator = new("pedRegEvento_v1.01.xsd", "Pedido de registro de evento");

    public CancelNfseSerializationResult Build(
        CancelNfseRequest request,
        CancelNfseSerializationContext context)
    {
        if (request is null) { throw new ArgumentNullException(nameof(request)); }
        if (context is null) { throw new ArgumentNullException(nameof(context)); }

        if (context.SigningCertificate is null)
        {
            throw new NFSeSerializationException("A signing certificate is required to generate a cancellation event.");
        }

        var profile = NFSeLayoutProfileInfo.Resolve(context.LayoutProfile);
        var accessKey = NormalizeAccessKey(request.AccessKey, profile);
        var authorTaxId = BrazilianTaxId.Parse(request.AuthorTaxId, nameof(request.AuthorTaxId), profile);
        var reason = EnsureTextLength(request.Reason, nameof(request.Reason), 15, 255);
        var eventRequestId = BuildEventRequestId(accessKey);

        var envelope = new CancelNfseEventEnvelopeXml
        {
            Version = LayoutVersion,
            Info = new CancelNfseEventInfoXml
            {
                Id = eventRequestId,
                EnvironmentType = ((int)context.Environment).ToString(CultureInfo.InvariantCulture),
                ApplicationVersion = NormalizeApplicationVersion(context.ApplicationVersion),
                EventAt = (request.EventAt ?? DateTimeOffset.Now).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                AuthorCnpj = authorTaxId.IsCnpj ? authorTaxId.Value : null,
                AuthorCpf = authorTaxId.IsCnpj ? null : authorTaxId.Value,
                AccessKey = accessKey,
                Cancellation = new CancelNfseEventDetailXml
                {
                    ReasonCode = ((int)request.ReasonCode).ToString(CultureInfo.InvariantCulture),
                    Reason = reason
                }
            }
        };

        var unsignedXml = SerializeUnsigned(envelope);
        var signedXml = _signer.Sign(unsignedXml, eventRequestId, context.SigningCertificate);
        _schemaValidator.Validate(signedXml, context.LayoutProfile);

        return new CancelNfseSerializationResult
        {
            EventRequestId = eventRequestId,
            XmlContent = signedXml
        };
    }

    private static string SerializeUnsigned(CancelNfseEventEnvelopeXml envelope)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(CancelNfseEventEnvelopeXml));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, NFSeLookupXmlNamespace.SpedNFSe);

            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(
                       stream,
                       new XmlWriterSettings
                       {
                           Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                           Indent = false,
                           OmitXmlDeclaration = false
                       }))
            {
                serializer.Serialize(writer, envelope, namespaces);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (InvalidOperationException exception)
        {
            throw new NFSeSerializationException("Failed to serialize the NFS-e cancellation event XML document.", exception);
        }
    }

    private static string BuildEventRequestId(string accessKey)
    {
        return string.Concat("PRE", accessKey, CancellationEventTypeCode);
    }

    private static string NormalizeAccessKey(string? value, NFSeLayoutProfileInfo profile)
    {
        var normalized = EnsureNotWhiteSpace(value, nameof(CancelNfseRequest.AccessKey)).Trim().ToUpperInvariant();
        var valid = normalized.Length == 50 && normalized.All(character =>
            character is >= '0' and <= '9' ||
            profile.SupportsAlphanumericCnpj && character is >= 'A' and <= 'Z');
        if (!valid)
        {
            throw new NFSeSerializationException(
                $"{nameof(CancelNfseRequest.AccessKey)} must contain 50 " +
                (profile.SupportsAlphanumericCnpj ? "uppercase alphanumeric characters" : "numeric digits") +
                $" for layout profile {profile.Profile}.");
        }

        return normalized!;
    }

    private static string EnsureTextLength(string? value, string parameterName, int minLength, int maxLength)
    {
        var normalized = EnsureNotWhiteSpace(value, parameterName);

        if (normalized.Length < minLength || normalized.Length > maxLength)
        {
            throw new NFSeSerializationException(
                $"{parameterName} must contain between {minLength} and {maxLength} characters.");
        }

        return normalized;
    }

    private static string EnsureNotWhiteSpace(string? value, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new NFSeSerializationException($"{parameterName} must be informed.");
        }

        return normalized!;
    }

    private static string NormalizeApplicationVersion(string? value)
    {
        var normalized = value?.Trim();
        var result = string.IsNullOrWhiteSpace(normalized) ? "NFSeNacionalSdk" : normalized!;
        return result.Length <= ApplicationVersionMaxLength
            ? result
            : result.Substring(0, ApplicationVersionMaxLength);
    }

}
