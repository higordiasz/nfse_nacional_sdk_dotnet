using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalMunicipalServiceParametersApiEnvelope
{
    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; set; }

    [JsonPropertyName("erros")]
    public IReadOnlyList<SefinNationalApiMessage>? Errors { get; set; }

    [JsonPropertyName("mensagem")]
    public string? Message { get; set; }

    [JsonPropertyName("aliquotas")]
    public IReadOnlyDictionary<string, IReadOnlyList<SefinNationalMunicipalServiceTaxRate>>? TaxRates { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }
}

internal sealed class SefinNationalMunicipalServiceTaxRate
{
    [JsonPropertyName("Incidencia")]
    public string? Incidence { get; set; }

    [JsonPropertyName("Aliq")]
    public decimal Rate { get; set; }

    [JsonPropertyName("DtIni")]
    public DateTimeOffset? ValidFrom { get; set; }

    [JsonPropertyName("DtFim")]
    public DateTimeOffset? ValidTo { get; set; }
}
