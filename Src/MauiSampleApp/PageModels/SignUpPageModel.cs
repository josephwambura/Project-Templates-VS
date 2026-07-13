using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiSampleApp.PageModels
{
    public partial class SignUpPageModel(
    UserRepository userRepository,
    SessionService sessionService,
    IServiceProvider serviceProvider,
    IErrorHandler errorHandler) : ObservableObject
    {
        [ObservableProperty]
        private string _userName = string.Empty;
        [ObservableProperty]
        private string _confirmUserName = string.Empty;
        [ObservableProperty]
        private string _password = string.Empty;
        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        // Password visibility toggle
        [ObservableProperty]
        private bool _isPassword = true;

        [ObservableProperty]
        private bool _isConfirmPassword = true;

        [ObservableProperty]
        private FontImageSource _passwordToggleIcon = new()
        {
            Glyph = FluentUI.eye_off_24_regular,   // closed eye glyph
            FontFamily = FluentUI.FontFamily,
            Color = Colors.Gray,
            Size = 24
        };

        [ObservableProperty]
        private FontImageSource _confirmPasswordToggleIcon = new()
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
        private void ToggleConfirmPassword()
        {
            IsConfirmPassword = !IsConfirmPassword;

            ConfirmPasswordToggleIcon = new()
            {
                Glyph = IsPassword ? FluentUI.eye_off_24_regular : FluentUI.eye_24_regular,
                FontFamily = FluentUI.FontFamily,
                Color = Colors.Gray,
                Size = 24
            };
        }

        [RelayCommand]
        private void SignIn()
        {
        }

        [RelayCommand]
        private async Task SignUp(CancellationToken cancellationToken = default)
        {
            if (!string.Equals(UserName.Trim(), ConfirmUserName.Trim()))
            {
                errorHandler.HandleError(new Exception("The confirm username must match the username."));
                return;
            }

            if (!string.Equals(Password.Trim(), ConfirmPassword.Trim()))
            {
                errorHandler.HandleError(new Exception("The confirm password must match the password."));
                return;
            }

            var userExists = await userRepository.AnyByUserNameAsync(UserName.Trim(), cancellationToken);

            if (userExists)
            {
                errorHandler.HandleError(new Exception("That username is already taken."));
                return;
            }

            var newUser = new Models.ApplicationUser
            {
                Username = UserName.Trim(),
                UsernameNormalized = UserName.ToUpper().Trim(),
                PasswordHash = Password
            };

            // Bind current physical device during user account initialization
            newUser.Devices.Add(new Models.UserDevice
            {
                DeviceIdentifier = sessionService.GetCurrentDeviceIdentifier(),
                DeviceName = DeviceInfo.Current.Name,
                Model = DeviceInfo.Current.Model,
                Platform = DeviceInfo.Current.Platform.ToString(),
                IsEnabled = true
            });

            await userRepository.SaveItemAsync(newUser);

            sessionService.CurrentUserId = newUser.ID;

            Preferences.Default.Set("IsUserLoggedIn", true);

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                currentWindow.Page = serviceProvider.GetRequiredService<AppShell>();

                AppShell.DisplayToastAsync("Sign up successful").FireAndForgetSafeAsync(errorHandler);
            }
        }
    }
}
