// MainWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.AppShell.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Views;

public partial class MainWindow : Window
{
    private readonly IRosetta _rosetta;

    public MainWindow(IRosetta rosetta)
    {
        _rosetta = rosetta;
        InitializeComponent();
    }

    private void OnAboutClicked(object? sender, RoutedEventArgs e)
    {
        var window = new AboutWindow(_rosetta);
        window.ShowDialog(this);
    }
}
