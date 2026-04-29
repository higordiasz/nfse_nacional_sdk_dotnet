using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using NFSeNacionalSdk.Contracts.Documents;
using NFSeNacionalSdk.Contracts.Responses;
using NFSeNacionalSdk.Contracts.Serialization;
using NFSeNacionalSdk.Core.Exceptions;
using NFSeNacionalSdk.Serialization.Xml.Lookup.Models;

namespace NFSeNacionalSdk.Serialization.Xml.Lookup;

internal sealed class NFSeLookupXmlResponseParser
{
    public NFSeLookupDeserializationResult Deserialize(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new NFSeSerializationException("XML content cannot be null or empty.");
        }

        try
        {
            var root = XDocument.Parse(content, LoadOptions.PreserveWhitespace).Root
                ?? throw new NFSeSerializationException("XML content does not contain a root element.");

            return root.Name.LocalName switch
            {
                "NFSe" => MapSuccess(
                    DeserializeXml<NFSeLookupSuccessEnvelopeXml>(content, root.Name.LocalName, root.Name.NamespaceName)),
                "ListaMensagemRetorno" => MapBusinessErrors(
                    DeserializeXml<NFSeLookupBusinessErrorEnvelopeXml>(content, root.Name.LocalName, root.Name.NamespaceName).Messages),
                "MensagemRetorno" => MapBusinessErrors(
                    [DeserializeXml<NFSeLookupBusinessErrorMessageXml>(content, root.Name.LocalName, root.Name.NamespaceName)]),
                _ => throw new NFSeSerializationException(
                    $"Unsupported NFSe lookup XML root '{root.Name.LocalName}'.")
            };
        }
        catch (NFSeSerializationException)
        {
            throw;
        }
        catch (Exception exception) when (exception is InvalidOperationException or XmlException)
        {
            throw new NFSeSerializationException("Failed to deserialize NFSe lookup XML content.", exception);
        }
    }

    private static NFSeLookupDeserializationResult MapSuccess(NFSeLookupSuccessEnvelopeXml envelope)
    {
        var info = envelope.Info ?? throw new NFSeSerializationException("NFSe lookup XML does not contain infNFSe.");
        var dps = info.Dps;
        var dpsInfo = dps?.Info;

        var issuer = MapParty(info.Issuer, dpsInfo?.Provider, dps?.LegacyProvider);
        var recipient = MapParty(dpsInfo?.Recipient, dps?.LegacyRecipient);
        var service = MapService(info, dps);

        var document = new NFSeDocument
        {
            AccessKey = ExtractAccessKey(info.Id),
            Number = TrimToNull(info.Number),
            DfseNumber = TrimToNull(info.DfseNumber),
            VerificationCode = null,
            IssuedAt = ParseDateTimeOffset(info.ProcessedAt)
                ?? ParseDateTimeOffset(dpsInfo?.IssuedAt)
                ?? ParseDateTimeOffset(dps?.LegacyIssuedAt),
            DpsIssuedAt = ParseDateTimeOffset(dpsInfo?.IssuedAt)
                ?? ParseDateTimeOffset(dps?.LegacyIssuedAt),
            CompetenceDate = ParseDateOnly(dpsInfo?.CompetenceDate ?? dps?.LegacyCompetenceDate),
            StatusCode = TrimToNull(info.StatusCode),
            ApplicationVersion = TrimToNull(info.ApplicationVersion),
            IssuingMunicipalityName = TrimToNull(info.IssuingMunicipalityName),
            ServiceLocationMunicipalityName = TrimToNull(info.ServiceLocationMunicipalityName),
            IncidenceMunicipalityCode = TrimToNull(info.IncidenceMunicipalityCode),
            IncidenceMunicipalityName = TrimToNull(info.IncidenceMunicipalityName),
            NationalTaxationDescription = TrimToNull(info.NationalTaxationDescription),
            DpsId = TrimToNull(dpsInfo?.Id) ?? TrimToNull(dps?.LegacyId),
            DpsSeries = TrimToNull(dpsInfo?.Series) ?? TrimToNull(dps?.LegacySeries),
            DpsNumber = TrimToNull(dpsInfo?.Number) ?? TrimToNull(dps?.LegacyNumber),
            NetAmount = ParseDecimal(info.Values?.NetAmount),
            Issuer = issuer,
            Recipient = recipient,
            Service = service
        };

        return new NFSeLookupDeserializationResult
        {
            Success = true,
            Document = document
        };
    }

    private static NFSeLookupDeserializationResult MapBusinessErrors(
        IReadOnlyCollection<NFSeLookupBusinessErrorMessageXml> messages)
    {
        var resolvedMessages = messages
            .Select(message => new NFSeMessage
            {
                Code = message.GetResolvedCode(),
                Description = message.GetResolvedDescription() ?? "The NFSe API returned a business error without description."
            })
            .ToArray();

        if (resolvedMessages.Length == 0)
        {
            throw new NFSeSerializationException("Business error XML did not contain any messages.");
        }

        return new NFSeLookupDeserializationResult
        {
            Success = false,
            Messages = resolvedMessages
        };
    }

    private static NFSeParty? MapParty(params NFSeLookupPartyXml?[] candidates)
    {
        var sources = candidates
            .Where(candidate => candidate is not null)
            .Cast<NFSeLookupPartyXml>()
            .ToArray();

        if (sources.Length == 0)
        {
            return null;
        }

        var party = new NFSeParty
        {
            Name = FirstValue(sources, source => source.Name),
            TaxId = FirstValue(sources, source => TrimToNull(source.Cnpj) ?? TrimToNull(source.Cpf)),
            MunicipalRegistration = FirstValue(sources, source => source.MunicipalRegistration),
            Phone = FirstValue(sources, source => source.Phone),
            Email = FirstValue(sources, source => source.Email),
            Address = sources
                .Select(source => MapAddress((NFSeLookupAddressXml?)source.NationalAddress ?? source.Address))
                .FirstOrDefault(address => address is not null)
        };

        return HasAnyValue(party.Name, party.TaxId, party.MunicipalRegistration, party.Phone, party.Email)
            || party.Address is not null
            ? party
            : null;
    }

    private static string? FirstValue<T>(IEnumerable<T> sources, Func<T, string?> selector)
    {
        return sources
            .Select(source => TrimToNull(selector(source)))
            .FirstOrDefault(value => value is not null);
    }

    private static NFSeAddress? MapAddress(NFSeLookupAddressXml? source)
    {
        if (source is null)
        {
            return null;
        }

        var address = new NFSeAddress
        {
            Street = TrimToNull(source.Street),
            Number = TrimToNull(source.Number),
            Complement = TrimToNull(source.Complement),
            Neighborhood = TrimToNull(source.Neighborhood),
            MunicipalityCode = TrimToNull(source.MunicipalityCode),
            State = TrimToNull(source.State),
            ZipCode = TrimToNull(source.ZipCode)
        };

        return HasAddressValue(address) ? address : null;
    }

    private static NFSeService? MapService(NFSeLookupInfoXml info, NFSeLookupDpsXml? dps)
    {
        var dpsInfo = dps?.Info;
        var serviceSource = dpsInfo?.Service ?? dps?.LegacyService;
        var valuesSource = dpsInfo?.Values ?? dps?.LegacyValues;

        if (serviceSource is null && valuesSource is null)
        {
            return null;
        }

        var service = new NFSeService
        {
            Description = TrimToNull(serviceSource?.Code?.Description)
                ?? TrimToNull(serviceSource?.LegacyDescription),
            ServiceCode = TrimToNull(serviceSource?.Code?.NationalTaxCode),
            MunicipalServiceCode = TrimToNull(serviceSource?.Code?.MunicipalTaxCode),
            NationalClassificationCode = TrimToNull(serviceSource?.Code?.NationalClassificationCode),
            InternalCode = TrimToNull(serviceSource?.Code?.InternalContributorCode),
            NationalTaxationDescription = TrimToNull(info.NationalTaxationDescription),
            LocationMunicipalityCode = TrimToNull(serviceSource?.Location?.MunicipalityCode)
                ?? TrimToNull(dpsInfo?.MunicipalityCode)
                ?? TrimToNull(dps?.LegacyMunicipalityCode),
            LocationMunicipalityName = TrimToNull(info.ServiceLocationMunicipalityName),
            ServiceAmount = ParseDecimal(valuesSource?.ServiceValues?.ServiceAmount)
                ?? ParseDecimal(info.Values?.NetAmount)
        };

        return HasAnyValue(
                service.Description,
                service.ServiceCode,
                service.MunicipalServiceCode,
                service.NationalClassificationCode,
                service.InternalCode,
                service.NationalTaxationDescription,
                service.LocationMunicipalityCode,
                service.LocationMunicipalityName)
            || service.ServiceAmount.HasValue
            ? service
            : null;
    }

    private static string? ExtractAccessKey(string? id)
    {
        var normalizedId = TrimToNull(id);
        if (normalizedId is null)
        {
            return null;
        }

        return normalizedId.StartsWith("NFS", StringComparison.OrdinalIgnoreCase) && normalizedId.Length > 3
            ? normalizedId[3..]
            : normalizedId;
    }

    private static decimal? ParseDecimal(string? rawValue)
    {
        var normalizedValue = TrimToNull(rawValue);
        if (normalizedValue is null)
        {
            return null;
        }

        if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        return decimal.TryParse(normalizedValue, NumberStyles.Number, new CultureInfo("pt-BR"), out var ptBrValue)
            ? ptBrValue
            : null;
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? rawValue)
    {
        var normalizedValue = TrimToNull(rawValue);
        if (normalizedValue is null)
        {
            return null;
        }

        return DateTimeOffset.TryParse(
            normalizedValue,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var result)
            ? result
            : null;
    }

    private static DateOnly? ParseDateOnly(string? rawValue)
    {
        var normalizedValue = TrimToNull(rawValue);
        if (normalizedValue is null)
        {
            return null;
        }

        return DateOnly.TryParse(
            normalizedValue,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var result)
            ? result
            : null;
    }

    private static T DeserializeXml<T>(string content, string rootName, string rootNamespace)
    {
        var serializer = new XmlSerializer(
            typeof(T),
            new XmlRootAttribute(rootName)
            {
                Namespace = rootNamespace
            });

        using var reader = new StringReader(content);
        var value = serializer.Deserialize(reader);

        if (value is not T typedValue)
        {
            throw new NFSeSerializationException(
                $"XML content for '{typeof(T).Name}' produced a null or incompatible result.");
        }

        return typedValue;
    }

    private static string? TrimToNull(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool HasAnyValue(params string?[] values)
    {
        return values.Any(value => !string.IsNullOrWhiteSpace(value));
    }

    private static bool HasAddressValue(NFSeAddress address)
    {
        return HasAnyValue(
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.MunicipalityCode,
            address.State,
            address.ZipCode);
    }
}
