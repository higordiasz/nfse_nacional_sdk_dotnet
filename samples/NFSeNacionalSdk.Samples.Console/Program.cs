using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NFSeNacionalSdk;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Core.Options;

Console.OutputEncoding = Encoding.UTF8;

var configuration = new SampleConfiguration
{
    Environment = ParseEnvironment(Environment.GetEnvironmentVariable("NFSE_ENVIRONMENT")),
    CertificatePath = Normalize(Environment.GetEnvironmentVariable("NFSE_CERTIFICATE_PATH")),
    CertificatePassword = Environment.GetEnvironmentVariable("NFSE_CERTIFICATE_PASSWORD"),
    AccessKey = Normalize(Environment.GetEnvironmentVariable("NFSE_ACCESS_KEY"))
};

WriteWelcome(configuration);

while (true)
{
    WriteMenu(configuration);

    var option = Console.ReadLine()?.Trim();

    switch (option)
    {
        case "1":
            await ExecuteLookupAsync(configuration);
            break;
        case "2":
            await ExecuteEmissionAsync(configuration);
            break;
        case "3":
            ConfigureEnvironment(configuration);
            break;
        case "4":
            ConfigureCertificate(configuration);
            break;
        case "5":
            ShowResolvedEndpoints(configuration.Environment);
            break;
        case "6":
            return;
        default:
            Console.WriteLine("Opcao invalida. Escolha uma das opcoes do menu.");
            break;
    }
}

static async Task ExecuteLookupAsync(SampleConfiguration configuration)
{
    configuration.AccessKey = PromptForValue(
        "Informe a chave de acesso da NFS-e",
        configuration.AccessKey,
        allowEmpty: false);

    using var client = CreateClient(configuration);
    if (client is null)
    {
        return;
    }

    var endpoints = NFSeEndpointsOptions.For(configuration.Environment);
    var resolvedPath = endpoints.NfseByAccessKeyPath.Replace(
        "{chaveAcesso}",
        Uri.EscapeDataString(configuration.AccessKey),
        StringComparison.Ordinal);
    var resolvedUrl = new Uri(new Uri(endpoints.BaseUrl, UriKind.Absolute), resolvedPath.TrimStart('/'));

    Console.WriteLine();
    Console.WriteLine("Executando consulta...");
    Console.WriteLine($"Ambiente: {GetEnvironmentLabel(configuration.Environment)}");
    Console.WriteLine($"BaseUrl: {endpoints.BaseUrl}");
    Console.WriteLine($"Path: {resolvedPath}");
    Console.WriteLine($"URL final: {resolvedUrl}");
    WriteCertificateSummary(client.CertificatePath, client.Certificate);

    try
    {
        var result = await client.Instance.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
        {
            AccessKey = configuration.AccessKey
        });

        Console.WriteLine();
        Console.WriteLine("Resultado da consulta");
        Console.WriteLine($"HTTP: {(int)result.StatusCode} ({result.StatusCode})");
        Console.WriteLine($"Success: {result.Success}");
        Console.WriteLine($"AccessKey: {result.AccessKey}");

        if (result.Document is not null)
        {
            Console.WriteLine($"Numero: {result.Document.Number ?? "(nao informado)"}");
            Console.WriteLine($"Emitida em: {result.Document.IssuedAt:O}");
            Console.WriteLine($"Prestador: {result.Document.Issuer?.Name ?? "(nao informado)"}");
            Console.WriteLine($"Tomador: {result.Document.Recipient?.Name ?? "(nao informado)"}");
            Console.WriteLine($"Codigo de servico: {result.Document.Service?.ServiceCode ?? "(nao informado)"}");
            Console.WriteLine($"Valor do servico: {result.Document.Service?.ServiceAmount?.ToString(CultureInfo.InvariantCulture) ?? "(nao informado)"}");
        }

        WriteMessages(result.Messages);
        WriteXmlIfPresent(result.RawXml, "Raw XML");
        WriteJsonIfPresent(result.JsonContent);
    }
    catch (NFSeTransportException exception)
    {
        Console.WriteLine($"Falha de transporte ao consultar a API: {exception.Message}");
        WriteInnerException(exception);
    }
    catch (NFSeSerializationException exception)
    {
        Console.WriteLine($"Falha ao interpretar o XML retornado pela API: {exception.Message}");
        WriteInnerException(exception);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Falha inesperada durante a consulta: {exception.Message}");
        WriteInnerException(exception);
    }

    Console.WriteLine();
}

