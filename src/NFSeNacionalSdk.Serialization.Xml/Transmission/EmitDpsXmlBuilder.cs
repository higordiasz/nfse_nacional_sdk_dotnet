using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NFSeNacionalSdk.Contracts.Requests;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Enums;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml.Lookup;
using NFSeNacionalSdk.Serialization.Xml.Transmission.Models;

namespace NFSeNacionalSdk.Serialization.Xml.Transmission;

internal sealed class EmitDpsXmlBuilder
{
    private const int ApplicationVersionMaxLength = 20;

    private readonly EmitDpsXmlSigner _signer = new();
    private readonly EmitDpsXmlSchemaValidator _schemaValidator = new();

    public EmitDpsSerializationResult Build(
        EmitDpsRequest request,
        EmitDpsSerializationContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        if (context.SigningCertificate is null)
        {
            throw new NFSeSerializationException("A signing certificate is required to generate a DPS document.");
        }

        var provider = request.Provider
            ?? throw new NFSeSerializationException($"{nameof(request.Provider)} must be informed.");
        var service = request.Service
            ?? throw new NFSeSerializationException($"{nameof(request.Service)} must be informed.");
        var taxation = request.Taxation
            ?? throw new NFSeSerializationException($"{nameof(request.Taxation)} must be informed.");
        var providerTaxId = NormalizeTaxId(provider.TaxId, nameof(request.Provider.TaxId));
        var municipalityCode = NormalizeDigits(request.MunicipalityCode, 7, 7, nameof(request.MunicipalityCode));
        var series = NormalizeSeries(request.Series);
        var number = NormalizeNumber(request.Number);
        var dpsId = BuildDpsId(municipalityCode, providerTaxId, series, number);

        ValidateBusinessRules(provider, taxation);

        var envelope = new EmitDpsEnvelopeXml
        {
            Info = new EmitDpsInfoXml
            {
                Id = dpsId,
                EnvironmentType = ((int)context.Environment).ToString(CultureInfo.InvariantCulture),
                IssuedAt = request.IssuedAt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture),
                ApplicationVersion = NormalizeApplicationVersion(context.ApplicationVersion),
                Series = series,
                Number = number,
                CompetenceDate = request.CompetenceDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EmitterType = ((int)request.EmitterType).ToString(CultureInfo.InvariantCulture),
                MunicipalityCode = municipalityCode,
                Provider = BuildProvider(provider, providerTaxId),
                Recipient = request.Recipient is null ? null : BuildRecipient(request.Recipient),
                Service = BuildService(service, municipalityCode),
                Values = BuildValues(service, provider, taxation)
            }
        };

        var unsignedXml = SerializeUnsigned(envelope);
        var signedXml = _signer.Sign(unsignedXml, dpsId, context.SigningCertificate);
        _schemaValidator.Validate(signedXml);

