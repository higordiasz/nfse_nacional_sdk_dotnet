namespace NFSeNacionalSdk.Core.Enums;

public enum NFSeIbsCbsPurpose
{
    Regular = 0
}

public enum NFSeIbsCbsDestinationIndicator
{
    No = 0,
    Yes = 1
}

public enum NFSePisCofinsWithholdingType
{
    PisCofinsCsllNotWithheld = 0,
    PisCofinsWithheld = 1,
    PisCofinsNotWithheld = 2,
    PisCofinsCsllWithheld = 3,
    PisCofinsWithheldCsllNotWithheld = 4,
    PisWithheldCofinsCsllNotWithheld = 5,
    CofinsWithheldPisCsllNotWithheld = 6,
    PisNotWithheldCofinsCsllWithheld = 7,
    PisCofinsNotWithheldCsllWithheld = 8,
    CofinsNotWithheldPisCsllWithheld = 9
}
