// ConfigSectionPlaceholderScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigSectionPlaceholderScreen : UserControl
{
    private ConfigSectionViewModel? _vm;

    public ConfigSectionPlaceholderScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = DataContext as ConfigSectionViewModel;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e) => _vm?.GoBack();
}
