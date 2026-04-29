using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Core.Enums;
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
        Assert.Equal("123456789012345", result.Document.DfseNumber);
        Assert.Null(result.Document.VerificationCode);
        Assert.Equal(new DateTimeOffset(2026, 04, 13, 15, 30, 00, TimeSpan.FromHours(-3)), result.Document.IssuedAt);
        Assert.Equal(new DateTimeOffset(2026, 04, 13, 15, 25, 00, TimeSpan.FromHours(-3)), result.Document.DpsIssuedAt);
        Assert.Equal(new DateOnly(2026, 04, 13), result.Document.CompetenceDate);
        Assert.Equal("100", result.Document.StatusCode);
        Assert.Equal("1.0.0", result.Document.ApplicationVersion);
        Assert.Equal("Sao Paulo", result.Document.IssuingMunicipalityName);
        Assert.Equal("Sao Paulo", result.Document.ServiceLocationMunicipalityName);
        Assert.Equal("3550308", result.Document.IncidenceMunicipalityCode);
        Assert.Equal("Sao Paulo", result.Document.IncidenceMunicipalityName);
        Assert.Equal("Consultoria em tecnologia da informacao", result.Document.NationalTaxationDescription);
        Assert.Equal("DPS12345678901234567890123456789012345678901234567890", result.Document.DpsId);
        Assert.Equal("70000", result.Document.DpsSeries);
        Assert.Equal("1", result.Document.DpsNumber);
        Assert.Equal(1500.75m, result.Document.NetAmount);
        Assert.Equal("Prestador Exemplo LTDA", result.Document.Issuer?.Name);
        Assert.Equal("12345678000199", result.Document.Issuer?.TaxId);
        Assert.Equal("998877", result.Document.Issuer?.MunicipalRegistration);
        Assert.Equal("11999990000", result.Document.Issuer?.Phone);
        Assert.Equal("contato@prestador.example", result.Document.Issuer?.Email);
        Assert.Equal("Rua do Prestador", result.Document.Issuer?.Address?.Street);
        Assert.Equal("100", result.Document.Issuer?.Address?.Number);
        Assert.Equal("Sala 10", result.Document.Issuer?.Address?.Complement);
        Assert.Equal("Centro", result.Document.Issuer?.Address?.Neighborhood);
        Assert.Equal("3550308", result.Document.Issuer?.Address?.MunicipalityCode);
        Assert.Equal("SP", result.Document.Issuer?.Address?.State);
        Assert.Equal("01001000", result.Document.Issuer?.Address?.ZipCode);
        Assert.Equal("3", result.Document.Issuer?.TaxRegime?.SimplesNationalOptionCode);
        Assert.Equal(NFSeSimplesNationalOption.MicroOrSmallBusiness, result.Document.Issuer?.TaxRegime?.SimplesNationalOption);
        Assert.Equal("1", result.Document.Issuer?.TaxRegime?.SimplifiedNationalTaxRegimeCode);
        Assert.Equal(
            NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational,
            result.Document.Issuer?.TaxRegime?.SimplifiedNationalTaxRegime);
        Assert.Equal("0", result.Document.Issuer?.TaxRegime?.SpecialTaxRegimeCode);
        Assert.Equal(NFSeSpecialTaxRegime.None, result.Document.Issuer?.TaxRegime?.SpecialTaxRegime);
        Assert.Equal("Tomador Exemplo SA", result.Document.Recipient?.Name);
        Assert.Equal("12345678901", result.Document.Recipient?.TaxId);
        Assert.Equal("financeiro@tomador.example", result.Document.Recipient?.Email);
        Assert.Equal("Consultoria especializada", result.Document.Service?.Description);
        Assert.Equal("140101", result.Document.Service?.ServiceCode);
        Assert.Equal("001", result.Document.Service?.MunicipalServiceCode);
        Assert.Equal("111032200", result.Document.Service?.NationalClassificationCode);
        Assert.Equal("CONS-001", result.Document.Service?.InternalCode);
        Assert.Equal("Consultoria em tecnologia da informacao", result.Document.Service?.NationalTaxationDescription);
        Assert.Equal("3550308", result.Document.Service?.LocationMunicipalityCode);
        Assert.Equal("Sao Paulo", result.Document.Service?.LocationMunicipalityName);
        Assert.Equal(1500.75m, result.Document.Service?.ServiceAmount);
        Assert.Equal(1500.75m, result.Document.Values?.ServiceAmount);
        Assert.Equal(100.00m, result.Document.Values?.AmountReceivedByIntermediary);
        Assert.Equal(10.00m, result.Document.Values?.UnconditionalDiscountAmount);
        Assert.Equal(5.00m, result.Document.Values?.ConditionalDiscountAmount);
        Assert.Equal(1500.75m, result.Document.Values?.NetAmount);
        Assert.Equal("1", result.Document.Taxation?.Municipal?.IssTaxationTypeCode);
        Assert.Equal(NFSeIssTaxationType.TaxableOperation, result.Document.Taxation?.Municipal?.IssTaxationType);
        Assert.Equal("2", result.Document.Taxation?.Municipal?.IssWithholdingTypeCode);
        Assert.Equal(NFSeIssWithholdingType.WithheldByRecipient, result.Document.Taxation?.Municipal?.IssWithholdingType);
        Assert.Equal(3.00m, result.Document.Taxation?.Municipal?.IssRate);
        Assert.Equal("01", result.Document.Taxation?.Federal?.PisCofins?.TaxStatusCode);
        Assert.Equal(1500.75m, result.Document.Taxation?.Federal?.PisCofins?.CalculationBase);
        Assert.Equal(0.65m, result.Document.Taxation?.Federal?.PisCofins?.PisRate);
        Assert.Equal(3.00m, result.Document.Taxation?.Federal?.PisCofins?.CofinsRate);
        Assert.Equal(9.75m, result.Document.Taxation?.Federal?.PisCofins?.PisAmount);
        Assert.Equal(45.02m, result.Document.Taxation?.Federal?.PisCofins?.CofinsAmount);
        Assert.Equal("1", result.Document.Taxation?.Federal?.PisCofins?.WithholdingTypeCode);
        Assert.Equal(1.00m, result.Document.Taxation?.Federal?.SocialSecurityRetentionAmount);
        Assert.Equal(2.00m, result.Document.Taxation?.Federal?.IncomeTaxRetentionAmount);
        Assert.Equal(3.00m, result.Document.Taxation?.Federal?.SocialContributionRetentionAmount);
        Assert.Equal(2.00m, result.Document.Taxation?.Total?.SimplesNationalRate);
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
        Assert.Equal("Tomador Exemplo SA", document.Recipient?.Name);
        Assert.Equal("Consultoria especializada", document.Service?.Description);
    }
}
