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
            ConfigureEnvironment(configuration);
            break;
        case "3":
            ConfigureCertificate(configuration);
            break;
        case "4":
            ShowResolvedEndpoints(configuration.Environment);
            break;
        case "5":
            return;
        default:
            Console.WriteLine("Opção inválida. Escolha uma das opções do menu.");
            break;
    }
}

static async Task ExecuteLookupAsync(SampleConfiguration configuration)
{
    configuration.AccessKey = PromptForValue(
        "Informe a chave de acesso da NFS-e",
        configuration.AccessKey,
        allowEmpty: false);

    if (string.IsNullOrWhiteSpace(configuration.CertificatePath))
    {
        ConfigureCertificate(configuration);
    }

    if (string.IsNullOrWhiteSpace(configuration.CertificatePath))
    {
        Console.WriteLine("O caminho do certificado é obrigatório para executar a consulta.");
        return;
    }

    if (!File.Exists(configuration.CertificatePath))
    {
        Console.WriteLine($"Certificado não encontrado em: {configuration.CertificatePath}");
        return;
    }

    if (configuration.CertificatePassword is null)
    {
        Console.Write("Senha do certificado: ");
        configuration.CertificatePassword = ReadPassword();
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
    Console.WriteLine($"Certificado: {configuration.CertificatePath}");

    try
    {
        using var certificate = LoadCertificate(configuration.CertificatePath, configuration.CertificatePassword);

        Console.WriteLine($"Subject: {certificate.Subject}");
        Console.WriteLine($"Issuer: {certificate.Issuer}");
        Console.WriteLine($"Válido até: {certificate.NotAfter:O}");

        using var client = new NFSeClient(
            new NFSeSdkOptions
            {
                Environment = configuration.Environment,
                UserAgent = "NFSeNacionalSdk.Samples.Console"
            },
            certificate);

        var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
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
            Console.WriteLine($"Número: {result.Document.Number ?? "(não informado)"}");
            Console.WriteLine($"Emitida em: {result.Document.IssuedAt:O}");
            Console.WriteLine($"Prestador: {result.Document.Issuer?.Name ?? "(não informado)"}");
            Console.WriteLine($"Tomador: {result.Document.Recipient?.Name ?? "(não informado)"}");
            Console.WriteLine($"Código de serviço: {result.Document.Service?.ServiceCode ?? "(não informado)"}");
            Console.WriteLine($"Valor do serviço: {result.Document.Service?.ServiceAmount?.ToString() ?? "(não informado)"}");
        }

        if (result.Messages.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Mensagens retornadas pela API");

            foreach (var message in result.Messages)
            {
                Console.WriteLine($"- {message.Code ?? "SEM-CODIGO"}: {message.Description}");
            }
        }

        if (!string.IsNullOrWhiteSpace(result.RawXml))
        {
            Console.WriteLine();
            Console.WriteLine("Raw XML");
            Console.WriteLine(result.RawXml);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("A API não retornou XML da NFS-e nessa consulta.");
        }

        if (!string.IsNullOrWhiteSpace(result.JsonContent))
        {
            Console.WriteLine();
            Console.WriteLine("JSON");
            Console.WriteLine(result.JsonContent);
        }
    }
    catch (CryptographicException exception)
    {
        configuration.CertificatePassword = null;
        Console.WriteLine($"Falha ao carregar o certificado. Confira o arquivo e a senha. Detalhe: {exception.Message}");
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

static void ConfigureEnvironment(SampleConfiguration configuration)
{
    Console.WriteLine();
    Console.WriteLine("Selecione o ambiente");
    Console.WriteLine("1. Produção Restrita");
    Console.WriteLine("2. Produção");
    Console.Write("Opção: ");

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
    Console.WriteLine($"Consulta genérica NFS-e: {endpoints.NfsePath}");
    Console.WriteLine($"Consulta DPS por Id: {endpoints.DpsByIdPath}");
    Console.WriteLine();
}

static void WriteWelcome(SampleConfiguration configuration)
{
    Console.WriteLine("NFSe Nacional SDK - Console de Homologação");
    Console.WriteLine("Este sample usa certificado local para testar a consulta por chave de acesso.");
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
    Console.WriteLine("2. Alterar ambiente");
    Console.WriteLine("3. Configurar certificado");
    Console.WriteLine("4. Mostrar endpoints resolvidos");
    Console.WriteLine("5. Sair");
    Console.WriteLine($"Ambiente atual: {GetEnvironmentLabel(configuration.Environment)}");
    Console.Write("Escolha uma opção: ");
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
            Console.WriteLine("Esse valor é obrigatório.");
            return PromptForValue(label, currentValue, allowEmpty);
        }
    }

    return value?.Trim() ?? string.Empty;
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
        NFSeEnvironment.ProductionRestricted => "Produção Restrita",
        NFSeEnvironment.Production => "Produção",
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
