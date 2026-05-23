// ConfigListScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigListScreen : UserControl
{
    private ConfigListViewModel? _vm;

    public ConfigListScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = DataContext as ConfigListViewModel;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_vm is null) return;
        if (e.AddedItems is { Count: > 0 } && e.AddedItems[0] is UserConfiguration config)
            _vm.SelectConfig(config);
    }

    private void OnAddClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        _vm.IsAddPanelVisible = true;
    }

    private async void OnConfirmAddClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        await _vm.AddConfigAsync();
    }

    private void OnCancelAddClicked(object sender, RoutedEventArgs e)
    {
        _vm?.CancelAdd();
    }

    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null || sender is not Button { DataContext: UserConfiguration config }) return;
        var confirmed = ShowDeleteConfirm(config.Name);
        if (confirmed)
            await _vm.DeleteConfigAsync(config);
    }

    private bool ShowDeleteConfirm(string configName)
    {
        if (_vm is null) return false;
        var msg = _vm.FormatDeleteMessage(configName);
        var result = MessageBox.Show(
            Window.GetWindow(this),
            msg,
            _vm.LabelDeleteTitle,
            MessageBoxButton.OKCancel,
            MessageBoxImage.Warning);
        return result == MessageBoxResult.OK;
    }
}