static async Task ExecuteEmissionAsync(SampleConfiguration configuration)
{
    using var client = CreateClient(configuration);
    if (client is null)
    {
        return;
    }

    var request = PromptEmissionRequest();
    var endpoints = NFSeEndpointsOptions.For(configuration.Environment);
    var resolvedUrl = new Uri(new Uri(endpoints.BaseUrl, UriKind.Absolute), endpoints.NfsePath.TrimStart('/'));

    Console.WriteLine();
    Console.WriteLine("Executando emissao sincrona da DPS...");
    Console.WriteLine($"Ambiente: {GetEnvironmentLabel(configuration.Environment)}");
    Console.WriteLine($"BaseUrl: {endpoints.BaseUrl}");
    Console.WriteLine($"Path: {endpoints.NfsePath}");
    Console.WriteLine($"URL final: {resolvedUrl}");
    WriteCertificateSummary(client.CertificatePath, client.Certificate);

    try
    {
        var result = await client.Instance.EmitDpsAsync(request);

        Console.WriteLine();
        Console.WriteLine("Resultado da emissao");
        Console.WriteLine($"HTTP: {(int)result.StatusCode} ({result.StatusCode})");
        Console.WriteLine($"Success: {result.Success}");
        Console.WriteLine($"DpsId: {result.DpsId ?? "(nao informado)"}");
        Console.WriteLine($"AccessKey: {result.AccessKey ?? "(nao informado)"}");

        if (result.Document is not null)
        {
            Console.WriteLine($"Numero da NFS-e: {result.Document.Number ?? "(nao informado)"}");
            Console.WriteLine($"Emitida em: {result.Document.IssuedAt:O}");
            Console.WriteLine($"Prestador: {result.Document.Issuer?.Name ?? "(nao informado)"}");
            Console.WriteLine($"Tomador: {result.Document.Recipient?.Name ?? "(nao informado)"}");
        }

        WriteMessages(result.Messages);
        WriteXmlIfPresent(result.SubmittedDpsXml, "DPS XML assinado");
        WriteXmlIfPresent(result.RawXml, "NFS-e XML retornada");
        WriteJsonIfPresent(result.JsonContent);
    }
    catch (NFSeTransportException exception)
    {
        Console.WriteLine($"Falha de transporte ao emitir a DPS: {exception.Message}");
        WriteInnerException(exception);
    }
    catch (NFSeSerializationException exception)
    {
        Console.WriteLine($"Falha ao gerar ou interpretar o XML da operacao: {exception.Message}");
        WriteInnerException(exception);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Falha inesperada durante a emissao: {exception.Message}");
        WriteInnerException(exception);
    }

    Console.WriteLine();
}

