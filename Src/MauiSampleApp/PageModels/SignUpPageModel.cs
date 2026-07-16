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

        [RelayCommand]
        private async Task SignIn()
        {
            // Wipe local session indicators
            sessionService.CurrentUserId = null;
            Preferences.Default.Set("IsUserLoggedIn", false);

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                var loginPage = serviceProvider.GetRequiredService<SignInPage>();

                // This tears down the entire AppShell and its navigation stack from memory
                currentWindow.Page = loginPage;
            }
        }

        [RelayCommand]
        private async Task SignUp(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(UserName) ||
                string.IsNullOrWhiteSpace(ConfirmUserName) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                errorHandler.HandleError(new Exception("All fields are required. Please fill out the entire form."));
                return;
            }

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
            Preferences.Default.Set("CurrentUserId", newUser.ID.ToString());

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                currentWindow.Page = serviceProvider.GetRequiredService<AppShell>();

                AppShell.DisplayToastAsync("Sign up successful").FireAndForgetSafeAsync(errorHandler);
            }
        }
    }
}
