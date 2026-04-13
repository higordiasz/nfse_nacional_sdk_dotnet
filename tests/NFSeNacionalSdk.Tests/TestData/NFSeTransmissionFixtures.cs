using System.IO.Compression;
using System.Text;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Tests.TestData;

internal static class NFSeTransmissionFixtures
{
    public const string MunicipalityCode = "3550308";
    public const string ProviderTaxId = "12345678000199";
    public const string Series = "70000";
    public const string Number = "1";
    public const string ExpectedDpsId = "DPS355030821234567800019970000000000000000001";

    public static EmitDpsRequest CreateRequest()
    {
        return new EmitDpsRequest
        {
            Series = Series,
            Number = Number,
            CompetenceDate = new DateOnly(2026, 04, 13),
            IssuedAt = new DateTimeOffset(2026, 04, 13, 15, 25, 00, TimeSpan.FromHours(-3)),
            MunicipalityCode = MunicipalityCode,
            Provider = new EmitDpsProvider
            {
                TaxId = "12.345.678/0001-99",
                MunicipalRegistration = "998877",
                Name = "Prestador Exemplo LTDA",
                Phone = "(11) 99999-0000",
                Email = "contato@prestador.example",
                SimplesNationalOption = NFSeSimplesNationalOption.MicroOrSmallBusiness,
                SimplifiedNationalTaxRegime = NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational,
                SpecialTaxRegime = NFSeSpecialTaxRegime.None
            },
            Recipient = new EmitDpsRecipient
            {
                TaxId = "123.456.789-01",
                Name = "Tomador Exemplo SA",
                Email = "financeiro@tomador.example",
                Address = new EmitDpsAddress
                {
                    MunicipalityCode = MunicipalityCode,
                    ZipCode = "01001000",
                    Street = "Rua Exemplo",
                    Number = "100",
                    Neighborhood = "Centro",
                    Complement = "Sala 101"
                }
            },
            Service = new EmitDpsService
            {
                ServiceLocationMunicipalityCode = MunicipalityCode,
                NationalTaxationCode = "140101",
                Description = "Consultoria especializada",
                NationalClassificationCode = "111032200",
                Amount = 1500.75m
            },
            Taxation = new EmitDpsTaxation
            {
                IssTaxationType = NFSeIssTaxationType.TaxableOperation,
                IssWithholdingType = NFSeIssWithholdingType.NotWithheld,
                TotalTaxIndicator = NFSeTotalTaxIndicator.NotInformed
            }
        };
    }

    public static string SuccessApiResponseJson => $$"""
        {
          "tipoAmbiente": 2,
          "versaoAplicativo": "SefinNacional_1.6.0",
          "dataHoraProcessamento": "2026-04-13T16:56:04.1505667-03:00",
          "idDps": "{{ExpectedDpsId}}",
          "chaveAcesso": "{{NFSeLookupXmlFixtures.AccessKey}}",
          "nfseXmlGZipB64": "{{ToGZipBase64(NFSeLookupXmlFixtures.Success)}}",
          "alertas": [
            {
              "codigo": "A100",
              "descricao": "Emitida com alerta de homologacao."
            }
          ]
        }
        """;

    public static string ErrorApiResponseJson => $$"""
        {
          "tipoAmbiente": 2,
          "versaoAplicativo": "SefinNacional_1.6.0",
          "dataHoraProcessamento": "2026-04-13T16:56:04.1505667-03:00",
          "idDPS": "{{ExpectedDpsId}}",
          "erros": [
            {
              "codigo": "E3001",
              "descricao": "DPS invalido."
            },
            {
              "mensagem": "Revise os dados do tomador."
            }
          ]
        }
        """;

    public static string DecodeGZipBase64(string content)
    {
        var bytes = Convert.FromBase64String(content);
        using var input = new MemoryStream(bytes);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        gzip.CopyTo(output);

        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static string ToGZipBase64(string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        using var output = new MemoryStream();

        using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(contentBytes, 0, contentBytes.Length);
        }

        return Convert.ToBase64String(output.ToArray());
    }
}
