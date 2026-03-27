// ConfigListScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigListScreen : UserControl
{
    private ConfigListViewModel? _vm;

    public ConfigListScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigListViewModel;
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_vm is null) return;
        if (e.AddedItems is { Count: > 0 } && e.AddedItems[0] is UserConfiguration config)
            _vm.SelectConfig(config);
    }

    private void OnAddClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        _vm.IsAddPanelVisible = true;
    }

    private async void OnConfirmAddClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        await _vm.AddConfigAsync();
    }

    private void OnCancelAddClicked(object? sender, RoutedEventArgs e)
    {
        _vm?.CancelAdd();
    }

    private async void OnDeleteClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null || sender is not Button { DataContext: UserConfiguration config }) return;
        var confirmed = await ShowDeleteConfirmAsync(config.Name);
        if (confirmed)
            await _vm.DeleteConfigAsync(config);
    }

    private async Task<bool> ShowDeleteConfirmAsync(string configName)
    {
        if (_vm is null) return false;

        var tcs = new TaskCompletionSource<bool>();

        var dialog = new Window
        {
            Title  = _vm.LabelDeleteTitle,
            Width  = 400,
            Height = 170,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var message = new TextBlock
        {
            Text = _vm.FormatDeleteMessage(configName),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };

        var confirmBtn = new Button { Content = _vm.LabelDeleteButton };
        var cancelBtn  = new Button { Content = _vm.LabelCancel };

        confirmBtn.Click += (_, _) => { tcs.TrySetResult(true);  dialog.Close(); };
        cancelBtn.Click  += (_, _) => { tcs.TrySetResult(false); dialog.Close(); };
        dialog.Closed    += (_, _) =>   tcs.TrySetResult(false);

        var buttons = new StackPanel
        {
            Orientation         = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing             = 8
        };
        buttons.Children.Add(cancelBtn);
        buttons.Children.Add(confirmBtn);

        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 16 };
        panel.Children.Add(message);
        panel.Children.Add(buttons);
        dialog.Content = panel;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);

        return await tcs.Task;
    }
}
