namespace MauiSampleApp.Pages;

public partial class SignUpPage : ContentPage
{
    public SignUpPage(SignUpPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}