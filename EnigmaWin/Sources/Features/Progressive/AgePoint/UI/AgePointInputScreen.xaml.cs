// AgePointInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

public partial class AgePointInputScreen : UserControl
{
    public AgePointInputScreen()
    {
        InitializeComponent();
    }

    private AgePointViewModel? Vm =>
        (DataContext as AgePointInputViewModel)?.Inner ?? DataContext as AgePointViewModel;

    private void OnSelectClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EventRow row })
            Vm?.SelectEvent(row.Event);
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new AgePointFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new AgePointHelpWindow(rosetta, "view.agepointinput.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