static EmitDpsRequest PromptEmissionRequest()
{
    var defaultNumber = DateTimeOffset.Now.ToString("ddHHmmss", CultureInfo.InvariantCulture);
    var series = PromptForValue("Serie do DPS", "70000", allowEmpty: false);
    var number = PromptForValue("Numero do DPS", defaultNumber, allowEmpty: false);
    var competenceDate = PromptDateOnly("Data de competencia (yyyy-MM-dd)", DateOnly.FromDateTime(DateTime.Today));
    var issuedAt = PromptDateTimeOffset("Data e hora de emissao (yyyy-MM-ddTHH:mm:sszzz)", DateTimeOffset.Now);
    var municipalityCode = PromptForValue("Codigo IBGE do municipio emissor", null, allowEmpty: false);

    var provider = new EmitDpsProvider
    {
        TaxId = PromptForValue("CNPJ ou CPF do prestador", null, allowEmpty: false),
        MunicipalRegistration = PromptForValue("Inscricao municipal do prestador", null, allowEmpty: true),
        Name = PromptForValue("Nome do prestador", null, allowEmpty: true),
        Phone = PromptForValue("Telefone do prestador", null, allowEmpty: true),
        Email = PromptForValue("Email do prestador", null, allowEmpty: true),
        SimplesNationalOption = PromptEnum(
            "Opcao do Simples Nacional (1 = Nao optante, 2 = MEI, 3 = ME/EPP)",
            NFSeSimplesNationalOption.Mei),
        SimplifiedNationalTaxRegime = PromptOptionalEnum<NFSeSimplifiedNationalTaxRegime>(
            "Regime de apuracao do Simples Nacional (1, 2 ou 3; Enter para omitir)"),
        SpecialTaxRegime = PromptEnum(
            "Regime especial do prestador (0, 1, 2, 3, 4, 5, 6 ou 9)",
            NFSeSpecialTaxRegime.None)
    };

    var recipient = PromptOptionalRecipient(municipalityCode);
    var serviceLocationMunicipality = PromptForValue(
        "Codigo IBGE do local da prestacao",
        municipalityCode,
        allowEmpty: false);
    var service = new EmitDpsService
    {
        ServiceLocationMunicipalityCode = serviceLocationMunicipality,
        NationalTaxationCode = PromptForValue("Codigo nacional do servico (cTribNac)", null, allowEmpty: false),
        MunicipalTaxationCode = PromptForValue("Codigo municipal do servico (opcional)", null, allowEmpty: true),
        Description = PromptForValue("Descricao do servico", null, allowEmpty: false),
        NationalClassificationCode = PromptForValue("Codigo NBS (opcional)", null, allowEmpty: true),
        InternalCode = PromptForValue("Codigo interno do contribuinte (opcional)", null, allowEmpty: true),
        Amount = PromptDecimal("Valor do servico", 1.00m)
    };

    var taxation = new EmitDpsTaxation
    {
        IssTaxationType = PromptEnum(
            "Tributacao do ISSQN (1 = Tributavel, 2 = Imunidade, 3 = Exportacao, 4 = Nao incidencia)",
            NFSeIssTaxationType.TaxableOperation),
        IssWithholdingType = PromptEnum(
            "Retencao do ISSQN (1 = Nao retido, 2 = Retido pelo tomador, 3 = Retido pelo intermediario)",
            NFSeIssWithholdingType.NotWithheld),
        TotalTaxIndicator = NFSeTotalTaxIndicator.NotInformed
    };

    return new EmitDpsRequest
    {
        Series = series,
        Number = number,
        CompetenceDate = competenceDate,
        IssuedAt = issuedAt,
        MunicipalityCode = municipalityCode,
        Provider = provider,
        Recipient = recipient,
        Service = service,
        Taxation = taxation
    };
}

static EmitDpsRecipient? PromptOptionalRecipient(string defaultMunicipalityCode)
{
    var taxId = PromptForValue("CNPJ ou CPF do tomador (Enter para omitir)", null, allowEmpty: true);
    if (string.IsNullOrWhiteSpace(taxId))
    {
        return null;
    }

    var address = PromptOptionalAddress("tomador", defaultMunicipalityCode);

    return new EmitDpsRecipient
    {
        TaxId = taxId,
        Name = PromptForValue("Nome do tomador", null, allowEmpty: false),
        MunicipalRegistration = PromptForValue("Inscricao municipal do tomador", null, allowEmpty: true),
        Address = address,
        Phone = PromptForValue("Telefone do tomador", null, allowEmpty: true),
        Email = PromptForValue("Email do tomador", null, allowEmpty: true)
    };
}

