// ProgressiveCalendarInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

public partial class ProgressiveCalendarInputScreen : UserControl
{
    public ProgressiveCalendarInputScreen() => InitializeComponent();

    private ProgressiveCalendarViewModel? Vm => DataContext as ProgressiveCalendarViewModel;

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        if (vm is null) return;
        new ProgressiveCalendarHelpWindow(vm.LabelHelp, vm.LabelHelpTitle, vm.LabelHelpClose)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }

    private void OnTransitFactorsClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        if (vm is null) return;
        var dlg = new ProgressiveCalendarFactorSelectionWindow(
            vm.LabelTransitFactorsButton, vm.LabelTransitFactorsButton, vm.LabelSelectionDone, vm.LabelSelectionCancel,
            ProgressiveCalendarViewModel.SelectableFactors, vm.TransitFactors)
        { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            vm.TransitFactors = dlg.SelectedFactors;
    }

    private void OnSecondaryFactorsClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        if (vm is null) return;
        var dlg = new ProgressiveCalendarFactorSelectionWindow(
            vm.LabelSecondaryFactorsButton, vm.LabelSecondaryFactorsButton, vm.LabelSelectionDone, vm.LabelSelectionCancel,
            ProgressiveCalendarViewModel.SelectableFactors, vm.SecondaryDirectionFactors)
        { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            vm.SecondaryDirectionFactors = dlg.SelectedFactors;
    }

    private void OnSymbolicFactorsClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        if (vm is null) return;
        var dlg = new ProgressiveCalendarFactorSelectionWindow(
            vm.LabelSymbolicFactorsButton, vm.LabelSymbolicFactorsButton, vm.LabelSelectionDone, vm.LabelSelectionCancel,
            ProgressiveCalendarViewModel.SelectableFactors, vm.SymbolicDirectionFactors)
        { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            vm.SymbolicDirectionFactors = dlg.SelectedFactors;
    }

    private void OnRadixFactorsClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        if (vm is null) return;
        var dlg = new ProgressiveCalendarFactorSelectionWindow(
            vm.LabelRadixFactorsButton, vm.LabelRadixFactorsButton, vm.LabelSelectionDone, vm.LabelSelectionCancel,
            ProgressiveCalendarViewModel.SelectableFactors, vm.RadixFactors)
        { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            vm.RadixFactors = dlg.SelectedFactors;
    }

    private void OnAspectsClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        if (vm is null) return;
        var dlg = new ProgressiveCalendarAspectSelectionWindow(
            vm.LabelAspectSelTitle, vm.LabelAspectSelTitle, vm.LabelSelectionDone, vm.LabelSelectionCancel, vm.Aspects)
        { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            vm.Aspects = dlg.SelectedAspects;
    }
}
