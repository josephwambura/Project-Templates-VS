namespace MauiSampleApp.Services;

public static class SecureStorageHelper
{
    public static async Task<T?> GetDecryptedAsync<T>(string key, string uniqueId)
    {
        var decrypted = await GetDecryptedAsync(key, uniqueId);
        return decrypted.FromJson<T>();
    }

    public static async Task<string?> GetDecryptedAsync(string key, string uniqueId)
    {
        var deviceSpec = $"{DeviceInfo.Current.Platform}{DeviceInfo.Current.Idiom}{DeviceInfo.Current.DeviceType}{AppInfo.Current.PackageName}";
        var encKey = HashHelper.HashData($"{deviceSpec}{uniqueId}");

        var encryptedValue = await SecureStorage.GetAsync(key);
        if (string.IsNullOrWhiteSpace(encryptedValue))
            return null;

        return EncryptionHelper.Decrypt(encryptedValue, encKey);
    }

    public static Task SetEncryptedAsync(string key, object value, string uniqueId)
        => SetEncryptedAsync(key, value.ToJson()!, uniqueId);

    public static async Task SetEncryptedAsync(string key, string value, string uniqueId)
    {
        var deviceSpec = $"{DeviceInfo.Current.Platform}{DeviceInfo.Current.Idiom}{DeviceInfo.Current.DeviceType}{AppInfo.Current.PackageName}";
        var encKey = HashHelper.HashData($"{deviceSpec}{uniqueId}");

        var encryptedValue = EncryptionHelper.Encrypt(value, encKey);
        if (string.IsNullOrWhiteSpace(encryptedValue))
            return;

        await SecureStorage.SetAsync(key, encryptedValue);
    }
}

