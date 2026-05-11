namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class GetMunicipalServiceParametersRequest
{
    private string _municipalityCode = string.Empty;
    private string _serviceCode = string.Empty;

    public string MunicipalityCode
    {
        get => _municipalityCode;
        set => _municipalityCode = NormalizeMunicipalityCode(value);
    }

    public string ServiceCode
    {
        get => _serviceCode;
        set => _serviceCode = NormalizeServiceCode(value);
    }

    public DateOnly CompetenceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

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
        if (digits.Length == 6)
        {
            digits += "000";
        }

        if (digits.Length != 9)
        {
            throw new ArgumentException(
                "The service code must contain six national taxation digits or nine municipal parameter digits.",
                nameof(value));
        }

        return string.Concat(
            digits.Substring(0, 2),
            ".",
            digits.Substring(2, 2),
            ".",
            digits.Substring(4, 2),
            ".",
            digits.Substring(6, 3));
    }
}
