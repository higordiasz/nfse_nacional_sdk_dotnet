namespace NFSeNacionalSdk.Contracts.Requests;

public sealed class GetMunicipalConventionRequest
{
    private string _municipalityCode = string.Empty;

    public required string MunicipalityCode
    {
        get => _municipalityCode;
        init => _municipalityCode = NormalizeMunicipalityCode(value);
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
}
