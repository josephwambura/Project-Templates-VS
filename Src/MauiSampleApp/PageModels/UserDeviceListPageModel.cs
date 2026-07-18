using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Models;

namespace MauiSampleApp.PageModels
{
    public partial class UserDeviceListPageModel(UserDeviceRepository userDeviceRepository, IErrorHandler errorHandler, ISecurityService securityService,
        SessionService sessionService, IServiceProvider serviceProvider) : ObservableObject, IUserDevicePageModel
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;

        [ObservableProperty]
        private List<UserDevice> _devices = [];

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _isRefreshing;

        [ObservableProperty]
        private string _today = DateTimeOffset.UtcNow.ToString("dddd, MMM d");

        [ObservableProperty]
        private UserDevice? selectedDevice;

        public bool HasDisabledDevices
            => Devices?.Any(t => !t.IsEnabled) ?? false;

        private async Task LoadData()
        {
            try
            {
                IsBusy = true;

                Devices = await userDeviceRepository.ListByApplicationUserIdAsync(sessionService.CurrentUserId.Value);
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(HasDisabledDevices));
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            try
            {
                IsRefreshing = true;
                await LoadData();
            }
            catch (Exception e)
            {
                errorHandler.HandleError(e);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void NavigatedTo() =>
            _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() =>
            _isNavigatedTo = false;

        [RelayCommand]
        private async Task Appearing()
        {
            if (!_dataLoaded)
            {
                await Refresh();
                _dataLoaded = true;
                await Refresh();
            }
            // This means we are being navigated to
            else if (!_isNavigatedTo)
            {
                await Refresh();
            }
        }

        [RelayCommand]
        private Task DeviceDisabled(UserDevice device)
        {
            OnPropertyChanged(nameof(HasDisabledDevices));
            return userDeviceRepository.SaveItemAsync(device);
        }

        [RelayCommand]
        private Task NavigateToDevice(UserDevice device)
            => Shell.Current.GoToAsync($"device?id={device.ID}");

        [RelayCommand]
        private async Task CleanDevices()
        {
            var currentUserId = sessionService.CurrentUserId;
            if (currentUserId == null) return;

            // Gracefully tries Biometrics. If disabled/unavailable/unconfigured, falls back to the local App PIN.
            bool authorized = await securityService.AuthenticateWithFallback(currentUserId.Value, "Clean Up", "Authorize device deletion");

            if (!authorized)
            {
                return;
            }

            var disabledDevices = Devices.Where(t => !t.IsEnabled).ToList();
            foreach (var device in disabledDevices)
            {
                await userDeviceRepository.DeleteItemAsync(device);
                Devices.Remove(device);
            }

            OnPropertyChanged(nameof(HasDisabledDevices));
            Devices = new(Devices);
            await AppShell.DisplayToastAsync("All cleaned up!");
        }

        [RelayCommand]
        private async Task SignOut()
        {
            // Wipe local session indicators
            sessionService.CurrentUserId = null;
            Preferences.Default.Set("IsUserLoggedIn", false);
            Preferences.Default.Remove("CurrentUserId");

            var currentWindow = Application.Current?.Windows?.FirstOrDefault();
            if (currentWindow != null)
            {
                var loginPage = serviceProvider.GetRequiredService<SignInPage>();

                // This tears down the entire AppShell and its navigation stack from memory
                currentWindow.Page = loginPage;
            }
        }
    }
}