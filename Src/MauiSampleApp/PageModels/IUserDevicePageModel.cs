using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Models;

namespace MauiSampleApp.PageModels
{
    public interface IUserDevicePageModel
    {
        IAsyncRelayCommand<UserDevice> NavigateToDeviceCommand { get; }
        bool IsBusy { get; }
    }
}