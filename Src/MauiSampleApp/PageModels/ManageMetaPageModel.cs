using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Data;
using MauiSampleApp.Models;
using MauiSampleApp.Services;

using System.Collections.ObjectModel;

namespace MauiSampleApp.PageModels
{
    public partial class ManageMetaPageModel(CategoryRepository categoryRepository, TagRepository tagRepository, SeedDataService seedDataService, SessionService sessionService, IErrorHandler errorHandler, ISecurityService securityService) : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Category> _categories = [];

        [ObservableProperty]
        private ObservableCollection<Tag> _tags = [];

        private async Task LoadData()
        {
            var categoriesList = await categoryRepository.ListAsync();
            Categories = new ObservableCollection<Category>(categoriesList);
            var tagsList = await tagRepository.ListAsync();
            Tags = new ObservableCollection<Tag>(tagsList);
        }

        [RelayCommand]
        private Task Appearing()
            => LoadData();

        [RelayCommand]
        private async Task SaveCategories()
        {
            foreach (var category in Categories)
            {
                await categoryRepository.SaveItemAsync(category);
            }

            await AppShell.DisplayToastAsync("Categories saved");
            SemanticScreenReader.Announce("Categories saved");
        }

        [RelayCommand]
        private async Task DeleteCategory(Category category)
        {
            if (category == null) return;

            try
            {
                bool isAssignedToProjects = await categoryRepository.IsCategoryInUseAsync(category.ID);

                if (isAssignedToProjects)
                {
                    errorHandler.HandleError(new Exception(
                        $"Cannot delete '{category.Title}'. It is currently assigned to active projects. " +
                        "Reassign or delete those projects before removing this category."));
                    return;
                }

                Categories.Remove(category);
                await categoryRepository.DeleteItemAsync(category);
                await AppShell.DisplayToastAsync("Category deleted");
                SemanticScreenReader.Announce("Category deleted");
            }
            catch (Exception ex)
            {
                // Handle unexpected database errors gracefully
                errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task AddCategory()
        {
            var category = new Category();
            Categories.Add(category);
            await categoryRepository.SaveItemAsync(category);
            await AppShell.DisplayToastAsync("Category added");
            SemanticScreenReader.Announce("Category added");
        }

        [RelayCommand]
        private async Task SaveTags()
        {
            foreach (var tag in Tags)
            {
                await tagRepository.SaveItemAsync(tag);
            }

            await AppShell.DisplayToastAsync("Tags saved");
            SemanticScreenReader.Announce("Tags saved");
        }

        [RelayCommand]
        private async Task DeleteTag(Tag tag)
        {
            Tags.Remove(tag);
            await tagRepository.DeleteItemAsync(tag);
            await AppShell.DisplayToastAsync("Tag deleted");
            SemanticScreenReader.Announce("Tags deleted");
        }

        [RelayCommand]
        private async Task AddTag()
        {
            var tag = new Tag();
            Tags.Add(tag);
            await tagRepository.SaveItemAsync(tag);
            await AppShell.DisplayToastAsync("Tag added");
            SemanticScreenReader.Announce("Tags added");
        }

        [RelayCommand]
        private async Task Reset()
        {
            var currentUserId = sessionService.CurrentUserId;
            if (currentUserId == null) return;

            // Gracefully tries Biometrics. If disabled/unavailable/unconfigured, falls back to the local App PIN.
            bool authorized = await securityService.AuthenticateWithFallback(currentUserId.Value, "Reset Data", "Authorize data reset");

            if (!authorized)
            {
                return;
            }

            Preferences.Default.Remove("is_seeded");
            await seedDataService.LoadSeedDataAsync();
            Preferences.Default.Set("is_seeded", true);
            await Shell.Current.GoToAsync("//main");
        }
    }
}