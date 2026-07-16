namespace MauiSampleApp.Pages;

public partial class UserDeviceListPage : ContentPage
{
	public UserDeviceListPage(UserDeviceListPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}