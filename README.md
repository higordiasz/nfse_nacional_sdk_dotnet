# NFSe Nacional SDK for .NET

SDK .NET para integracao com o ambiente nacional da NFS-e, incluindo consulta de NFS-e, emissao sincronica de DPS, consulta de DPS, parametrizacao municipal, assinatura XML, validacao XSD e parse estruturado do XML retornado.

> Status: `0.2.0-preview.1`
>
> Baseline tecnico revisado em setembro de 2026. O XML oficial continua em `1.01`; o perfil atual combina NT 004, `tpRetPisCofins` da NT 007, grupos RTC/IBS/CBS e CNPJ alfanumerico. A NT 009 nao esta implementada porque a pagina oficial informa que ela ainda nao foi implantada e nao possui cronograma.

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
- [x] Evento de cancelamento de NFS-e
- [x] Normalizacao base de respostas com `Success`, `StatusCode`, `Messages`, `RawXml` e `RawJson`
- [x] Factory oficial para uso direto da DLL
- [x] Extensoes oficiais para dependency injection
- [x] Perfis de leiaute imutaveis, com bundle XSD oficial por revisao
- [x] CNPJ numerico e alfanumerico com validacao de digitos verificadores
- [x] Tributacao federal, PIS/COFINS e grupos RTC/IBS/CBS da revisao de julho de 2026
- [x] Consulta GET de eventos, por chave, tipo e sequencia
- [x] Parametrizacao municipal tipada, preservando `RawJson`
- [x] Certificados separados para mTLS e assinatura XML
- [x] Validacao XSD de XML recebido (sem confundir com verificacao de confianca da assinatura)

## Versionamento de Layouts / Notas Tecnicas

`NFSeSdkOptions.LayoutProfile` seleciona uma revisao imutavel. Nao use o numero do XML como proxy para a Nota Tecnica: os dois perfis abaixo usam `versao="1.01"`.

- `NFSeLayoutProfile.RtcV101_202607` (padrao): bundle de Producao Restrita de 27/07/2026, com RTC/IBS/CBS e CNPJ alfanumerico. O historico oficial informa ativacao do tratamento de CNPJ alfanumerico em Producao em 10/08/2026.
- `NFSeLayoutProfile.LegacyV101_202602`: bundle de Producao de 09/02/2026 (NT 004/IBS-CBS, CNPJ numerico), para integracoes que ainda precisam do contrato anterior a revisao alfanumerica.

`NFSeLayoutDefaults.Current` e o alias dinamico usado quando `LayoutProfile` nao e informado e pode mudar em uma futura versao do SDK. Para comportamento reprodutivel entre upgrades, fixe um dos valores imutaveis de `NFSeLayoutProfile`; a adicao futura da NT 009 nao removera os perfis anteriores.

A pagina de Producao atualizada em agosto ainda aponta para o ZIP de fevereiro, enquanto o historico de implantacao confirma a ativacao posterior. Por isso a divergencia e explicita, auditavel e selecionavel; nenhum schema e baixado em runtime. O manifesto fica em `src/NFSeNacionalSdk.Serialization.Xml/Schemas/README.md`.

Limitacao conhecida do bundle de 27/07/2026: `TSCNPJ` e `TSIdPedRegEvt` admitem caracteres do CNPJ alfanumerico, mas o pattern publicado para `TSChaveNFSe` libera letras em posicoes diferentes das usadas pelo ID do evento. Assim, algumas chaves contendo CNPJ alfanumerico podem falhar na validacao de eventos. O SDK aceita `CNPJAutor` alfanumerico, mas nao relaxa nem altera o XSD oficial; uma correcao depende de novo bundle oficial.

## Target Frameworks

- `netstandard2.0`
- `net8.0`
- `net10.0`

O target `netstandard2.0` permite consumir o SDK em projetos .NET Framework, incluindo .NET Framework 4.6.2. Quando for possivel escolher, .NET Framework 4.7.2 ou superior tende a reduzir problemas de dependencias transitivas e binding redirects, mas o pacote tambem publica assets compativeis com 4.6.2.

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

