using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using NFSeNacionalSdk.Core.Exceptions;

namespace NFSeNacionalSdk.Serialization.Xml;

internal sealed class NFSeXmlSigner
{
    private const string XmlDsigSha256DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

    public string Sign(string xmlContent, string referenceId, X509Certificate2 certificate)
    {
        if (string.IsNullOrWhiteSpace(xmlContent)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(xmlContent)); }
        if (string.IsNullOrWhiteSpace(referenceId)) { throw new ArgumentException("Value cannot be null or whitespace.", nameof(referenceId)); }
        if (certificate is null) { throw new ArgumentNullException(nameof(certificate)); }

        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa is null)
        {
            throw new NFSeSerializationException(
                "The signing certificate does not expose an RSA private key for XMLDSIG.");
        }

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        try
        {
            document.LoadXml(xmlContent);

            var signedXml = new NFSeSignedXml(document)
            {
                SigningKey = rsa
            };

            signedXml.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigExcC14NWithCommentsTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            var reference = new Reference
            {
                Uri = $"#{referenceId}",
                DigestMethod = XmlDsigSha256DigestMethod
            };

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NWithCommentsTransform());

            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            var x509Data = new KeyInfoX509Data(certificate);
            keyInfo.AddClause(x509Data);
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            if (document.DocumentElement is null)
            {
                throw new NFSeSerializationException("The XML document does not contain a root element to receive the signature.");
            }

            var signatureElement = signedXml.GetXml();
            document.DocumentElement.AppendChild(document.ImportNode(signatureElement, deep: true));

            return Save(document);
        }
        catch (NFSeSerializationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is CryptographicException or XmlException)
        {
            throw new NFSeSerializationException("Failed to generate the XMLDSIG signature for the NFSe XML document.", exception);
        }
    }

    private static string Save(XmlDocument document)
    {
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
            document.Save(writer);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private sealed class NFSeSignedXml(XmlDocument document) : SignedXml(document)
    {
        public override XmlElement? GetIdElement(XmlDocument? document, string idValue)
        {
            return base.GetIdElement(document, idValue) ?? FindByIdAttribute(document, idValue);
        }

        private static XmlElement? FindByIdAttribute(XmlDocument? document, string idValue)
        {
            if (document?.DocumentElement is null)
            {
                return null;
            }

            foreach (XmlElement element in document.GetElementsByTagName("*"))
            {
                if (string.Equals(element.GetAttribute("Id"), idValue, StringComparison.Ordinal))
                {
                    return element;
                }
            }

            return null;
        }
    }
}
