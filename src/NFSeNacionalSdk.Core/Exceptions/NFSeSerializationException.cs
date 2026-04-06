namespace NFSeNacionalSdk.Core.Exceptions;

public sealed class NFSeSerializationException : NFSeSdkException
{
    public NFSeSerializationException()
    {
    }

    public NFSeSerializationException(string message)
        : base(message)
    {
    }

    public NFSeSerializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}