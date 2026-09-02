// ProgressiveCalendarScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

public partial class ProgressiveCalendarScreen : UserControl
{
    public ProgressiveCalendarScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProgressiveCalendarViewModel vm) return;
        new ProgressiveCalendarHelpWindow(vm.LabelResultsHelp, vm.LabelHelpTitle, vm.LabelHelpClose)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }
}
