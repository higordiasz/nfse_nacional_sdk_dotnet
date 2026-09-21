using NFSeNacionalSdk.Core.Exceptions;

namespace NFSeNacionalSdk.Serialization.Xml.Layout;

internal readonly struct BrazilianTaxId
{
    private BrazilianTaxId(string value, bool isCnpj)
    {
        Value = value;
        IsCnpj = isCnpj;
    }

    public string Value { get; }

    public bool IsCnpj { get; }

    public string TypeCode => IsCnpj ? "2" : "1";

    public static BrazilianTaxId Parse(string? rawValue, string parameterName, NFSeLayoutProfileInfo profile)
    {
        var value = Normalize(rawValue, parameterName);

        if (value.Length == 11 && value.All(IsAsciiDigit))
        {
            if (!HasValidCpfCheckDigits(value))
            {
                throw new NFSeSerializationException($"{parameterName} contains an invalid CPF check digit.");
            }

            return new BrazilianTaxId(value, isCnpj: false);
        }

        if (value.Length != 14 || !value.All(IsAsciiUpperAlphaNumeric))
        {
            throw new NFSeSerializationException(
                $"{parameterName} must contain a valid 11-digit CPF or 14-character CNPJ.");
        }

        if (value.Any(character => character is >= 'A' and <= 'Z') && !profile.SupportsAlphanumericCnpj)
        {
            throw new NFSeSerializationException(
                $"{parameterName} contains an alphanumeric CNPJ, which is incompatible with layout profile {profile.Profile}.");
        }

        if (!HasValidCnpjCheckDigits(value))
        {
            throw new NFSeSerializationException($"{parameterName} contains an invalid CNPJ check digit.");
        }

        return new BrazilianTaxId(value, isCnpj: true);
    }

    private static string Normalize(string? rawValue, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new NFSeSerializationException($"{parameterName} must be informed.");
        }

        var characters = new List<char>(rawValue!.Length);
        foreach (var rawCharacter in rawValue!.Trim().ToUpperInvariant())
        {
            if (rawCharacter is '.' or '/' or '-' || char.IsWhiteSpace(rawCharacter))
            {
                continue;
            }

            if (!IsAsciiUpperAlphaNumeric(rawCharacter))
            {
                throw new NFSeSerializationException($"{parameterName} contains an unsupported character '{rawCharacter}'.");
            }

            characters.Add(rawCharacter);
        }

        return new string(characters.ToArray());
    }

    private static bool HasValidCpfCheckDigits(string value)
    {
        if (value.Distinct().Count() == 1)
        {
            return false;
        }

        var first = CalculateCpfDigit(value, 9, 10);
        var second = CalculateCpfDigit(value, 10, 11);
        return value[9] - '0' == first && value[10] - '0' == second;
    }

    private static int CalculateCpfDigit(string value, int length, int initialWeight)
    {
        var sum = 0;
        for (var index = 0; index < length; index++)
        {
            sum += (value[index] - '0') * (initialWeight - index);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static bool HasValidCnpjCheckDigits(string value)
    {
        var first = CalculateCnpjDigit(value, 12, 5);
        var second = CalculateCnpjDigit(value, 13, 6);
        return value[12] - '0' == first && value[13] - '0' == second;
    }

    private static int CalculateCnpjDigit(string value, int length, int initialWeight)
    {
        var sum = 0;
        var weight = initialWeight;
        for (var index = 0; index < length; index++)
        {
            sum += (value[index] - '0') * weight;
            weight = weight == 2 ? 9 : weight - 1;
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static bool IsAsciiDigit(char value) => value is >= '0' and <= '9';

    private static bool IsAsciiUpperAlphaNumeric(char value) =>
        IsAsciiDigit(value) || value is >= 'A' and <= 'Z';
}