static EmitDpsAddress? PromptOptionalAddress(string label, string defaultMunicipalityCode)
{
    Console.Write($"Deseja informar endereco do {label}? (s/N): ");
    var option = Console.ReadLine()?.Trim();

    if (!string.Equals(option, "s", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    return new EmitDpsAddress
    {
        MunicipalityCode = PromptForValue($"Codigo IBGE do municipio do {label}", defaultMunicipalityCode, allowEmpty: false),
        ZipCode = PromptForValue($"CEP do {label}", null, allowEmpty: false),
        Street = PromptForValue($"Logradouro do {label}", null, allowEmpty: false),
        Number = PromptForValue($"Numero do {label}", null, allowEmpty: false),
        Complement = PromptForValue($"Complemento do {label}", null, allowEmpty: true),
        Neighborhood = PromptForValue($"Bairro do {label}", null, allowEmpty: false)
    };
}

static void ConfigureEnvironment(SampleConfiguration configuration)
{
    Console.WriteLine();
    Console.WriteLine("Selecione o ambiente");
    Console.WriteLine("1. Producao Restrita");
    Console.WriteLine("2. Producao");
    Console.Write("Opcao: ");

    var option = Console.ReadLine()?.Trim();

    configuration.Environment = option switch
    {
        "1" => NFSeEnvironment.ProductionRestricted,
        "2" => NFSeEnvironment.Production,
        _ => configuration.Environment
    };

    Console.WriteLine($"Ambiente atual: {GetEnvironmentLabel(configuration.Environment)}");
}

static void ConfigureCertificate(SampleConfiguration configuration)
{
    Console.WriteLine();
    configuration.CertificatePath = PromptForValue(
        "Informe o caminho completo do certificado .pfx",
        configuration.CertificatePath,
        allowEmpty: false);

    Console.Write("Senha do certificado: ");
    configuration.CertificatePassword = ReadPassword();
}

static void ShowResolvedEndpoints(NFSeEnvironment environment)
{
    var endpoints = NFSeEndpointsOptions.For(environment);

    Console.WriteLine();
    Console.WriteLine("Endpoints resolvidos");
    Console.WriteLine($"Ambiente: {GetEnvironmentLabel(environment)}");
    Console.WriteLine($"BaseUrl: {endpoints.BaseUrl}");
    Console.WriteLine($"Consulta por chave: {endpoints.NfseByAccessKeyPath}");
    Console.WriteLine($"Emissao sincrona NFS-e: {endpoints.NfsePath}");
    Console.WriteLine($"Consulta DPS por Id: {endpoints.DpsByIdPath}");
    Console.WriteLine();
}

static void WriteWelcome(SampleConfiguration configuration)
{
    Console.WriteLine("NFSe Nacional SDK - Console de Homologacao");
    Console.WriteLine("Este sample usa certificado local para testar consulta por chave e emissao sincronica.");
    Console.WriteLine($"Ambiente inicial: {GetEnvironmentLabel(configuration.Environment)}");

    if (!string.IsNullOrWhiteSpace(configuration.CertificatePath))
    {
        Console.WriteLine($"Certificado inicial: {configuration.CertificatePath}");
    }

    if (!string.IsNullOrWhiteSpace(configuration.AccessKey))
    {
        Console.WriteLine($"Chave inicial: {configuration.AccessKey}");
    }

    Console.WriteLine();
}

static void WriteMenu(SampleConfiguration configuration)
{
    Console.WriteLine("Menu");
    Console.WriteLine("1. Consultar NFS-e por chave");
    Console.WriteLine("2. Emitir DPS e gerar NFS-e");
    Console.WriteLine("3. Alterar ambiente");
    Console.WriteLine("4. Configurar certificado");
    Console.WriteLine("5. Mostrar endpoints resolvidos");
    Console.WriteLine("6. Sair");
    Console.WriteLine($"Ambiente atual: {GetEnvironmentLabel(configuration.Environment)}");
    Console.Write("Escolha uma opcao: ");
}

static string PromptForValue(string label, string? currentValue, bool allowEmpty)
{
    Console.Write(label);

    if (!string.IsNullOrWhiteSpace(currentValue))
    {
        Console.Write($" [{currentValue}]");
    }

    Console.Write(": ");

    var value = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(value))
    {
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            return currentValue;
        }

        if (!allowEmpty)
        {
            Console.WriteLine("Esse valor e obrigatorio.");
            return PromptForValue(label, currentValue, allowEmpty);
        }
    }

    return value?.Trim() ?? string.Empty;
}

