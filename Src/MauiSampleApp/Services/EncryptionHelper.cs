using System.Security.Cryptography;
using System.Text;

namespace MauiSampleApp.Services;

public class EncryptionHelper
{
    /// <summary>
    /// Uses Rfc2898DeriveBytes class to derive a strong key from the provided key and a randomly generated IV.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? Encrypt(string? input, string key)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var contentBytes = Encoding.UTF8.GetBytes(input);

        using Aes aes = Aes.Create();
        aes.GenerateIV();
        var deriveBytes = Rfc2898DeriveBytes.Pbkdf2(key, aes.IV, Constants.Pbkdf2Iterations, Constants.HashAlgorithm, 32);
        aes.Key = deriveBytes;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        msEncrypt.Write(aes.IV, 0, aes.IV.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(contentBytes, 0, contentBytes.Length);
            csEncrypt.FlushFinalBlock();
        }

        var encryptedData = msEncrypt.ToArray();

        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// Uses Rfc2898DeriveBytes class to derive a strong key from the provided key and a randomly generated IV.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? Encrypt<T>(T? input, string key) => Encrypt(input.ToJson(), key);

    /// <summary>
    /// Uses Rfc2898DeriveBytes class to derive a strong key from the provided key and the IV extracted from the encrypted data.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string? Decrypt(string? input, string key)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var encryptedBytes = Convert.FromBase64String(input);

        using Aes aes = Aes.Create();
        byte[] iv = new byte[aes.IV.Length];
        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, iv.Length);
        var deriveBytes = Rfc2898DeriveBytes.Pbkdf2(key, iv, Constants.Pbkdf2Iterations, Constants.HashAlgorithm, 32);
        aes.Key = deriveBytes;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    /// <summary>
    /// Uses Rfc2898DeriveBytes class to derive a strong key from the provided key and the IV extracted from the encrypted data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static T? Decrypt<T>(string? input, string key) => Decrypt(input, key).FromJson<T>();
}