### Criar o cliente com factory

```csharp
using NFSeNacionalSdk;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

using var client = NFSeClientFactory.Create(options =>
{
    options.Environment = NFSeEnvironment.ProductionRestricted;
    options.ApplicationName = "MeuERP";
    options.ApplicationVersion = "4.2.1";
    options.CertificateFile = new NFSeCertificateFileOptions
    {
        Path = "certificado.pfx",
        Password = "senha-do-certificado"
    };
});
```

O exemplo acima usa o default vigente. Para fixar explicitamente o contrato fiscal:

```csharp
using var client = NFSeClientFactory.Create(options =>
{
    options.Environment = NFSeEnvironment.Production;
    options.LayoutProfile = NFSeLayoutProfile.RtcV101_202607;
});
```

`ApplicationName` e `ApplicationVersion` formam o `verAplic` (maximo oficial de 20 caracteres). Se omitidos, o SDK mantem o identificador historico baseado na propria versao.

Quando mTLS e assinatura usam certificados diferentes, configure-os separadamente:

```csharp
options.CertificateFile = new NFSeCertificateFileOptions { Path = "mtls.pfx", Password = "..." };
options.SigningCertificateFile = new NFSeCertificateFileOptions { Path = "assinatura.pfx", Password = "..." };
```

`ClientCertificate`/`CertificateFile` continuam funcionando sozinhos: na ausencia de certificado de assinatura dedicado, o mesmo certificado e reutilizado. Arquivos PFX usam `EphemeralKeySet` por padrao para nao persistir chaves no host.

Tambem e possivel informar um `X509Certificate2` ja carregado:

```csharp
using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

using var certificate = NFSeCertificateLoader.LoadFromPfxFile(
    "certificado.pfx",
    "senha-do-certificado",
    X509KeyStorageFlags.EphemeralKeySet);

using var client = NFSeClientFactory.Create(
    new NFSeSdkOptions
    {
        Environment = NFSeEnvironment.ProductionRestricted
    },
    certificate);
```

### Usar com dependency injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using NFSeNacionalSdk;
using NFSeNacionalSdk.Contracts.Clients;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

var services = new ServiceCollection();

services.AddNFSeNacionalSdk(options =>
{
    options.Environment = NFSeEnvironment.ProductionRestricted;
    options.CertificateFile = new NFSeCertificateFileOptions
    {
        Path = "certificado.pfx",
        Password = "senha-do-certificado"
    };
});

using var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<INFSeClient>();
```

### Usar em VB.NET / .NET Framework 4.6.2

Em projetos legados, instale o pacote NuGet `NFSeNacionalSdk` no projeto .NET Framework. O NuGet deve restaurar tambem os pacotes transitivos necessarios para `netstandard2.0`. Se o projeto usar `packages.config`, habilite ou gere binding redirects quando o Visual Studio solicitar.

Exemplo em VB.NET:

```vbnet
Imports NFSeNacionalSdk
Imports NFSeNacionalSdk.Contracts.Requests
Imports NFSeNacionalSdk.Core.Enums
Imports NFSeNacionalSdk.Core.Options

Dim client = NFSeClientFactory.Create(
    Sub(options)
        options.Environment = NFSeEnvironment.ProductionRestricted
        options.CertificateFile = New NFSeCertificateFileOptions With {
            .Path = "C:\certificados\certificado.pfx",
            .Password = "senha-do-certificado"
        }
    End Sub)

Dim request = New GetNfseByAccessKeyRequest With {
    .AccessKey = "<CHAVE_ACESSO_NFSE>"
}

Dim result = client.GetNfseByAccessKeyAsync(request).GetAwaiter().GetResult()

If result.Success AndAlso result.Document IsNot Nothing Then
    Console.WriteLine(result.Document.Number)
    Console.WriteLine(result.RawXml)
End If

