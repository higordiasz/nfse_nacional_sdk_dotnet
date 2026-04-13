using System.IO.Compression;
using System.Text;

namespace NFSeNacionalSdk.Tests.TestData;

internal static class NFSeLookupXmlFixtures
{
    public const string AccessKey = "12345678901234567890123456789012345678901234567890";

    public const string Success = """
        <NFSe xmlns="http://www.sped.fazenda.gov.br/nfse" versao="1.01">
          <infNFSe Id="NFS12345678901234567890123456789012345678901234567890">
            <xLocEmi>Sao Paulo</xLocEmi>
            <xLocPrestacao>Sao Paulo</xLocPrestacao>
            <nNFSe>2024000000001</nNFSe>
            <cLocIncid>3550308</cLocIncid>
            <xLocIncid>Sao Paulo</xLocIncid>
            <xTribNac>Consultoria em tecnologia da informacao</xTribNac>
            <verAplic>1.0.0</verAplic>
            <ambGer>2</ambGer>
            <tpEmis>1</tpEmis>
            <cStat>100</cStat>
            <dhProc>2026-04-13T15:30:00-03:00</dhProc>
            <nDFSe>123456789012345</nDFSe>
            <emit>
              <CNPJ>12345678000199</CNPJ>
              <IM>998877</IM>
              <xNome>Prestador Exemplo LTDA</xNome>
              <email>contato@prestador.example</email>
            </emit>
            <DPS Id="DPS12345678901234567890123456789012345678901234567890">
              <dhEmi>2026-04-13T15:25:00-03:00</dhEmi>
              <prest>
                <CNPJ>12345678000199</CNPJ>
                <IM>998877</IM>
                <xNome>Prestador Exemplo LTDA</xNome>
                <email>contato@prestador.example</email>
              </prest>
              <toma>
                <CPF>12345678901</CPF>
                <xNome>Tomador Exemplo SA</xNome>
                <email>financeiro@tomador.example</email>
              </toma>
              <serv>
                <cServ>
                  <cTribNac>140101</cTribNac>
                </cServ>
                <xDescServ>Consultoria especializada</xDescServ>
              </serv>
              <valores>
                <vServPrest>
                  <vServ>1500.75</vServ>
                </vServPrest>
              </valores>
            </DPS>
          </infNFSe>
        </NFSe>
        """;

    public const string BusinessError = """
        <ListaMensagemRetorno xmlns="http://www.sped.fazenda.gov.br/nfse">
          <MensagemRetorno>
            <Codigo>E160</Codigo>
            <Mensagem>NFS-e nao encontrada para a chave de acesso informada.</Mensagem>
          </MensagemRetorno>
          <MensagemRetorno>
            <Codigo>E161</Codigo>
            <Descricao>Verifique se a chave pertence ao ambiente consultado.</Descricao>
          </MensagemRetorno>
        </ListaMensagemRetorno>
        """;

    public static string SuccessApiResponseJson => $$"""
        {
          "tipoAmbiente": 2,
          "versaoAplicativo": "SefinNacional_1.6.0",
          "dataHoraProcessamento": "2026-04-13T16:56:04.1505667-03:00",
          "chaveAcesso": "{{AccessKey}}",
          "nfseXmlGZipB64": "{{ToGZipBase64(Success)}}"
        }
        """;

    public static string NotFoundApiResponseJson => """
        {
          "tipoAmbiente": 2,
          "versaoAplicativo": "SefinNacional_1.6.0",
          "dataHoraProcessamento": "2026-04-13T16:56:04.1505667-03:00",
          "erro": {
            "codigo": "E2401",
            "descricao": "Chave de acesso não encontrada."
          }
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
