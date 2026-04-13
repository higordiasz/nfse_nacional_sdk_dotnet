using System.IO.Compression;
using System.Text;
using NFSeNacionalSdk.Core.Exceptions;

namespace NFSeNacionalSdk.SefinNational;

internal static class SefinNationalCompressedDocumentEncoder
{
    public static string EncodeGZipBase64(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            throw new NFSeSerializationException("The XML payload to be compressed cannot be null or empty.");
        }

        try
        {
            var contentBytes = Encoding.UTF8.GetBytes(xmlContent);
            using var output = new MemoryStream();

            using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
            {
                gzip.Write(contentBytes, 0, contentBytes.Length);
            }

            return Convert.ToBase64String(output.ToArray());
        }
        catch (Exception exception) when (exception is InvalidDataException or EncoderFallbackException)
        {
            throw new NFSeSerializationException("Failed to compress the XML payload in the SEFIN GZip/base64 format.", exception);
        }
    }
}
