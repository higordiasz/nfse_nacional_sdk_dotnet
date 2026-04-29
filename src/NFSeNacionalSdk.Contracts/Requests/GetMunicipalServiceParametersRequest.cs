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

    public DateOnly CompetenceDate { get; init; } = DateOnly.FromDateTime(DateTime.Today);

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

        return string.Create(12, digits, static (destination, source) =>
        {
            source.AsSpan(0, 2).CopyTo(destination);
            destination[2] = '.';
            source.AsSpan(2, 2).CopyTo(destination[3..]);
            destination[5] = '.';
            source.AsSpan(4, 2).CopyTo(destination[6..]);
            destination[8] = '.';
            source.AsSpan(6, 3).CopyTo(destination[9..]);
        });
    }
}
