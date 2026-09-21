using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Exceptions;
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
                LayoutProfile = NFSeLayoutProfile.LegacyV101_202602,
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
        Assert.Contains("<CNPJ>12345678000195</CNPJ>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<CPF>52998224725</CPF>", result.XmlContent, StringComparison.Ordinal);

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
            NFSeTransmissionFixtures.CreateRequest(
                includeOptionalValues: true,
                issWithholdingType: NFSeIssWithholdingType.WithheldByRecipient),
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
    public void SerializeSignedDps_ShouldFormatServiceAmountWithTwoDecimalPlaces()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(amount: 1.0m),
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate,
                ApplicationVersion = "NFSeNacionalSdk_Tests"
            });

        Assert.Contains("<vServ>1.00</vServ>", result.XmlContent, StringComparison.Ordinal);
        Assert.DoesNotContain("<vServ>1.0</vServ>", result.XmlContent, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectIssRateForSimpleNationalWithoutIssWithholding()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var exception = Assert.Throws<NFSeSerializationException>(() =>
            serializer.SerializeSignedDps(
                NFSeTransmissionFixtures.CreateRequest(
                    simplesNationalOption: NFSeSimplesNationalOption.MicroOrSmallBusiness,
                    simplifiedNationalTaxRegime: NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational,
                    issRate: 0.00m,
                    totalTaxIndicator: null,
                    simplesNationalTotalTaxRate: 2.00m),
                new EmitDpsSerializationContext
                {
                    Environment = NFSeEnvironment.ProductionRestricted,
                    SigningCertificate = certificate,
                    ApplicationVersion = "NFSeNacionalSdk_Tests"
                }));

        Assert.Contains("taxation.IssRate must be omitted", exception.Message, StringComparison.Ordinal);
        Assert.Contains("taxation.issRate to null", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldMapSimplesNationalTotalTaxRate()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(
                simplesNationalOption: NFSeSimplesNationalOption.MicroOrSmallBusiness,
                simplifiedNationalTaxRegime: NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational,
                totalTaxIndicator: null,
                simplesNationalTotalTaxRate: 2.00m),
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate,
                ApplicationVersion = "NFSeNacionalSdk_Tests"
            });

        Assert.Contains("<pTotTribSN>2.00</pTotTribSN>", result.XmlContent, StringComparison.Ordinal);
        Assert.DoesNotContain("<indTotTrib>", result.XmlContent, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectTotalTaxIndicatorForMicroOrSmallBusiness()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var exception = Assert.Throws<NFSeSerializationException>(() =>
            serializer.SerializeSignedDps(
                NFSeTransmissionFixtures.CreateRequest(
                    simplesNationalOption: NFSeSimplesNationalOption.MicroOrSmallBusiness,
                    simplifiedNationalTaxRegime: NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational),
                new EmitDpsSerializationContext
                {
                    Environment = NFSeEnvironment.ProductionRestricted,
                    SigningCertificate = certificate,
                    ApplicationVersion = "NFSeNacionalSdk_Tests"
                }));

        Assert.Contains("taxation.TotalTaxIndicator must be omitted", exception.Message, StringComparison.Ordinal);
        Assert.Contains("taxation.simplesNationalTotalTaxRate", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectXmlInvalidAgainstOfficialDpsSchema()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var exception = Assert.Throws<NFSeSerializationException>(() =>
            serializer.SerializeSignedDps(
                NFSeTransmissionFixtures.CreateRequest(
                    simplifiedNationalTaxRegime: (NFSeSimplifiedNationalTaxRegime)0),
                new EmitDpsSerializationContext
                {
                    Environment = NFSeEnvironment.ProductionRestricted,
                    SigningCertificate = certificate,
                    ApplicationVersion = "NFSeNacionalSdk_Tests"
                }));

        Assert.Contains("/DPS/infDPS/prest/regTrib/regApTribSN", exception.Message, StringComparison.Ordinal);
        Assert.Contains("0", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldGenerateXmlValidAgainstOfficialDpsSchema()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(
                includeOptionalValues: true,
                issWithholdingType: NFSeIssWithholdingType.WithheldByRecipient),
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

        var schemaDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "TestData",
            "Schemas",
            "LegacyV101_202602");
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

    [Fact]
    public void SerializeSignedDps_ShouldSupportOfficialAlphanumericCnpjInCurrentProfile()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var request = NFSeTransmissionFixtures.CreateRequest();
        request.Provider.TaxId = "00.000.000/E08G-12";

        var result = serializer.SerializeSignedDps(request, new EmitDpsSerializationContext
        {
            Environment = NFSeEnvironment.ProductionRestricted,
            LayoutProfile = NFSeLayoutProfile.RtcV101_202607,
            SigningCertificate = certificate
        });

        Assert.Contains("<CNPJ>00000000E08G12</CNPJ>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("DPS3550308200000000E08G12", result.DpsId, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectAlphanumericCnpjInLegacyProfile()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var request = NFSeTransmissionFixtures.CreateRequest();
        request.Provider.TaxId = "00.000.000/E08G-12";

        var exception = Assert.Throws<NFSeSerializationException>(() => serializer.SerializeSignedDps(
            request,
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                LayoutProfile = NFSeLayoutProfile.LegacyV101_202602,
                SigningCertificate = certificate
            }));

        Assert.Contains("incompatible with layout profile LegacyV101_202602", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectCnpjWithInvalidCheckDigits()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var request = NFSeTransmissionFixtures.CreateRequest();
        request.Provider.TaxId = "12.345.678/0001-96";

        var exception = Assert.Throws<NFSeSerializationException>(() => serializer.SerializeSignedDps(
            request,
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                LayoutProfile = NFSeLayoutProfile.RtcV101_202607,
                SigningCertificate = certificate
            }));

        Assert.Contains("check digit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectCpfWithInvalidCheckDigits()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var request = NFSeTransmissionFixtures.CreateRequest();
        request.Recipient!.TaxId = "529.982.247-24";

        var exception = Assert.Throws<NFSeSerializationException>(() => serializer.SerializeSignedDps(
            request,
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate
            }));

        Assert.Contains("check digit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(NFSeLayoutProfile.LegacyV101_202602)]
    [InlineData(NFSeLayoutProfile.RtcV101_202607)]
    public void SerializeSignedDps_ShouldMapFederalAndIbsCbsGroups(NFSeLayoutProfile layoutProfile)
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var request = NFSeTransmissionFixtures.CreateRequest();
        request.Taxation.Federal = new EmitDpsFederalTaxation
        {
            PisCofins = new EmitDpsPisCofinsTaxation
            {
                TaxStatusCode = "01",
                CalculationBase = 100m,
                PisRate = 0.65m,
                CofinsRate = 3m,
                PisAmount = 0.65m,
                CofinsAmount = 3m,
                WithholdingType = NFSePisCofinsWithholdingType.PisCofinsWithheld
            },
            IncomeTaxRetentionAmount = 1m
        };
        request.Taxation.IbsCbs = new EmitDpsIbsCbsTaxation
        {
            OperationIndicatorCode = "010101",
            DestinationIndicator = NFSeIbsCbsDestinationIndicator.No,
            TaxStatusCode = "000",
            TaxClassificationCode = "000001"
        };

        var result = serializer.SerializeSignedDps(request, new EmitDpsSerializationContext
        {
            Environment = NFSeEnvironment.ProductionRestricted,
            LayoutProfile = layoutProfile,
            SigningCertificate = certificate
        });

        Assert.Contains("<tribFed>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<tpRetPisCofins>1</tpRetPisCofins>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<IBSCBS>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<cIndOp>010101</cIndOp>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains("<cClassTrib>000001</cClassTrib>", result.XmlContent, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectInvalidIbsCbsClassification()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var request = NFSeTransmissionFixtures.CreateRequest();
        request.Taxation.IbsCbs = new EmitDpsIbsCbsTaxation
        {
            OperationIndicatorCode = "010101",
            DestinationIndicator = NFSeIbsCbsDestinationIndicator.No,
            TaxStatusCode = "000",
            TaxClassificationCode = "INVALID"
        };

        var exception = Assert.Throws<NFSeSerializationException>(() => serializer.SerializeSignedDps(
            request,
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                LayoutProfile = NFSeLayoutProfile.RtcV101_202607,
                SigningCertificate = certificate
            }));

        Assert.Contains("TaxClassificationCode", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SerializeSignedDps_ShouldRejectImplicitFiscalRounding()
    {
        var serializer = new NFSeXmlSerializer();
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var exception = Assert.Throws<NFSeSerializationException>(() => serializer.SerializeSignedDps(
            NFSeTransmissionFixtures.CreateRequest(amount: 1.005m),
            new EmitDpsSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                SigningCertificate = certificate
            }));

        Assert.Contains("does not round fiscal values implicitly", exception.Message, StringComparison.Ordinal);
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
