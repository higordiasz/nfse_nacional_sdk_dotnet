namespace NFSeNacionalSdk.Core.Exceptions;

public class NFSeSdkException : Exception
{
    public NFSeSdkException()
    {
    }

    public NFSeSdkException(string message)
        : base(message)
    {
    }

    public NFSeSdkException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}