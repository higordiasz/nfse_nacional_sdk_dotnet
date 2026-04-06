namespace NFSeNacionalSdk.Contracts.Serialization;

public interface INFSeSerializer
{
    string Serialize<T>(T value);

    T Deserialize<T>(string content);
}