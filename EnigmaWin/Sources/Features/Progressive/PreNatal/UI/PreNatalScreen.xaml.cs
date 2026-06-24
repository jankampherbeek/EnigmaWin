// PreNatalScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalScreen : UserControl
{
    public PreNatalScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PreNatalViewModel vm) return;
        new PreNatalHelpWindow(vm.Inner.LabelHelp, vm.Inner.LabelHelpTitle, vm.Inner.LabelHelpClose)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PreNatalViewModel vm) return;
        new PreNatalFactsheetWindow(vm.Inner.LabelFactsheetTitle)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }
}
