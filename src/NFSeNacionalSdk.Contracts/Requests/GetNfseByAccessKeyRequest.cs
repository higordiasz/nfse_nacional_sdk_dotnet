namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class GetNfseByAccessKeyRequest
{
    private string _accessKey = string.Empty;

    public string AccessKey
    {
        get => _accessKey;
        set => _accessKey = NormalizeAccessKey(value);
    }

    private static string NormalizeAccessKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The NFSe access key must be provided.", nameof(value));
        }

        return value.Trim();
    }
}
