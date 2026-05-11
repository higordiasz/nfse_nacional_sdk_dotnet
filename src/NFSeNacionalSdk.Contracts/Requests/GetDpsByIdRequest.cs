namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class GetDpsByIdRequest
{
    private string _dpsId = string.Empty;

    public string DpsId
    {
        get => _dpsId;
        set => _dpsId = NormalizeDpsId(value);
    }

    private static string NormalizeDpsId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The DPS id must be provided.", nameof(value));
        }

        return value.Trim();
    }
}
