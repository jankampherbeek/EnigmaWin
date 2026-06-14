// SolarInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Progressive.Solar.UI;

public partial class SolarInputScreen : UserControl
{
    public SolarInputScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SolarViewModel vm) return;
        var window = new SolarHelpWindow(vm.LabelHelp,
            vm.LabelHelpTitle, vm.LabelHelpClose);
        window.ShowDialog();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SolarViewModel vm) return;
        var window = new SolarFactsheetWindow(vm.LabelFactsheetTitle);
        window.ShowDialog();
    }
}
