// SymbolicInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Progressive.SymbolicDir.UI;

public partial class SymbolicInputScreen : UserControl
{
    public SymbolicInputScreen()
    {
        InitializeComponent();
    }

    private SymbolicViewModel? Vm =>
        (DataContext as SymbolicInputViewModel)?.Inner ?? DataContext as SymbolicViewModel;

    private void OnSelectClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EventRow row })
            Vm?.SelectEvent(row.Event);
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SymbolicFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SymbolicHelpWindow(rosetta, "view.symbolicscreen.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
