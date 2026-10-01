// MainWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Windows;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.About.UI;
using EnigmaWin.Sources.Features.GlyphOverView.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Views;

public partial class MainWindow : Window
{
    private bool _closeConfirmed;

    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    /// <summary>Warns about unsaved configuration changes before the application closes.</summary>
    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_closeConfirmed) return;
        var unsavedChanges = ((App)Application.Current).Services.GetRequiredService<IUnsavedChangesGuard>();
        if (!unsavedChanges.HasUnsavedChanges) return;

        e.Cancel = true;
        unsavedChanges.Perform(() =>
        {
            _closeConfirmed = true;
            // Close cannot be called while the window is closing; close again once this event is done.
            Dispatcher.BeginInvoke(Close);
        });
    }

    private void OnAboutClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new AboutWindow(rosetta) { Owner = this }.ShowDialog();
    }

    private void OnGlyphOverviewClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new GlyphOverviewWindow(new GlyphOverviewViewModel(rosetta)) { Owner = this }.ShowDialog();
    }
}