        return new EmitDpsSerializationResult
        {
            DpsId = dpsId,
            XmlContent = signedXml
        };
    }

    private static EmitDpsProviderXml BuildProvider(EmitDpsProvider provider, NormalizedTaxId taxId)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var providerXml = new EmitDpsProviderXml
        {
            MunicipalRegistration = NormalizeOptionalText(provider.MunicipalRegistration),
            Name = NormalizeOptionalText(provider.Name),
            Address = provider.Address is null ? null : BuildAddress(provider.Address),
            Phone = NormalizeOptionalDigits(provider.Phone),
            Email = NormalizeOptionalText(provider.Email),
            TaxRegime = new EmitDpsProviderTaxRegimeXml
            {
                SimplesNationalOption = ((int)provider.SimplesNationalOption).ToString(CultureInfo.InvariantCulture),
                SimplifiedNationalTaxRegime = provider.SimplifiedNationalTaxRegime is null
                    ? null
                    : ((int)provider.SimplifiedNationalTaxRegime.Value).ToString(CultureInfo.InvariantCulture),
                SpecialTaxRegime = ((int)provider.SpecialTaxRegime).ToString(CultureInfo.InvariantCulture)
            }
        };

        ApplyTaxId(providerXml, taxId);
        return providerXml;
    }

    private static EmitDpsPersonXml BuildRecipient(EmitDpsRecipient recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        var taxId = NormalizeTaxId(recipient.TaxId, nameof(recipient.TaxId));
        var recipientXml = new EmitDpsPersonXml
        {
            Name = EnsureNotWhiteSpace(recipient.Name, nameof(recipient.Name)),
            MunicipalRegistration = NormalizeOptionalText(recipient.MunicipalRegistration),
            Address = recipient.Address is null ? null : BuildAddress(recipient.Address),
            Phone = NormalizeOptionalDigits(recipient.Phone),
            Email = NormalizeOptionalText(recipient.Email)
        };

        ApplyTaxId(recipientXml, taxId);
        return recipientXml;
    }

    private static EmitDpsAddressXml BuildAddress(EmitDpsAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        return new EmitDpsAddressXml
        {
            NationalAddress = new EmitDpsNationalAddressXml
            {
                MunicipalityCode = NormalizeDigits(address.MunicipalityCode, 7, 7, nameof(address.MunicipalityCode)),
                ZipCode = NormalizeDigits(address.ZipCode, 8, 8, nameof(address.ZipCode))
            },
            Street = EnsureNotWhiteSpace(address.Street, nameof(address.Street)),
            Number = EnsureNotWhiteSpace(address.Number, nameof(address.Number)),
            Complement = NormalizeOptionalText(address.Complement),
            Neighborhood = EnsureNotWhiteSpace(address.Neighborhood, nameof(address.Neighborhood))
        };
    }

    private static EmitDpsServiceXml BuildService(EmitDpsService service, string defaultMunicipalityCode)
    {
        ArgumentNullException.ThrowIfNull(service);

        return new EmitDpsServiceXml
        {
            Location = new EmitDpsServiceLocationXml
            {
                MunicipalityCode = string.IsNullOrWhiteSpace(service.ServiceLocationMunicipalityCode)
                    ? defaultMunicipalityCode
                    : NormalizeDigits(service.ServiceLocationMunicipalityCode, 7, 7, nameof(service.ServiceLocationMunicipalityCode))
            },
            Code = new EmitDpsServiceCodeXml
            {
                NationalTaxationCode = NormalizeDigits(service.NationalTaxationCode, 6, 6, nameof(service.NationalTaxationCode)),
                MunicipalTaxationCode = NormalizeOptionalText(service.MunicipalTaxationCode),
                Description = EnsureNotWhiteSpace(service.Description, nameof(service.Description)),
                NationalClassificationCode = NormalizeOptionalDigits(service.NationalClassificationCode, 9, 9, nameof(service.NationalClassificationCode)),
                InternalCode = NormalizeOptionalText(service.InternalCode)
            }
        };
    }

    private static EmitDpsValuesXml BuildValues(
        EmitDpsService service,
        EmitDpsProvider provider,
        EmitDpsTaxation taxation)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(taxation);

        if (service.Amount <= 0)
        {
            throw new NFSeSerializationException("Service amount must be greater than zero.");
        }

        return new EmitDpsValuesXml
        {
            ServiceValues = new EmitDpsServiceValuesXml
            {
                ReceivedAmount = FormatOptionalAmount(
                    service.AmountReceivedByIntermediary,
                    nameof(service.AmountReceivedByIntermediary)),
                Amount = FormatNonNegativeDecimal(
                    service.Amount,
                    nameof(service.Amount),
                    maximumExclusive: 1_000_000_000_000_000m)
            },
            DiscountValues = BuildDiscountValues(service),
            Taxation = new EmitDpsTaxationXml
            {
                MunicipalTaxation = new EmitDpsMunicipalTaxationXml
                {
                    IssTaxationType = ((int)taxation.IssTaxationType).ToString(CultureInfo.InvariantCulture),
                    IssWithholdingType = ((int)taxation.IssWithholdingType).ToString(CultureInfo.InvariantCulture),
                    IssRate = FormatOptionalIssRate(taxation.IssRate, nameof(taxation.IssRate))
                },
                TotalTax = BuildTotalTax(provider, taxation)
            }
        };
    }

    private static EmitDpsTotalTaxXml BuildTotalTax(EmitDpsProvider provider, EmitDpsTaxation taxation)
    {
        if (provider.SimplesNationalOption == NFSeSimplesNationalOption.MicroOrSmallBusiness)
        {
            if (taxation.TotalTaxIndicator is not null)
            {
                throw new NFSeSerializationException(
                    "taxation.TotalTaxIndicator must be omitted for provider.SimplesNationalOption MicroOrSmallBusiness (3). " +
                    "Set taxation.totalTaxIndicator to null and inform taxation.simplesNationalTotalTaxRate.");
            }

            if (taxation.SimplesNationalTotalTaxRate is null)
            {
                throw new NFSeSerializationException(
                    "taxation.SimplesNationalTotalTaxRate must be informed for provider.SimplesNationalOption MicroOrSmallBusiness (3).");
            }

            return new EmitDpsTotalTaxXml
            {
                SimplesNationalRate = FormatNonNegativeDecimal(
                    taxation.SimplesNationalTotalTaxRate.Value,
                    nameof(taxation.SimplesNationalTotalTaxRate),
                    maximumExclusive: 100m)
            };
        }

        if (taxation.SimplesNationalTotalTaxRate is not null)
        {
            throw new NFSeSerializationException(
                "taxation.SimplesNationalTotalTaxRate can only be informed for provider.SimplesNationalOption MicroOrSmallBusiness (3).");
        }

        if (taxation.TotalTaxIndicator is null)
        {
            throw new NFSeSerializationException(
                "taxation.TotalTaxIndicator must be informed when provider.SimplesNationalOption is not MicroOrSmallBusiness (3).");
        }

        return new EmitDpsTotalTaxXml
        {
            Indicator = ((int)taxation.TotalTaxIndicator.Value).ToString(CultureInfo.InvariantCulture)
        };
    }

    private static void ValidateBusinessRules(EmitDpsProvider provider, EmitDpsTaxation taxation)
    {
        if (provider.SimplesNationalOption == NFSeSimplesNationalOption.MicroOrSmallBusiness &&
            provider.SimplifiedNationalTaxRegime == NFSeSimplifiedNationalTaxRegime.FederalAndMunicipalTaxesInSimplesNational &&
            taxation.IssWithholdingType == NFSeIssWithholdingType.NotWithheld &&
            taxation.IssRate is not null)
        {
            throw new NFSeSerializationException(
                "taxation.IssRate must be omitted when provider.SimplesNationalOption is MicroOrSmallBusiness (3), " +
                "provider.SimplifiedNationalTaxRegime is FederalAndMunicipalTaxesInSimplesNational (1), and " +
                "taxation.IssWithholdingType is NotWithheld (1). Set taxation.issRate to null.");
        }
    }

    private static EmitDpsDiscountValuesXml? BuildDiscountValues(EmitDpsService service)
    {
        var unconditionalAmount = FormatOptionalAmount(
            service.UnconditionalDiscountAmount,
            nameof(service.UnconditionalDiscountAmount));
        var conditionalAmount = FormatOptionalAmount(
            service.ConditionalDiscountAmount,
            nameof(service.ConditionalDiscountAmount));

        return unconditionalAmount is null && conditionalAmount is null
            ? null
            : new EmitDpsDiscountValuesXml
            {
                UnconditionalAmount = unconditionalAmount,
                ConditionalAmount = conditionalAmount
            };
    }

    private static string SerializeUnsigned(EmitDpsEnvelopeXml envelope)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(EmitDpsEnvelopeXml));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, NFSeLookupXmlNamespace.SpedNFSe);

            using var stream = new MemoryStream();
            using (var writer = XmlWriter.Create(
                       stream,
                       new XmlWriterSettings
                       {
                           Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                           Indent = false,
                           OmitXmlDeclaration = false
                       }))
            {
                serializer.Serialize(writer, envelope, namespaces);
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }
        catch (InvalidOperationException exception)
        {
            throw new NFSeSerializationException("Failed to serialize the DPS XML document.", exception);
        }
    }

    private static string BuildDpsId(
        string municipalityCode,
        NormalizedTaxId providerTaxId,
        string series,
        string number)
    {
        var paddedTaxId = providerTaxId.Digits.PadLeft(14, '0');
        var paddedSeries = series.PadLeft(5, '0');
        var paddedNumber = number.PadLeft(15, '0');

        return string.Create(
            45,
            (municipalityCode, providerTaxId, paddedTaxId, paddedSeries, paddedNumber),
            static (span, state) =>
            {
                "DPS".AsSpan().CopyTo(span);
                state.municipalityCode.AsSpan().CopyTo(span[3..]);
                state.providerTaxId.TypeCode.AsSpan().CopyTo(span[10..]);
                state.paddedTaxId.AsSpan().CopyTo(span[11..]);
                state.paddedSeries.AsSpan().CopyTo(span[25..]);
                state.paddedNumber.AsSpan().CopyTo(span[30..]);
            });
    }

    private static void ApplyTaxId(EmitDpsProviderXml destination, NormalizedTaxId taxId)
    {
        if (taxId.IsCnpj)
        {
            destination.Cnpj = taxId.Digits;
        }
        else
        {
            destination.Cpf = taxId.Digits;
        }
    }

    private static void ApplyTaxId(EmitDpsPersonXml destination, NormalizedTaxId taxId)
    {
        if (taxId.IsCnpj)
        {
            destination.Cnpj = taxId.Digits;
        }
        else
        {
            destination.Cpf = taxId.Digits;
        }
    }

    private static string NormalizeSeries(string? value)
    {
        var digits = NormalizeDigits(value, 1, 5, nameof(EmitDpsRequest.Series));

        if (digits.Length > 5)
        {
            throw new NFSeSerializationException("DPS series must contain up to five numeric digits.");
        }

        return digits;
    }

    private static string NormalizeNumber(string? value)
    {
        var digits = NormalizeDigits(value, 1, 15, nameof(EmitDpsRequest.Number));

        if (digits[0] == '0')
        {
            throw new NFSeSerializationException("DPS number must not start with zero.");
        }

        return digits;
    }

    private static NormalizedTaxId NormalizeTaxId(string? value, string parameterName)
    {
        var digits = NormalizeDigits(value, 11, 14, parameterName);

        return digits.Length switch
        {
            11 => new NormalizedTaxId(digits, false, "1"),
            14 => new NormalizedTaxId(digits, true, "2"),
            _ => throw new NFSeSerializationException($"{parameterName} must contain either 11 digits (CPF) or 14 digits (CNPJ).")
        };
    }

    private static string NormalizeDigits(string? value, int minLength, int maxLength, string parameterName)
    {
        var digits = new string(EnsureNotWhiteSpace(value, parameterName).Where(char.IsDigit).ToArray());

        if (digits.Length < minLength || digits.Length > maxLength)
        {
            throw new NFSeSerializationException(
                $"{parameterName} must contain between {minLength} and {maxLength} numeric digits.");
        }

        return digits;
    }

    private static string? NormalizeOptionalDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    private static string? NormalizeOptionalDigits(string? value, int minLength, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizeDigits(value, minLength, maxLength, parameterName);
    }

    private static string? FormatOptionalAmount(decimal? value, string parameterName)
    {
        if (value is null)
        {
            return null;
        }

        return FormatNonNegativeDecimal(value.Value, parameterName, maximumExclusive: 1_000_000_000_000_000m);
    }

    private static string? FormatOptionalIssRate(decimal? value, string parameterName)
    {
        if (value is null)
        {
            return null;
        }

        return FormatNonNegativeDecimal(value.Value, parameterName, maximumExclusive: 10m);
    }

    private static string FormatNonNegativeDecimal(
        decimal value,
        string parameterName,
        decimal maximumExclusive)
    {
        if (value < 0)
        {
            throw new NFSeSerializationException($"{parameterName} must be greater than or equal to zero.");
        }

        if (value >= maximumExclusive)
        {
            throw new NFSeSerializationException($"{parameterName} exceeds the maximum value accepted by the DPS schema.");
        }

        return decimal
            .Round(value, 2, MidpointRounding.AwayFromZero)
            .ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string EnsureNotWhiteSpace(string? value, string parameterName)
    {
        var normalized = NormalizeOptionalText(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new NFSeSerializationException($"{parameterName} must be informed.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string NormalizeApplicationVersion(string? value)
    {
        var normalized = NormalizeOptionalText(value) ?? "NFSeNacionalSdk";

        return normalized.Length <= ApplicationVersionMaxLength
            ? normalized
            : normalized[..ApplicationVersionMaxLength];
    }

    private readonly record struct NormalizedTaxId(string Digits, bool IsCnpj, string TypeCode);
}
