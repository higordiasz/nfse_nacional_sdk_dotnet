using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using NFSeNacionalSdk.Core.Exceptions;

namespace NFSeNacionalSdk.Serialization.Xml.Transmission;

internal sealed class EmitDpsXmlSigner
{
    private const string XmlDsigSha256DigestMethod = "http://www.w3.org/2001/04/xmlenc#sha256";

    public string Sign(string xmlContent, string dpsId, X509Certificate2 certificate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xmlContent);
        ArgumentException.ThrowIfNullOrWhiteSpace(dpsId);
        ArgumentNullException.ThrowIfNull(certificate);

        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa is null)
        {
            throw new NFSeSerializationException(
                "The signing certificate does not expose an RSA private key required for XMLDSIG.");
        }

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        try
        {
            document.LoadXml(xmlContent);

            var signedXml = new EmitDpsSignedXml(document)
            {
                SigningKey = rsa
            };

            signedXml.SignedInfo!.CanonicalizationMethod = SignedXml.XmlDsigExcC14NWithCommentsTransformUrl;
            signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

            var reference = new Reference
            {
                Uri = $"#{dpsId}",
                DigestMethod = XmlDsigSha256DigestMethod
            };

            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(new XmlDsigExcC14NWithCommentsTransform());

            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            var x509Data = new KeyInfoX509Data(certificate);
            x509Data.AddCertificate(certificate);
            keyInfo.AddClause(x509Data);
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            if (document.DocumentElement is null)
            {
                throw new NFSeSerializationException("The DPS XML document does not contain a root element to receive the signature.");
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
            throw new NFSeSerializationException("Failed to generate the XMLDSIG signature for the DPS document.", exception);
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

    private sealed class EmitDpsSignedXml(XmlDocument document) : SignedXml(document)
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
