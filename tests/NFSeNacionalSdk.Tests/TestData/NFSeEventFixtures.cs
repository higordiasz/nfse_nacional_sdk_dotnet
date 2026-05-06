using System.IO.Compression;
using System.Text;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Tests.TestData;

internal static class NFSeEventFixtures
{
    public const string AccessKey = NFSeLookupXmlFixtures.AccessKey;
    public const string AuthorTaxId = "12345678000199";
    public const string EventTypeCode = "101101";
    public const string EventRequestId = $"PRE{AccessKey}{EventTypeCode}";
    public const string EventId = $"EVT{AccessKey}{EventTypeCode}001";

    public static CancelNfseRequest CreateCancellationRequest()
    {
        return new CancelNfseRequest
        {
            AccessKey = AccessKey,
            AuthorTaxId = AuthorTaxId,
            EventAt = new DateTimeOffset(2026, 04, 13, 17, 10, 00, TimeSpan.FromHours(-3)),
            ReasonCode = NFSeCancellationReasonCode.ServiceNotProvided,
            Reason = "Servico nao prestado ao tomador conforme acordado."
        };
    }

    public static string SuccessEventXml => $$"""
        <?xml version="1.0" encoding="utf-8"?>
        <evento versao="1.01" xmlns="http://www.sped.fazenda.gov.br/nfse">
          <infEvento Id="{{EventId}}">
            <verAplic>SefinNacional_1.6.0</verAplic>
            <ambGer>2</ambGer>
            <nSeqEvento>1</nSeqEvento>
            <dhProc>2026-04-13T17:10:30-03:00</dhProc>
            <nDFSe>1698</nDFSe>
            <pedRegEvento versao="1.01">
              <infPedReg Id="{{EventRequestId}}">
                <tpAmb>2</tpAmb>
                <verAplic>NFSeSdk_0.2.0</verAplic>
                <dhEvento>2026-04-13T17:10:00-03:00</dhEvento>
                <CNPJAutor>{{AuthorTaxId}}</CNPJAutor>
                <chNFSe>{{AccessKey}}</chNFSe>
                <e101101>
                  <xDesc>Cancelamento de NFS-e</xDesc>
                  <cMotivo>2</cMotivo>
                  <xMotivo>Servico nao prestado ao tomador conforme acordado.</xMotivo>
                </e101101>
              </infPedReg>
            </pedRegEvento>
          </infEvento>
        </evento>
        """;

    public static string SuccessApiResponseJson => $$"""
        {
          "tipoAmbiente": 2,
          "versaoAplicativo": "SefinNacional_1.6.0",
          "dataHoraProcessamento": "2026-04-13T17:10:30-03:00",
          "chaveAcesso": "{{AccessKey}}",
          "eventoXmlGZipB64": "{{ToGZipBase64(SuccessEventXml)}}"
        }
        """;

    public static string ErrorApiResponseJson => """
        {
          "tipoAmbiente": 2,
          "versaoAplicativo": "SefinNacional_1.6.0",
          "dataHoraProcessamento": "2026-04-13T17:10:30-03:00",
          "erros": [
            {
              "codigo": "E6101",
              "descricao": "Evento de cancelamento invalido."
            }
          ]
        }
        """;

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