static TEnum PromptEnum<TEnum>(string label, TEnum defaultValue)
    where TEnum : struct, Enum
{
    var numericDefault = Convert.ToInt32(defaultValue, CultureInfo.InvariantCulture);
    var value = PromptForValue(label, numericDefault.ToString(CultureInfo.InvariantCulture), allowEmpty: false);

    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
        Enum.IsDefined(typeof(TEnum), parsed))
    {
        return (TEnum)Enum.ToObject(typeof(TEnum), parsed);
    }

    Console.WriteLine("Opcao invalida para esse campo.");
    return PromptEnum(label, defaultValue);
}

static TEnum? PromptOptionalEnum<TEnum>(string label)
    where TEnum : struct, Enum
{
    var value = PromptForValue(label, null, allowEmpty: true);
    if (string.IsNullOrWhiteSpace(value))
    {
        return null;
    }

    if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) &&
        Enum.IsDefined(typeof(TEnum), parsed))
    {
        return (TEnum)Enum.ToObject(typeof(TEnum), parsed);
    }

    Console.WriteLine("Opcao invalida para esse campo.");
    return PromptOptionalEnum<TEnum>(label);
}

static DateOnly PromptDateOnly(string label, DateOnly defaultValue)
{
    var value = PromptForValue(label, defaultValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), allowEmpty: false);

    if (DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
    {
        return result;
    }

    Console.WriteLine("Data invalida. Use o formato yyyy-MM-dd.");
    return PromptDateOnly(label, defaultValue);
}

static DateTimeOffset PromptDateTimeOffset(string label, DateTimeOffset defaultValue)
{
    var value = PromptForValue(label, defaultValue.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture), allowEmpty: false);

    if (DateTimeOffset.TryParseExact(
            value,
            "yyyy-MM-ddTHH:mm:sszzz",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result))
    {
        return result;
    }

    Console.WriteLine("Data/hora invalida. Use o formato yyyy-MM-ddTHH:mm:sszzz.");
    return PromptDateTimeOffset(label, defaultValue);
}

static decimal PromptDecimal(string label, decimal defaultValue)
{
    var defaultText = defaultValue.ToString("0.00", CultureInfo.InvariantCulture);
    var value = PromptForValue(label, defaultText, allowEmpty: false);

    if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantResult))
    {
        return invariantResult;
    }

    if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var currentCultureResult))
    {
        return currentCultureResult;
    }

    Console.WriteLine("Valor invalido.");
    return PromptDecimal(label, defaultValue);
}

static string ReadPassword()
{
    var buffer = new StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return buffer.ToString();
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (buffer.Length > 0)
            {
                buffer.Length--;
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            buffer.Append(key.KeyChar);
        }
    }
}

