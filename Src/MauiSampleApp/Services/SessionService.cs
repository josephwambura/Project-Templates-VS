#if ANDROID
using static Android.Provider.Settings;
#endif

namespace MauiSampleApp.Services;

public class SessionService
{
    public Guid? CurrentUserId { get; set; }

    /// <summary>
    /// Generates a persistent unique hardware token fingerprint for device tracking
    /// </summary>
    public string GetCurrentDeviceIdentifier()
    {
        var deviceId = Preferences.Default.Get("AppDeviceUniqueIdentifier", string.Empty);
        if (string.IsNullOrWhiteSpace(deviceId))
        {
#if ANDROID
        var context = Platform.CurrentActivity;

        deviceId = Secure.GetString(context?.ContentResolver, Secure.AndroidId);
#else
            deviceId = Guid.CreateVersion7().ToString() ?? DeviceInfo.Current.Name;
#endif

            Preferences.Default.Set("AppDeviceUniqueIdentifier", deviceId);
        }
        return deviceId;
    }
}
