# NFSe Nacional SDK for .NET

SDK .NET para integracao com o ambiente nacional da NFS-e, incluindo consulta de NFS-e, emissao sincronica de DPS, consulta de DPS, parametrizacao municipal, assinatura XML, validacao XSD e parse estruturado do XML retornado.

> Status: `0.1.0-preview.1`
>
> A emissao e a consulta ja foram validadas em Producao Restrita. Use producao com cautela e valide as regras do municipio/prestador antes de transmitir documentos reais.

## Features

- [x] Ambientes de Producao Restrita e Producao
- [x] Cliente HTTP com certificado A1
- [x] Consulta de NFS-e por chave de acesso
- [x] Retorno do XML bruto e de um `NFSeDocument` estruturado
- [x] Emissao sincronica de DPS e geracao de NFS-e
- [x] Assinatura XML da DPS
- [x] Validacao XML contra schemas v1.01
- [x] Consulta de DPS por id
- [x] Verificacao de DPS por `HEAD`
- [x] Consulta de convenio municipal
- [x] Consulta de aliquota municipal por servico
- [ ] Eventos de NFS-e
- [ ] Normalizacao ampliada de erros
- [ ] Extensoes oficiais para dependency injection

## Target Frameworks

- `net8.0`
- `net10.0`

## Pacotes

O pacote principal e:

```bash
dotnet add package NFSeNacionalSdk --prerelease
```

Os projetos internos tambem sao empacotados, pois o pacote principal depende deles:

- `NFSeNacionalSdk.Core`
- `NFSeNacionalSdk.Contracts`
- `NFSeNacionalSdk.Serialization.Xml`
- `NFSeNacionalSdk.Transport.Http`

Ao publicar uma versao, publique todos os pacotes gerados com a mesma versao.

## Uso Basico

### Criar o cliente

```csharp
using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

var certificate = new X509Certificate2(
    "certificado.pfx",
    "senha-do-certificado",
    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

using var client = new NFSeClient(
    new NFSeSdkOptions
    {
        Environment = NFSeEnvironment.ProductionRestricted
    },
    certificate);
```

### Consultar NFS-e por chave

```csharp
using NFSeNacionalSdk.Contracts.Requests;

var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
{
    AccessKey = "<CHAVE_ACESSO_NFSE>"
});

if (result.Success && result.Document is not null)
{
    var rawXml = result.RawXml;
    var document = result.Document;

    Console.WriteLine(document.Number);
    Console.WriteLine(document.IssuedAt);
    Console.WriteLine(document.Issuer?.Name);
    Console.WriteLine(document.Service?.ServiceCode);
    Console.WriteLine(document.Values?.NetAmount);
    Console.WriteLine(document.Taxation?.Municipal?.IssTaxationType);
}
```

`RawXml` preserva o XML original retornado pelo ambiente nacional. `Document` contem os dados principais ja estruturados para uso no sistema consumidor.

### Emitir DPS e gerar NFS-e

```csharp
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;

var result = await client.EmitDpsAsync(new EmitDpsRequest
{
    Series = "1",
    Number = "1",
    CompetenceDate = DateOnly.FromDateTime(DateTime.Today),
    IssuedAt = DateTimeOffset.Now,
    MunicipalityCode = "3201506",
    Provider = new EmitDpsProvider
    {
        TaxId = "<CNPJ_PRESTADOR>",
        SimplesNationalOption = NFSeSimplesNationalOption.MicroOrSmallBusiness,
        SimplifiedNationalTaxRegime = NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational,
        SpecialTaxRegime = NFSeSpecialTaxRegime.None
    },
    Recipient = new EmitDpsRecipient
    {
        TaxId = "<CNPJ_TOMADOR>",
        Name = "TOMADOR EXEMPLO LTDA"
    },
    Service = new EmitDpsService
    {
        NationalTaxationCode = "010201",
        Description = "PROGRAMACAO DE SISTEMAS",
        Amount = 1.00m
    },
    Taxation = new EmitDpsTaxation
    {
        IssTaxationType = NFSeIssTaxationType.TaxableOperation,
        IssWithholdingType = NFSeIssWithholdingType.NotWithheld,
        IssRate = null,
        TotalTaxIndicator = null,
        SimplesNationalTotalTaxRate = 2.00m
    }
});

Console.WriteLine(result.Success);
Console.WriteLine(result.AccessKey);
Console.WriteLine(result.Document?.Number);
Console.WriteLine(result.SubmittedDpsXml);
Console.WriteLine(result.RawXml);
```

Para ME/EPP com `opSimpNac = 3`, `regApTribSN = 1` e ISSQN nao retido, informe `IssRate = null`. Para esse mesmo caso, use `TotalTaxIndicator = null` e informe `SimplesNationalTotalTaxRate`.

## Sample Console

O projeto `samples/NFSeNacionalSdk.Samples.Console` permite testar os fluxos principais por menu.

```powershell
$env:NFSE_ENVIRONMENT="ProductionRestricted"
$env:NFSE_CERTIFICATE_PATH="C:\caminho\certificado.pfx"
$env:NFSE_CERTIFICATE_PASSWORD="senha-do-certificado"

dotnet run --project "samples\NFSeNacionalSdk.Samples.Console\NFSeNacionalSdk.Samples.Console.csproj" --configuration Release
```

O menu possui opcao para emissao por JSON. O template fica em:

```text
samples/NFSeNacionalSdk.Samples.Console/emit-dps.request.template.json
```

## Parametrizacao Municipal

Antes de emitir, consulte:

- convenio municipal: `GetMunicipalConventionAsync`
- aliquota por municipio/servico/competencia: `GetMunicipalServiceParametersAsync`

Essas consultas ajudam a identificar casos em que o municipio nao esta ativo no ambiente nacional ou em que a aliquota deve ser omitida/informada conforme parametrizacao.

## Empacotamento

Gerar pacotes locais:

```bash
dotnet clean
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --no-build
dotnet pack --configuration Release --no-build --output artifacts/packages
```

Publicar no NuGet:

```bash
dotnet nuget push "artifacts/packages/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key <NUGET_API_KEY>
```

Para uma versao estavel, sobrescreva a versao no pack:

```bash
dotnet pack --configuration Release --no-build --output artifacts/packages -p:Version=0.1.0
```

## Estrutura

```text
src/
  NFSeNacionalSdk.Core
  NFSeNacionalSdk.Contracts
  NFSeNacionalSdk.Serialization.Xml
  NFSeNacionalSdk.Transport.Http
  NFSeNacionalSdk

tests/
  NFSeNacionalSdk.Tests

samples/
  NFSeNacionalSdk.Samples.Console
```

## Referencias Tecnicas

- Documentacao tecnica atual da NFS-e: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual
- APIs de Producao Restrita e Producao: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/apis-prod-restrita-e-producao
- Schemas usados nos testes: `NFSe-ESQUEMAS_XSD-v1.01-20260209`

## Contribuicao

Leia [CONTRIBUTING.md](./CONTRIBUTING.md) antes de abrir issues ou pull requests.

## Commits

Use Conventional Commits:

- `feat: add nfse event registration`
- `fix: parse nfse taxation values`
- `docs: update release instructions`
- `build: add nuget package metadata`
- `test: cover municipal parameter lookup`

## Licenca

MIT.