client.Dispose()
```

Para emissao, `DateOnly` tambem fica disponivel no target `netstandard2.0` pelo proprio pacote de contratos. Esse shim de compatibilidade foi preservado nesta release para evitar breaking change; substitui-lo por outro tipo continua sendo divida tecnica para uma major version:

```vbnet
Dim emissao = New EmitDpsRequest With {
    .Series = "1",
    .Number = "1",
    .CompetenceDate = New DateOnly(2026, 4, 29),
    .IssuedAt = DateTimeOffset.Now,
    .MunicipalityCode = "3201506"
}
```

### Consultar NFS-e por chave

```csharp
using NFSeNacionalSdk.Contracts.Requests;

var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
{
    AccessKey = "<CHAVE_ACESSO_NFSE>"
}, cancellationToken);

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
        SimplesNationalTotalTaxRate = 2.00m,
        Federal = new EmitDpsFederalTaxation
        {
            PisCofins = new EmitDpsPisCofinsTaxation
            {
                TaxStatusCode = "01",
                CalculationBase = 1.00m,
                PisRate = 0.65m,
                CofinsRate = 3.00m,
                PisAmount = 0.01m,
                CofinsAmount = 0.03m,
                WithholdingType = NFSePisCofinsWithholdingType.PisCofinsCsllNotWithheld
            }
        },
        IbsCbs = new EmitDpsIbsCbsTaxation
        {
            OperationIndicatorCode = "010101",
            DestinationIndicator = NFSeIbsCbsDestinationIndicator.No,
            TaxStatusCode = "000",
            TaxClassificationCode = "000001"
        }
    }
}, cancellationToken);

Console.WriteLine(result.Success);
Console.WriteLine(result.AccessKey);
Console.WriteLine(result.Document?.Number);
Console.WriteLine(result.SubmittedDpsXml);
Console.WriteLine(result.RawXml);
```

Para ME/EPP com `opSimpNac = 3`, `regApTribSN = 1` e ISSQN nao retido, informe `IssRate = null`. Para esse mesmo caso, use `TotalTaxIndicator = null` e informe `SimplesNationalTotalTaxRate`.

O SDK nao arredonda valores fiscais silenciosamente: valores com mais de duas casas sao rejeitados. Campos opcionais de IBS/CBS nao sao inventados; regras e codigos ainda preliminares da NT 009 ficaram fora da API.

### Cancelar NFS-e por evento

O cancelamento registra um evento oficial para a chave informada. Teste primeiro em Producao Restrita e confirme as regras do municipio/prestador antes de usar em producao.

```csharp
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;

var result = await client.CancelNfseAsync(new CancelNfseRequest
{
    AccessKey = "<CHAVE_ACESSO_NFSE>",
    AuthorTaxId = "<CNPJ_OU_CPF_AUTOR>",
    ReasonCode = NFSeCancellationReasonCode.ServiceNotProvided,
    Reason = "Servico nao prestado ao tomador conforme acordado."
}, cancellationToken);

Console.WriteLine(result.Success);
Console.WriteLine(result.StatusCode);
Console.WriteLine(result.EventId);
Console.WriteLine(result.SubmittedEventXml);
Console.WriteLine(result.RawXml);
Console.WriteLine(result.Event?.Description);
```

### Consultar eventos

```csharp
var eventos = await client.GetNfseEventsAsync(new GetNfseEventsRequest
{
    AccessKey = "<CHAVE_ACESSO_NFSE>",
    EventTypeCode = "101101", // opcional
    SequenceNumber = 1        // opcional; requer EventTypeCode
}, cancellationToken);

foreach (var evento in eventos.Events)
    Console.WriteLine($"{evento.TypeCode}/{evento.SequenceNumber}: {evento.Description}");
