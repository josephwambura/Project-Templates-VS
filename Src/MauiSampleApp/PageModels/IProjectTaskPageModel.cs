using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Models;

namespace MauiSampleApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}