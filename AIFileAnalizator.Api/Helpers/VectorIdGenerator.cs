using System.Security.Cryptography;
using System.Text;

namespace AIFileAnalizator.Api.Helpers;
public class VectorIdGenerator
{
    public static string NewGuidId() => Guid.NewGuid().ToString();

    public static string FromText(string text)
    {
        using var sha1 = SHA1.Create();
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = sha1.ComputeHash(bytes);           
        return BitConverter.ToString(hash);
    }
 
    public static string FromFileChunk(string filename, int chunkIndex)
    {
        var input = $"{filename}-{chunkIndex}";
        return FromText(input);
    }

}

