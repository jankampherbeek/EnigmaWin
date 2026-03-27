// ConfigOrbSectionScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigOrbSectionScreen : UserControl
{
    private ConfigOrbSectionViewModel? _vm;

    public ConfigOrbSectionScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigOrbSectionViewModel;
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e)   => _vm?.GoBack();
    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Revert();

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SaveAsync();
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var dialog = new Window
        {
            Title                 = _vm.LabelHelpTitle,
            Width                 = 500,
            Height                = 260,
            CanResize             = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var closeBtn = new Button { Content = _vm.LabelHelpClose, HorizontalAlignment = HorizontalAlignment.Right };
        closeBtn.Click += (_, _) => dialog.Close();

        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 8 };
        foreach (var line in new[] { _vm.LabelHelpLine1, _vm.LabelHelpLine2, _vm.LabelHelpLine3 })
            panel.Children.Add(new TextBlock { Text = line, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        panel.Children.Add(closeBtn);
        dialog.Content = panel;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);
    }
}
