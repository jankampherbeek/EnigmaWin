// PreNatalInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalInputScreen : UserControl
{
    public PreNatalInputScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PreNatalInputViewModel vm) return;
        new PreNatalHelpWindow(vm.LabelHelp, vm.LabelHelpTitle, vm.LabelHelpClose)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PreNatalInputViewModel vm) return;
        new PreNatalFactsheetWindow(vm.LabelFactsheetTitle)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }

    private void OnSelectFactorsClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PreNatalInputViewModel vm) return;
        var win = new PreNatalFactorSelectionWindow(vm.GetSelectedFactors())
        {
            Owner = Window.GetWindow(this)
        };
        if (win.ShowDialog() == true)
            vm.ApplyFactors(win.SelectedFactors);
    }

    private void OnSelectAspectsClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PreNatalInputViewModel vm) return;
        var win = new PreNatalAspectSelectionWindow(vm.GetSelectedAspects())
        {
            Owner = Window.GetWindow(this)
        };
        if (win.ShowDialog() == true)
            vm.ApplyAspects(win.SelectedAspects);
    }
}
