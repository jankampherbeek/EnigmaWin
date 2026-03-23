// ConfigEditScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using EnigmaWin.Sources.AppShell.Navigation;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigEditScreen : UserControl
{
    private ConfigEditViewModel? _vm;

    public ConfigEditScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigEditViewModel;
    }

    // ── Save / Cancel ────────────────────────────────────────────────────────

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SaveAsync();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        _vm?.Revert();
    }

    // ── Set Active ───────────────────────────────────────────────────────────

    private async void OnSetActiveClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null)
            await _vm.SetActiveAsync();
    }

    // ── Section navigation ───────────────────────────────────────────────────

    private void OnCalcClicked(object? sender, RoutedEventArgs e)         => _vm?.NavigateTo(AppRoutes.ConfigSectionCalc);
    private void OnDisplayClicked(object? sender, RoutedEventArgs e)      => _vm?.NavigateTo(AppRoutes.ConfigSectionDisplay);
    private void OnGlyphsClicked(object? sender, RoutedEventArgs e)       => _vm?.NavigateTo(AppRoutes.ConfigSectionGlyphs);
    private void OnFactorsClicked(object? sender, RoutedEventArgs e)      => _vm?.NavigateTo(AppRoutes.ConfigSectionFactors);
    private void OnAspectsClicked(object? sender, RoutedEventArgs e)      => _vm?.NavigateTo(AppRoutes.ConfigSectionAspects);
    private void OnOrbsClicked(object? sender, RoutedEventArgs e)         => _vm?.NavigateTo(AppRoutes.ConfigSectionOrbs);
    private void OnProgressionsClicked(object? sender, RoutedEventArgs e) => _vm?.NavigateTo(AppRoutes.ConfigSectionProgressions);

    // ── Help ─────────────────────────────────────────────────────────────────

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var dialog = new Window
        {
            Title     = _vm.LabelHelpTitle,
            Width     = 480,
            Height    = 280,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var closeBtn = new Button { Content = _vm.LabelHelpClose, HorizontalAlignment = HorizontalAlignment.Right };
        closeBtn.Click += (_, _) => dialog.Close();

        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = _vm.LabelHelpLine1, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = _vm.LabelHelpLine2, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        panel.Children.Add(closeBtn);
        dialog.Content = panel;

        if (TopLevel.GetTopLevel(this) is Window owner)
            await dialog.ShowDialog(owner);
    }
}
