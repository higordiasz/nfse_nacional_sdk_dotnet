using System.IO.Compression;
using System.Text;
using NFSeNacionalSdk.Core.Exceptions;

namespace NFSeNacionalSdk.SefinNational;

internal static class SefinNationalCompressedDocumentDecoder
{
    public static string DecodeGZipBase64(string compressedContent)
    {
        if (string.IsNullOrWhiteSpace(compressedContent))
        {
            throw new NFSeSerializationException("The SEFIN response did not contain a compressed NFSe XML payload.");
        }

        try
        {
            var bytes = Convert.FromBase64String(compressedContent);
            using var input = new MemoryStream(bytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();

            gzip.CopyTo(output);

            return Encoding.UTF8.GetString(output.ToArray());
        }
        catch (Exception exception) when (exception is FormatException or InvalidDataException)
        {
            throw new NFSeSerializationException(
                "Failed to decode the compressed NFSe XML returned by the SEFIN API.",
                exception);
        }
    }
}
