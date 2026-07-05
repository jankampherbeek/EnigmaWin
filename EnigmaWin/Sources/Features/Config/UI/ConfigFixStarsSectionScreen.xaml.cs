// ConfigFixStarsSectionScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigFixStarsSectionScreen : UserControl
{
    private ConfigFixStarsSectionViewModel? _vm;

    public ConfigFixStarsSectionScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = DataContext as ConfigFixStarsSectionViewModel;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)            => _vm?.GoBack();
    private void OnCancelClicked(object sender, RoutedEventArgs e)          => _vm?.Revert();
    private void OnRestoreDefaultsClicked(object sender, RoutedEventArgs e) => _vm?.RestoreDefaults();

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SaveAsync();
    }
}
