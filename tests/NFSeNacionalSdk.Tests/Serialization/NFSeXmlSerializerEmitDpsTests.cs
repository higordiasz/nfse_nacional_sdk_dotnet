using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
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
        Assert.Single(document.GetElementsByTagName("X509Certificate", SignedXml.XmlDsigNamespaceUrl).OfType<XmlElement>());

        var signatureElement = Assert.IsType<XmlElement>(
            document.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl).Item(0));
        var signedXml = new SignedXml(document);
        signedXml.LoadXml(signatureElement);

        Assert.True(signedXml.CheckSignature(certificate, verifySignatureOnly: true));
    }

    [Fact]
    public void SerializeSignedDps_ShouldMapOptionalValueAndTaxationFields()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(includeOptionalValues: true),
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate,
                ApplicationVersion = "NFSeNacionalSdk_Tests"
            });

        Assert.Contains("<vReceb>1450.75</vReceb>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<vDescIncond>100.00</vDescIncond>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<vDescCond>50.25</vDescCond>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<pAliq>5.00</pAliq>", result.XmlContent, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldGenerateXmlValidAgainstOfficialDpsSchema()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(includeOptionalValues: true),
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate,
                ApplicationVersion = "NFSeNacionalSdk_Tests"
            });

        var schemaSet = new XmlSchemaSet
        {
            XmlResolver = new XmlUrlResolver()
        };

        var schemaDirectory = Path.Combine(AppContext.BaseDirectory, "TestData", "Schemas", "1.01");
        AddSchema(
            schemaSet,
            SignedXml.XmlDsigNamespaceUrl,
            Path.Combine(schemaDirectory, "xmldsig-core-schema.xsd"));
        AddSchema(
            schemaSet,
            "http://www.sped.fazenda.gov.br/nfse",
            Path.Combine(schemaDirectory, "DPS_v1.01.xsd"));
        schemaSet.Compile();

        var errors = new List<string>();
        var document = XDocument.Parse(result.XmlContent, LoadOptions.PreserveWhitespace);

        document.Validate(schemaSet, (_, args) => errors.Add(args.Message));

        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    private static void AddSchema(XmlSchemaSet schemaSet, string targetNamespace, string schemaPath)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(schemaPath, settings);
        schemaSet.Add(targetNamespace, reader);
    }
}
