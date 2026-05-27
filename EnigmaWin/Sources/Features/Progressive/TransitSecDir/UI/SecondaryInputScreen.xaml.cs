// SecondaryInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;

public partial class SecondaryInputScreen : UserControl
{
    public SecondaryInputScreen()
    {
        InitializeComponent();
    }

    private SecondaryViewModel? Vm =>
        (DataContext as SecondaryInputViewModel)?.Inner ?? DataContext as SecondaryViewModel;

    private void OnSelectClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EventRow row })
            Vm?.SelectEvent(row.Event);
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SecondaryFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SecondaryHelpWindow(rosetta, "view.secondaryscreen.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
