// ConfigSymbolicDirectionsSectionScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 23-03-2026

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EnigmaWin.Sources.Features.Config.UI;

public partial class ConfigSymbolicDirectionsSectionScreen : UserControl
{
    private ConfigSymbolicDirectionsSectionViewModel? _vm;

    public ConfigSymbolicDirectionsSectionScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ConfigSymbolicDirectionsSectionViewModel;
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e)   => _vm?.GoBack();
    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Revert();
    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SaveAsync();
    }

    private void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new Window
        {
            Title                 = _vm.LabelHelpTitle,
            Width                 = 420,
            SizeToContent         = SizeToContent.Height,
            CanResize             = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var panel = new StackPanel { Margin = new Avalonia.Thickness(16), Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = _vm.LabelHelpLine1, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        panel.Children.Add(new TextBlock { Text = _vm.LabelHelpLine2, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        var closeBtn = new Button { Content = _vm.LabelHelpClose, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        closeBtn.Click += (_, _) => dlg.Close();
        panel.Children.Add(closeBtn);
        dlg.Content = panel;
        var topLevel = TopLevel.GetTopLevel(this) as Window;
        if (topLevel is not null)
            dlg.ShowDialog(topLevel);
        else
            dlg.Show();
    }
}
