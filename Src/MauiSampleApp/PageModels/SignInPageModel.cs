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

        [ObservableProperty]
        private bool _rememberMe;

        // Password visibility toggle
        [ObservableProperty]
        private bool _isPassword = true;

        [ObservableProperty]
        private FontImageSource _passwordToggleIcon = new()
        {
            Glyph = FluentUI.eye_off_24_regular,   // closed eye glyph
            FontFamily = FluentUI.FontFamily,
            Color = Colors.Gray,
            Size = 24
        };

        [RelayCommand]
        private void TogglePassword()
        {
            IsPassword = !IsPassword;

            PasswordToggleIcon = new()
            {
                Glyph = IsPassword ? FluentUI.eye_off_24_regular : FluentUI.eye_24_regular,
                FontFamily = FluentUI.FontFamily,
                Color = Colors.Gray,
                Size = 24
            };
        }

        [RelayCommand]
        private void SignUp()
        {
        }

        [RelayCommand]
        private async Task SignIn(CancellationToken cancellationToken = default)
        {
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

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                currentWindow.Page = serviceProvider.GetRequiredService<AppShell>();

                AppShell.DisplayToastAsync("Sign in successful").FireAndForgetSafeAsync(errorHandler);
            }
        }
    }
}
