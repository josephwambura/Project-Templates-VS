using System.Security.Cryptography;
using System.Text;

namespace MauiSampleApp.Services;

public static class HashHelper
{
    public static string HashData(string input)
    {
        ReadOnlySpan<byte> utf8Bytes = Encoding.UTF8.GetBytes(input);

        byte[] bytes = Constants.HashAlgorithm switch
        {
            var alg when alg == HashAlgorithmName.SHA1 => SHA1.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.SHA256 => SHA256.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.SHA384 => SHA384.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.SHA512 => SHA512.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.MD5 => MD5.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.SHA3_256 => SHA3_256.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.SHA3_384 => SHA3_384.HashData(utf8Bytes),
            var alg when alg == HashAlgorithmName.SHA3_512 => SHA3_512.HashData(utf8Bytes),
            _ => throw new NotSupportedException($"Unsupported algorithm: {Constants.HashAlgorithm}")
        };

        return Convert.ToHexStringLower(bytes);
    }

    public static string HashData(Stream stream)
    {
        byte[] bytes = Constants.HashAlgorithm switch
        {
            var alg when alg == HashAlgorithmName.SHA1 => SHA1.HashData(stream),
            var alg when alg == HashAlgorithmName.SHA256 => SHA256.HashData(stream),
            var alg when alg == HashAlgorithmName.SHA384 => SHA384.HashData(stream),
            var alg when alg == HashAlgorithmName.SHA512 => SHA512.HashData(stream),
            var alg when alg == HashAlgorithmName.MD5 => MD5.HashData(stream),
            var alg when alg == HashAlgorithmName.SHA3_256 => SHA3_256.HashData(stream),
            var alg when alg == HashAlgorithmName.SHA3_384 => SHA3_384.HashData(stream),
            var alg when alg == HashAlgorithmName.SHA3_512 => SHA3_512.HashData(stream),
            _ => throw new NotSupportedException($"Unsupported algorithm: {Constants.HashAlgorithm}")
        };

        return Convert.ToHexStringLower(bytes);
    }

    public static string HashFile(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return HashData(stream);
    }
}
