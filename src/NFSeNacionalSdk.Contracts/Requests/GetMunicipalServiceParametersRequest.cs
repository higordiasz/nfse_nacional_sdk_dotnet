namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class GetMunicipalServiceParametersRequest
{
    private string _municipalityCode = string.Empty;
    private string _serviceCode = string.Empty;

    public required string MunicipalityCode
    {
        get => _municipalityCode;
        init => _municipalityCode = NormalizeMunicipalityCode(value);
    }

    public required string ServiceCode
    {
        get => _serviceCode;
        init => _serviceCode = NormalizeServiceCode(value);
    }

    private static string NormalizeMunicipalityCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The municipality code must be provided.", nameof(value));
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 7)
        {
            throw new ArgumentException("The municipality code must contain seven numeric digits.", nameof(value));
        }

        return digits;
    }

    private static string NormalizeServiceCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The service code must be provided.", nameof(value));
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length != 6)
        {
            throw new ArgumentException("The service code must contain six numeric digits.", nameof(value));
        }

        return digits;
    }
}
