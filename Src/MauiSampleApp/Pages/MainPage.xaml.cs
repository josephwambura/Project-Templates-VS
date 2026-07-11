using MauiSampleApp.Models;
using MauiSampleApp.PageModels;

namespace MauiSampleApp.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}