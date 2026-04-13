using NFSeNacionalSdk.Contracts.Requests;

namespace NFSeNacionalSdk.Contracts.Serialization;

public interface INFSeSerializer
{
    string Serialize<T>(T value);

    T Deserialize<T>(string content);

    NFSeLookupDeserializationResult DeserializeLookupResponse(string content);

    EmitDpsSerializationResult SerializeSignedDps(
        EmitDpsRequest request,
        EmitDpsSerializationContext context);
}
