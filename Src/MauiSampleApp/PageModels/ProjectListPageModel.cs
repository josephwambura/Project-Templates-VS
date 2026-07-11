using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Data;
using MauiSampleApp.Models;
using MauiSampleApp.Services;

namespace MauiSampleApp.PageModels
{
    public partial class ProjectListPageModel(ProjectRepository projectRepository) : ObservableObject
    {
        [ObservableProperty]
        private List<Project> _projects = [];

        [ObservableProperty]
        private Project? selectedProject;

        [RelayCommand]
        private async Task Appearing()
        {
            Projects = await projectRepository.ListAsync();
        }

        [RelayCommand]
        Task? NavigateToProject(Project project)
            => project is null ? Task.CompletedTask : Shell.Current.GoToAsync($"project?id={project.ID}");

        [RelayCommand]
        async Task AddProject()
        {
            await Shell.Current.GoToAsync($"project");
        }
    }
}