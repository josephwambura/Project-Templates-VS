using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Data;
using MauiSampleApp.Models;
using MauiSampleApp.Services;

namespace MauiSampleApp.PageModels
{
    public partial class TaskDetailPageModel(ProjectRepository projectRepository, TaskRepository taskRepository, IErrorHandler errorHandler) : ObservableObject, IQueryAttributable
    {
        public const string ProjectQueryKey = "project";
        private ProjectTask? _task;
        private bool _canDelete;
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private List<Project> _projects = [];

        [ObservableProperty]
        private Project? _project;

        [ObservableProperty]
        private int _selectedProjectIndex = -1;

        [ObservableProperty]
        private bool _isExistingProject;

        [ObservableProperty]
        private bool _isProjectPickerExpanded;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            LoadTaskAsync(query).FireAndForgetSafeAsync(errorHandler);
        }

        private async Task LoadTaskAsync(IDictionary<string, object> query)
        {
            Project = ExtractProjectFromQuery(query);
            Guid taskId = ExtractTaskIdFromQuery(query);

            _task = await LoadOrCreateTaskAsync(taskId);
            await ConfigureProjectStateAsync();

            InitializeUIState(taskId);
        }

        private Project? ExtractProjectFromQuery(IDictionary<string, object> query)
            => query.TryGetValue(ProjectQueryKey, out var project) ? (Project)project : null;

        private Guid ExtractTaskIdFromQuery(IDictionary<string, object> query)
            => query.TryGetValue("id", out var value) ? Guid.Parse(value.ToString()) : Guid.Empty;

        private async Task<ProjectTask> LoadOrCreateTaskAsync(Guid taskId)
        {
            if (taskId == Guid.Empty) return new ProjectTask();

            var task = await taskRepository.GetAsync(taskId);
            if (task is null)
            {
                errorHandler.HandleError(new Exception($"Task Id {taskId} isn't valid."));
                return new ProjectTask();
            }

            Project = await projectRepository.GetAsync(task.ProjectID);
            return task;
        }

        private async Task ConfigureProjectStateAsync()
        {
            if (Project?.ID == Guid.Empty)
            {
                IsExistingProject = false;
            }
            else
            {
                Projects = await projectRepository.ListAsync();
                IsExistingProject = true;
            }

            SelectedProjectIndex = Project is not null
                ? Projects.FindIndex(p => p.ID == Project.ID)
                : Projects.FindIndex(p => p.ID == _task.ProjectID);
        }

        private void InitializeUIState(Guid taskId)
        {
            if (taskId != Guid.Empty && _task is not null)
            {
                Title = _task.Title;
                IsCompleted = _task.IsCompleted;
                CanDelete = true;
            }
        }

        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                _canDelete = value;
                DeleteCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnIsProjectPickerExpandedChanged(bool value)
        {
            if (value)
            {
                SemanticScreenReader.Announce("Project ComboBox, State Expanded");
            }
            else
            {
                SemanticScreenReader.Announce("Project ComboBox, State Collapsed");
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_task is null)
            {
                errorHandler.HandleError(
                    new Exception("Task or project is null. The task could not be saved."));

                return;
            }

            _task.Title = Title;

            Guid projectId = Project?.ID ?? Guid.Empty;

            if (Projects.Count > SelectedProjectIndex && SelectedProjectIndex >= 0)
                _task.ProjectID = projectId = Projects[SelectedProjectIndex].ID;

            _task.IsCompleted = IsCompleted;

            if (Project?.ID == projectId && !Project.Tasks.Contains(_task))
                Project.Tasks.Add(_task);

            if (_task.ProjectID != Guid.Empty)
                taskRepository.SaveItemAsync(_task).FireAndForgetSafeAsync(errorHandler);

            await Shell.Current.GoToAsync("..?refresh=true");

            if (_task.ID != Guid.Empty)
                await AppShell.DisplayToastAsync("Task saved");
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task Delete()
        {
            if (_task is null || Project is null)
            {
                errorHandler.HandleError(
                    new Exception("Task is null. The task could not be deleted."));

                return;
            }

            if (Project.Tasks.Contains(_task))
                Project.Tasks.Remove(_task);

            if (_task.ID != Guid.Empty)
                await taskRepository.DeleteItemAsync(_task);

            await Shell.Current.GoToAsync("..?refresh=true");
            await AppShell.DisplayToastAsync("Task deleted");
        }
    }
}