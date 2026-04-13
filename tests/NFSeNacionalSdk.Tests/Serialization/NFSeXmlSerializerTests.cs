using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Serialization.Xml;
using NFSeNacionalSdk.Tests.TestData;

namespace NFSeNacionalSdk.Tests.Serialization;

public sealed class NFSeXmlSerializerTests
{
    [Fact]
    public void DeserializeLookupResponse_ShouldMapSuccessEnvelopeIntoTypedDocument()
    {
        var serializer = new NFSeXmlSerializer();

        var result = serializer.DeserializeLookupResponse(NFSeLookupXmlFixtures.Success);

        Assert.True(result.Success);
        Assert.NotNull(result.Document);
        Assert.Empty(result.Messages);
        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, result.Document!.AccessKey);
        Assert.Equal("2024000000001", result.Document.Number);
        Assert.Null(result.Document.VerificationCode);
        Assert.Equal(new DateTimeOffset(2026, 04, 13, 15, 30, 00, TimeSpan.FromHours(-3)), result.Document.IssuedAt);
        Assert.Equal("Prestador Exemplo LTDA", result.Document.Issuer?.Name);
        Assert.Equal("12345678000199", result.Document.Issuer?.TaxId);
        Assert.Equal("Tomador Exemplo SA", result.Document.Recipient?.Name);
        Assert.Equal("financeiro@tomador.example", result.Document.Recipient?.Email);
        Assert.Equal("140101", result.Document.Service?.ServiceCode);
        Assert.Equal(1500.75m, result.Document.Service?.ServiceAmount);
    }

    [Fact]
    public void DeserializeLookupResponse_ShouldMapBusinessErrorEnvelopeIntoMessages()
    {
        var serializer = new NFSeXmlSerializer();

        var result = serializer.DeserializeLookupResponse(NFSeLookupXmlFixtures.BusinessError);

        Assert.False(result.Success);
        Assert.Null(result.Document);
        Assert.Collection(
            result.Messages,
            message =>
            {
                Assert.Equal("E160", message.Code);
                Assert.Equal("NFS-e nao encontrada para a chave de acesso informada.", message.Description);
            },
            message =>
            {
                Assert.Equal("E161", message.Code);
                Assert.Equal("Verifique se a chave pertence ao ambiente consultado.", message.Description);
            });
    }

    [Fact]
    public void DeserializeDocument_ShouldMapTheConcreteSuccessEnvelope()
    {
        var serializer = new NFSeXmlSerializer();

        var document = serializer.Deserialize<NFSeDocument>(NFSeLookupXmlFixtures.Success);

        Assert.Equal(NFSeLookupXmlFixtures.AccessKey, document.AccessKey);
        Assert.Equal("2024000000001", document.Number);
    }
}
