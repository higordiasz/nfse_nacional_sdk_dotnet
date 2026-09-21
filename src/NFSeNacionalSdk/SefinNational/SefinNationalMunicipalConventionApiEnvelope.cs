using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFSeNacionalSdk.SefinNational;

internal sealed class SefinNationalMunicipalConventionApiEnvelope
{
    [JsonPropertyName("erro")]
    public SefinNationalApiMessage? Error { get; set; }

    [JsonPropertyName("erros")]
    public IReadOnlyList<SefinNationalApiMessage>? Errors { get; set; }

    [JsonPropertyName("mensagem")]
    public string? Message { get; set; }

    [JsonPropertyName("parametrosConvenio")]
    public SefinNationalMunicipalConventionParameters? Parameters { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }
}

internal sealed class SefinNationalMunicipalConventionParameters
{
    [JsonPropertyName("tipoConvenioDeserializationSetter")]
    public int? ConventionType { get; set; }

    [JsonPropertyName("aderenteAmbienteNacional")]
    public int? UsesNationalEnvironment { get; set; }

    [JsonPropertyName("aderenteEmissorNacional")]
    public int? UsesNationalIssuer { get; set; }

    [JsonPropertyName("situacaoEmissaoPadraoContribuintesRFB")]
    public int? DefaultFederalTaxpayerIssuanceStatus { get; set; }

    [JsonPropertyName("aderenteMAN")]
    public int? UsesNationalSupportModule { get; set; }

    [JsonPropertyName("permiteAproveitametoDeCreditos")]
    public bool? AllowsTaxCredits { get; set; }
}
