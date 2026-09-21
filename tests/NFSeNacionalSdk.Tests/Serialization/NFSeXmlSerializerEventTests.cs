using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Tests.TestData;

namespace NFSeNacionalSdk.Tests.Serialization;

public sealed class NFSeXmlSerializerEventTests
{
    [Fact]
    public void SerializeSignedCancellation_ShouldSupportAlphanumericCnpjAuthorInCurrentProfile()
    {
        var request = NFSeEventFixtures.CreateCancellationRequest();
        request.AuthorTaxId = "00.000.000/E08G-12";
        using var certificate = TestCertificateFactory.CreateSelfSignedCertificate();

        var result = new NFSeXmlSerializer().SerializeSignedCancellation(
            request,
            new CancelNfseSerializationContext
            {
                Environment = NFSeEnvironment.ProductionRestricted,
                LayoutProfile = NFSeLayoutProfile.RtcV101_202607,
                SigningCertificate = certificate
            });

        Assert.Contains("<CNPJAutor>00000000E08G12</CNPJAutor>", result.XmlContent, StringComparison.Ordinal);
        Assert.Contains(request.AccessKey, result.EventRequestId, StringComparison.Ordinal);
    }
}
