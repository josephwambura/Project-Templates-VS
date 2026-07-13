using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MauiSampleApp.Data;
using MauiSampleApp.Models;
using MauiSampleApp.Services;

using System.Collections.ObjectModel;

namespace MauiSampleApp.PageModels
{
    public partial class ManageMetaPageModel(CategoryRepository categoryRepository, TagRepository tagRepository, SeedDataService seedDataService, IErrorHandler errorHandler) : ObservableObject
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
            Categories.Remove(category);
            await categoryRepository.DeleteItemAsync(category);
            await AppShell.DisplayToastAsync("Category deleted");
            SemanticScreenReader.Announce("Category deleted");
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
            if (!await errorHandler.AuthenticateBiometrics("Verify your identity", "Authenticate to reset data"))
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