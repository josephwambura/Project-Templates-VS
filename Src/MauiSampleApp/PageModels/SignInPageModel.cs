using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiSampleApp.PageModels
{
    public partial class SignInPageModel(
    UserRepository userRepository,
    UserDeviceRepository userDeviceRepository,
    SessionService sessionService,
    IServiceProvider serviceProvider,
    IErrorHandler errorHandler) : ObservableObject
    {
        [ObservableProperty]
        private string _userName = string.Empty;
        [ObservableProperty]
        private string _password = string.Empty;

        [RelayCommand]
        private async Task SignUp()
        {
            // Wipe local session indicators
            sessionService.CurrentUserId = null;
            Preferences.Default.Set("IsUserLoggedIn", false);
            Preferences.Default.Remove("CurrentUserId");

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                var loginPage = serviceProvider.GetRequiredService<SignUpPage>();

                // This tears down the entire AppShell and its navigation stack from memory
                currentWindow.Page = loginPage;
            }
        }

        [RelayCommand]
        private async Task SignIn(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
            {
                errorHandler.HandleError(new Exception("Please enter both your username and password."));
                return;
            }

            var user = await userRepository.GetByUserNameAsync(UserName.Trim(), cancellationToken);

            if (user == null || user.PasswordHash != Password)
            {
                errorHandler.HandleError(new Exception("Invalid credentials. Please verify your username and password."));
                return;
            }

            string currentDeviceToken = sessionService.GetCurrentDeviceIdentifier();
            var deviceBinding = user.Devices.FirstOrDefault(d => d.DeviceIdentifier == currentDeviceToken);

            if (deviceBinding == null)
            {
                // Unrecognized device logging in for the first time -> Auto Bind It
                deviceBinding = new Models.UserDevice
                {
                    DeviceIdentifier = currentDeviceToken,
                    DeviceName = DeviceInfo.Current.Name,
                    Model = DeviceInfo.Current.Model,
                    Platform = DeviceInfo.Current.Platform.ToString(),
                    IsEnabled = true
                };

                await userDeviceRepository.SaveItemAsync(deviceBinding);
            }
            else if (!deviceBinding.IsEnabled)
            {
                // Block authentication if hardware fingerprint was deactivated remotely
                errorHandler.HandleError(new Exception("Security Alert: This specific device has been disabled. Authorize it using an enabled console."));
                return;
            }

            sessionService.CurrentUserId = user.ID;

            Preferences.Default.Set("IsUserLoggedIn", true);
            Preferences.Default.Set("CurrentUserId", user.ID.ToString());

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                currentWindow.Page = serviceProvider.GetRequiredService<AppShell>();

                AppShell.DisplayToastAsync("Sign in successful").FireAndForgetSafeAsync(errorHandler);
            }
        }
    }
}
