using MauiSampleApp.Models;

using System.Windows.Input;

namespace MauiSampleApp.Pages.Controls;

public partial class DeviceView
{
	public DeviceView()
	{
		InitializeComponent();
    }

    public static readonly BindableProperty DeviceDisabledCommandProperty = BindableProperty.Create(
        nameof(DeviceDisabledCommand),
        typeof(ICommand),
        typeof(DeviceView),
        null);

    public ICommand DeviceDisabledCommand
    {
        get => (ICommand)GetValue(DeviceDisabledCommandProperty);
        set => SetValue(DeviceDisabledCommandProperty, value);
    }

    private void CheckBox_CheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        var checkbox = (CheckBox?)sender;

        if (checkbox?.BindingContext is not UserDevice device)
            return;

        if (device.IsEnabled == e.Value)
            return;

        device.IsEnabled = e.Value;
        DeviceDisabledCommand?.Execute(device);
    }
}