static ClientContext? CreateClient(SampleConfiguration configuration)
{
    if (string.IsNullOrWhiteSpace(configuration.CertificatePath))
    {
        ConfigureCertificate(configuration);
    }

    if (string.IsNullOrWhiteSpace(configuration.CertificatePath))
    {
        Console.WriteLine("O caminho do certificado e obrigatorio para executar essa operacao.");
        return null;
    }

    if (!File.Exists(configuration.CertificatePath))
    {
        Console.WriteLine($"Certificado nao encontrado em: {configuration.CertificatePath}");
        return null;
    }

    if (configuration.CertificatePassword is null)
    {
        Console.Write("Senha do certificado: ");
        configuration.CertificatePassword = ReadPassword();
    }

    try
    {
        var certificate = LoadCertificate(configuration.CertificatePath, configuration.CertificatePassword);
        var client = new NFSeClient(
            new NFSeSdkOptions
            {
                Environment = configuration.Environment,
                UserAgent = "NFSeNacionalSdk.Samples.Console"
            },
            certificate);

        return new ClientContext(client, certificate, configuration.CertificatePath);
    }
    catch (CryptographicException exception)
    {
        configuration.CertificatePassword = null;
        Console.WriteLine($"Falha ao carregar o certificado. Confira o arquivo e a senha. Detalhe: {exception.Message}");
        return null;
    }
}

static void WriteCertificateSummary(string certificatePath, X509Certificate2 certificate)
{
    Console.WriteLine($"Certificado: {certificatePath}");
    Console.WriteLine($"Subject: {certificate.Subject}");
    Console.WriteLine($"Issuer: {certificate.Issuer}");
    Console.WriteLine($"Valido ate: {certificate.NotAfter:O}");
}

static void WriteMessages(IReadOnlyList<NFSeNacionalSdk.Contracts.Responses.NFSeMessage> messages)
{
    if (messages.Count == 0)
    {
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Mensagens retornadas pela API");

    foreach (var message in messages)
    {
        Console.WriteLine($"- {message.Code ?? "SEM-CODIGO"}: {message.Description}");
    }
}

static void WriteXmlIfPresent(string? xml, string title)
{
    Console.WriteLine();

    if (string.IsNullOrWhiteSpace(xml))
    {
        Console.WriteLine($"{title}: nao disponivel.");
        return;
    }

    Console.WriteLine(title);
    Console.WriteLine(xml);
}

static void WriteJsonIfPresent(string? json)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return;
    }

    Console.WriteLine();
    Console.WriteLine("JSON");
    Console.WriteLine(json);
}

static X509Certificate2 LoadCertificate(string certificatePath, string? certificatePassword)
{
    return X509CertificateLoader.LoadPkcs12FromFile(
        certificatePath,
        certificatePassword,
        X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable,
        Pkcs12LoaderLimits.Defaults);
}

static NFSeEnvironment ParseEnvironment(string? value)
{
    return Enum.TryParse<NFSeEnvironment>(value, ignoreCase: true, out var environment)
        ? environment
        : NFSeEnvironment.ProductionRestricted;
}

static string GetEnvironmentLabel(NFSeEnvironment environment)
{
    return environment switch
    {
        NFSeEnvironment.ProductionRestricted => "Producao Restrita",
        NFSeEnvironment.Production => "Producao",
        _ => environment.ToString()
    };
}

static string? Normalize(string? value)
{
    var normalized = value?.Trim();
    return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
}

static void WriteInnerException(Exception exception)
{
    if (exception.InnerException is not null)
    {
        Console.WriteLine($"InnerException: {exception.InnerException.Message}");
    }
}

sealed class SampleConfiguration
{
    public NFSeEnvironment Environment { get; set; }

    public string? CertificatePath { get; set; }

    public string? CertificatePassword { get; set; }

    public string? AccessKey { get; set; }
}

sealed class ClientContext : IDisposable
{
    public ClientContext(NFSeClient instance, X509Certificate2 certificate, string certificatePath)
    {
        Instance = instance;
        Certificate = certificate;
        CertificatePath = certificatePath;
    }

    public NFSeClient Instance { get; }

    public X509Certificate2 Certificate { get; }

    public string CertificatePath { get; }

    public void Dispose()
    {
        Instance.Dispose();
        Certificate.Dispose();
    }
}
