using System.Xml.Linq;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Tests.TestData;

namespace NFSeNacionalSdk.Tests.Serialization;

public sealed class NFSeReceivedXmlValidatorTests
{
    [Theory]
    [InlineData(NFSeLayoutProfile.LegacyV101_202602)]
    [InlineData(NFSeLayoutProfile.RtcV101_202607)]
    public void ValidateEvent_ShouldAcceptValidEventForEachProfile(NFSeLayoutProfile profile)
    {
        new NFSeReceivedXmlValidator().ValidateEvent(CreateSchemaValidEventXml(profile), profile);
    }

    [Theory]
    [InlineData(NFSeLayoutProfile.LegacyV101_202602)]
    [InlineData(NFSeLayoutProfile.RtcV101_202607)]
    public void ValidateEvent_ShouldRejectInvalidEventForEachProfile(NFSeLayoutProfile profile)
    {
        var invalidXml = CreateSchemaValidEventXml(profile)
            .Replace("<ambGer>2</ambGer>", "<ambGer>9</ambGer>");

        Assert.Throws<NFSeSerializationException>(() =>
            new NFSeReceivedXmlValidator().ValidateEvent(invalidXml, profile));
    }

    [Fact]
    public void ValidateEvent_ShouldRejectDtd()
    {
        const string xml = "<!DOCTYPE evento [<!ENTITY x 'unsafe'>]><evento xmlns=\"http://www.sped.fazenda.gov.br/nfse\">&x;</evento>";

        Assert.Throws<NFSeSerializationException>(() =>
            new NFSeReceivedXmlValidator().ValidateEvent(xml));
    }

    private static string CreateSchemaValidEventXml(NFSeLayoutProfile profile)
    {
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();
        var signedRequest = new NFSeXmlSerializer().SerializeSignedCancellation(
            NFSeEventFixtures.CreateCancellationRequest(),
            new CancelNfseSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                LayoutProfile = profile,
                SigningCertificate = certificate
            });

        var response = XDocument.Parse(NFSeEventFixtures.SuccessEventXml, LoadOptions.PreserveWhitespace);
        var request = XDocument.Parse(signedRequest.XmlContent, LoadOptions.PreserveWhitespace);
        var signature = request.Descendants(
            XName.Get("Signature", "http://www.w3.org/2000/09/xmldsig#")).Single();
        response.Root!.Add(new XElement(signature));

        return response.ToString(SaveOptions.DisableFormatting);
    }
}
