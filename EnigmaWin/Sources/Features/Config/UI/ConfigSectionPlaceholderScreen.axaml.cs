// ConfigSectionPlaceholderScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigSectionPlaceholderScreen : UserControl
{
    private ConfigSectionViewModel? _vm;

    public ConfigSectionPlaceholderScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigSectionViewModel;
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e) => _vm?.GoBack();
}
