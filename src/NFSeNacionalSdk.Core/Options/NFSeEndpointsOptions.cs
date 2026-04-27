using NFSeNacionalSdk.Core.Enums;

namespace NFSeNacionalSdk.Core.Options;

public sealed class NFSeEndpointsOptions
{
    public NFSeEnvironment Environment { get; init; }

    public string BaseUrl { get; init; } = string.Empty;

    public string ParametrizationBaseUrl { get; init; } = string.Empty;

    public string MunicipalParametersByConventionPath => "/parametros_municipais/{codigoMunicipio}/convenio";
    public string MunicipalParametersByServiceCodePath => "/parametros_municipais/{codigoMunicipio}/{codigoServico}";
    public string NfsePath => "/nfse";
    public string NfseByAccessKeyPath => "/nfse/{chaveAcesso}";
    public string DpsByIdPath => "/dps/{id}";
    public string NfseEventsPath => "/nfse/{chaveAcesso}/eventos";
    public string NfseEventByTypePath => "/nfse/{chaveAcesso}/eventos/{tipoEvento}";
    public string NfseEventByTypeAndSequencePath => "/nfse/{chaveAcesso}/eventos/{tipoEvento}/{numSeqEvento}";

    public static NFSeEndpointsOptions For(NFSeEnvironment environment)
    {
        return environment switch
        {
            NFSeEnvironment.Production => new NFSeEndpointsOptions
            {
                Environment = environment,
                BaseUrl = "https://sefin.nfse.gov.br/SefinNacional/",
                ParametrizationBaseUrl = "https://adn.nfse.gov.br/parametrizacao/"
            },
            NFSeEnvironment.ProductionRestricted => new NFSeEndpointsOptions
            {
                Environment = environment,
                BaseUrl = "https://sefin.producaorestrita.nfse.gov.br/SefinNacional/",
                ParametrizationBaseUrl = "https://adn.producaorestrita.nfse.gov.br/parametrizacao/"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(environment), environment, null)
        };
    }
}