```

Sem tipo, o endpoint lista todos os eventos da chave. Com tipo, filtra o tipo; com tipo e sequencia, consulta a ocorrencia especifica. `RawJson` e `RawXmlDocuments` preservam as respostas originais.

### Respostas padronizadas

As respostas principais implementam `INFSeResponse` e expoem:

- `Success`: resultado normalizado da operacao
- `StatusCode`: status HTTP retornado pela API
- `Messages`: erros, alertas ou mensagens de negocio
- `RawXml`: XML bruto quando a API retornar XML compactado em base64
- `RawJson`: JSON bruto retornado pela API

Consultas e emissoes que retornam documento tambem disponibilizam objeto estruturado em `Document`. Eventos de cancelamento retornam dados estruturados em `Event`.

HTTP 2xx vazio nao e tratado como sucesso de negocio. A validacao de XML recebido fica ativa no cliente criado por `NFSeSdkOptions` (`ValidateResponseXml = true`); ela valida estrutura XSD e protecoes de parsing, mas nao valida cadeia de confianca, revogacao ou autoria da assinatura XML.

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

Os resultados agora incluem `GetMunicipalConventionResult.Parameters` e `GetMunicipalServiceParametersResult.TaxRates`. Campos desconhecidos continuam tolerados e o JSON integral permanece em `RawJson`.

## Testes de integracao

O projeto `tests/NFSeNacionalSdk.IntegrationTests` e opt-in. Sem credenciais, os testes ficam marcados como ignorados. Para executar contra Producao Restrita:

```powershell
$env:NFSE_INTEGRATION_ENABLED="1"
$env:NFSE_INTEGRATION_CERTIFICATE_PATH="C:\caminho\certificado.pfx"
$env:NFSE_INTEGRATION_CERTIFICATE_PASSWORD="senha"
$env:NFSE_INTEGRATION_MUNICIPALITY_CODE="3204005"
dotnet test tests/NFSeNacionalSdk.IntegrationTests -c Release
```

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

Para sobrescrever a versao no pack:

```bash
dotnet pack --configuration Release --no-build --output artifacts/packages -p:Version=0.2.0
```

Para a preview `0.2.0`:

```bash
dotnet pack --configuration Release --no-build --output artifacts/packages -p:Version=0.2.0-preview.1
```

## Release pelo GitHub Actions

O workflow `.github/workflows/release.yml` publica os pacotes no NuGet usando Trusted Publishing.

Configuracao necessaria no GitHub:

- Environment: `release`
- Environment variable: `NUGET_USER` com o username do perfil no nuget.org, nao o e-mail
- Secrets: nenhum, quando Trusted Publishing estiver configurado
- Deployment branches and tags: selecione tags `v*` para releases por tag; adicione tambem a branch `main` apenas se quiser permitir publicacao manual pelo botao do GitHub Actions
- Required reviewers: recomendado para evitar publicacao acidental

Configuracao necessaria no nuget.org:

- Trusted Publishing apontando para este repositorio
- Workflow file: `release.yml`
- Environment: `release`

Publicar por tag:

```bash
git tag v0.2.0-preview.1
git push origin v0.2.0-preview.1
```

Ou execute manualmente o workflow `Release NuGet` no GitHub e informe a versao, por exemplo `0.2.0-preview.1`. Para esse modo manual, a branch usada na execucao precisa estar permitida nas regras do Environment.

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
  NFSeNacionalSdk.IntegrationTests

samples/
  NFSeNacionalSdk.Samples.Console
```

## Referencias Tecnicas

- Documentacao tecnica atual da NFS-e: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/documentacao-atual
- APIs de Producao Restrita e Producao: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/apis-prod-restrita-e-producao
- RTC e Notas Tecnicas: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/rtc
- Historico de atualizacoes: https://www.gov.br/nfse/pt-br/biblioteca/documentacao-tecnica/atualizacoes-e-implantacoes
- Schemas embarcados: `NFSe-ESQUEMAS_XSD-v1.01-20260209` e `NFSe-ESQUEMAS_XSD-PRODREST-v1.01-20260727`

## Contribuicao

Leia [CONTRIBUTING.md](./CONTRIBUTING.md) antes de abrir issues ou pull requests.

## Commits

Use Conventional Commits:

- `feat: add nfse cancellation events`
- `fix: parse nfse taxation values`
- `docs: update release instructions`
- `build: add nuget package metadata`
- `test: cover municipal parameter lookup`

## Licenca

MIT.
