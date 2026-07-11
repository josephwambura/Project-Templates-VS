using MauiSampleApp.Models;

using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace MauiSampleApp.Data
{
    public class SeedDataService(ProjectRepository projectRepository, TaskRepository taskRepository, TagRepository tagRepository, CategoryRepository categoryRepository, ILogger<SeedDataService> logger)
    {
        private readonly string _seedDataFilePath = "SeedData.json";

        public async Task LoadSeedDataAsync()
        {
            await ClearTablesAsync();

            var payload = await LoadSeedDataFromFileAsync();
            if (payload is null) return;

            await SaveProjectsAsync(payload.Projects);
        }

        private async Task<ProjectsJson?> LoadSeedDataFromFileAsync()
        {
            try
            {
                await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(_seedDataFilePath);
                return JsonSerializer.Deserialize(templateStream, JsonContext.Default.ProjectsJson);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error deserializing seed data");
                return null;
            }
        }

        private async Task SaveProjectsAsync(IEnumerable<Project?> projects)
        {
            foreach (var project in projects.Where(p => p is not null))
            {
                await SaveProjectAsync(project!);
            }
        }

        private async Task SaveProjectAsync(Project project)
        {
            if (project.Category is not null)
            {
                await categoryRepository.SaveItemAsync(project.Category);
                project.CategoryID = project.Category.ID;
            }

            await projectRepository.SaveItemAsync(project);

            if (project.Tasks is not null)
            {
                foreach (var task in project.Tasks)
                {
                    task.ProjectID = project.ID;
                    await taskRepository.SaveItemAsync(task);
                }
            }

            if (project.Tags is not null)
            {
                foreach (var tag in project.Tags)
                {
                    await tagRepository.SaveItemAsync(tag, project.ID);
                }
            }
        }

        private async Task ClearTablesAsync()
        {
            try
            {
                await Task.WhenAll(
                    projectRepository.DropTableAsync(),
                    taskRepository.DropTableAsync(),
                    tagRepository.DropTableAsync(),
                    categoryRepository.DropTableAsync());
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error clearing tables");
            }
        }
    }
}