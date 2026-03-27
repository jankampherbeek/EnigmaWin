// ConfigProgressionsSectionScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigProgressionsSectionScreen : UserControl
{
    private ConfigProgressionsSectionViewModel? _vm;

    public ConfigProgressionsSectionScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigProgressionsSectionViewModel;
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e)     => _vm?.GoBack();
    private void OnPrimaryClicked(object? sender, RoutedEventArgs e)   => _vm?.GoToPrimary();
    private void OnTransitsClicked(object? sender, RoutedEventArgs e)  => _vm?.GoToTransits();
    private void OnSecondaryClicked(object? sender, RoutedEventArgs e) => _vm?.GoToSecondary();
    private void OnSymbolicClicked(object? sender, RoutedEventArgs e)  => _vm?.GoToSymbolic();
    private void OnSolarClicked(object? sender, RoutedEventArgs e)     => _vm?.GoToSolar();
}
