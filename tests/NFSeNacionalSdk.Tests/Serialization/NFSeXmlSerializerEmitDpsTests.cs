using System.Security.Cryptography.Xml;
using System.Xml;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Tests.TestData;

namespace NFSeNacionalSdk.Tests.Serialization;

public sealed class NFSeXmlSerializerEmitDpsTests
{
    [Fact]
    public void SerializeSignedDps_ShouldGenerateSignedXmlWithExpectedStructure()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(),
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate,
                ApplicationVersion = "NFSeNacionalSdk_Tests"
            });

        Assert.Equal(NFSeTransmissionFixtures.ExpectedDpsId, result.DpsId);
        Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-8\"?>", result.XmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<DPS", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<Signature", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<tpAmb>2</tpAmb>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<serie>70000</serie>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<nDPS>1</nDPS>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<cTribNac>140101</cTribNac>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<indTotTrib>0</indTotTrib>", result.XmlContent, StringComparison.Ordinal);

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };
        document.LoadXml(result.XmlContent);

        var namespaceManager = new XmlNamespaceManager(document.NameTable);
        namespaceManager.AddNamespace("nfse", "http://www.sped.fazenda.gov.br/nfse");
        namespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

        Assert.NotNull(document.SelectSingleNode("/nfse:DPS/nfse:infDPS", namespaceManager));
        Assert.NotNull(document.SelectSingleNode($"/nfse:DPS/nfse:infDPS[@Id='{NFSeTransmissionFixtures.ExpectedDpsId}']", namespaceManager));
        Assert.NotNull(document.SelectSingleNode("/nfse:DPS/ds:Signature", namespaceManager));

        var signatureElement = Assert.IsType<XmlElement>(
            document.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl).Item(0));
        var signedXml = new SignedXml(document);
        signedXml.LoadXml(signatureElement);

        Assert.True(signedXml.CheckSignature(certificate, verifySignatureOnly: true));
    }
}
