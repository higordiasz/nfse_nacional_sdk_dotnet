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

        var issuer = MapParty(info.Issuer, info.Dps?.Provider);
        var recipient = MapParty(info.Dps?.Recipient);
        var service = MapService(info.Dps);

        var document = new NFSeDocument
        {
            AccessKey = ExtractAccessKey(info.Id),
            Number = TrimToNull(info.Number),
            VerificationCode = null,
            IssuedAt = ParseDateTimeOffset(info.ProcessedAt) ?? ParseDateTimeOffset(info.Dps?.IssuedAt),
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

    private static NFSeLookupPartyXml? Prefer(params NFSeLookupPartyXml?[] candidates)
    {
        return candidates.FirstOrDefault(candidate => candidate is not null);
    }

    private static NFSeParty? MapParty(params NFSeLookupPartyXml?[] candidates)
    {
        var source = Prefer(candidates);
        if (source is null)
        {
            return null;
        }

        var party = new NFSeParty
        {
            Name = TrimToNull(source.Name),
            TaxId = TrimToNull(source.Cnpj) ?? TrimToNull(source.Cpf),
            MunicipalRegistration = TrimToNull(source.MunicipalRegistration),
            Email = TrimToNull(source.Email)
        };

        return HasAnyValue(party.Name, party.TaxId, party.MunicipalRegistration, party.Email)
            ? party
            : null;
    }

    private static NFSeService? MapService(NFSeLookupDpsXml? dps)
    {
        if (dps?.Service is null && dps?.Values is null)
        {
            return null;
        }

        var service = new NFSeService
        {
            Description = TrimToNull(dps?.Service?.Description),
            ServiceCode = TrimToNull(dps?.Service?.Code?.NationalTaxCode),
            ServiceAmount = ParseDecimal(dps?.Values?.ServiceValues?.ServiceAmount)
        };

        return HasAnyValue(service.Description, service.ServiceCode) || service.ServiceAmount.HasValue
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
}
