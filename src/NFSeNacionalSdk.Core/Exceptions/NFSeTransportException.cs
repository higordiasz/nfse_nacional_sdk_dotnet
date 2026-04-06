namespace NFSeNacionalSdk.Core.Exceptions;

public sealed class NFSeTransportException : NFSeSdkException
{
    public NFSeTransportException()
    {
    }

    public NFSeTransportException(string message)
        : base(message)
    {
    }

    public NFSeTransportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}