// LogTimeScaleInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;

public partial class LogTimeScaleInputScreen : UserControl
{
    public LogTimeScaleInputScreen()
    {
        InitializeComponent();
    }

    private LogTimeScaleViewModel? Vm =>
        (DataContext as LogTimeScaleInputViewModel)?.Inner ?? DataContext as LogTimeScaleViewModel;

    private void OnSelectClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EventRow row })
            Vm?.SelectEvent(row.Event);
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new LogTimeScaleFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new LogTimeScaleHelpWindow(rosetta, "view.logtimescaleinput.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
