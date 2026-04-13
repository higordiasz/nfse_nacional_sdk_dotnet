using System.Security.Cryptography.X509Certificates;
using NFSeNacionalSdk;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Options;

var accessKey = Environment.GetEnvironmentVariable("NFSE_ACCESS_KEY");
var certificateBase64 = Environment.GetEnvironmentVariable("NFSE_CERTIFICATE_BASE64");
var certificatePassword = Environment.GetEnvironmentVariable("NFSE_CERTIFICATE_PASSWORD");

if (string.IsNullOrWhiteSpace(accessKey) ||
    string.IsNullOrWhiteSpace(certificateBase64) ||
    string.IsNullOrWhiteSpace(certificatePassword))
{
    Console.WriteLine("Configure os env vars NFSE_ACCESS_KEY, NFSE_CERTIFICATE_BASE64 e NFSE_CERTIFICATE_PASSWORD para executar o exemplo.");
    return;
}

var certificateBytes = Convert.FromBase64String(certificateBase64);
using var certificate = new X509Certificate2(
    certificateBytes,
    certificatePassword,
    X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

using var client = new NFSeClient(
    new NFSeSdkOptions
    {
        Environment = NFSeEnvironment.ProductionRestricted,
        UserAgent = "NFSeNacionalSdk.Samples.Console"
    },
    certificate);

var result = await client.GetNfseByAccessKeyAsync(new GetNfseByAccessKeyRequest
{
    AccessKey = accessKey
});

if (!result.Success)
{
    Console.WriteLine("A API retornou erro de negócio para a consulta.");

    foreach (var message in result.Messages)
    {
        Console.WriteLine($"{message.Code ?? "SEM-CODIGO"}: {message.Description}");
    }

    return;
}

Console.WriteLine("NFSe consultada com sucesso.");
Console.WriteLine($"Chave: {result.AccessKey}");
Console.WriteLine($"Numero: {result.Document?.Number}");
Console.WriteLine($"Emitida em: {result.Document?.IssuedAt:O}");
Console.WriteLine("JSON:");
Console.WriteLine(result.JsonContent);
