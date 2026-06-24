// PrimDirScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir.UI;

public partial class PrimDirScreen : UserControl
{
    public PrimDirScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PrimDirViewModel vm) return;
        new PrimDirHelpWindow(vm.LabelResultsHelp, vm.LabelHelpTitle, vm.LabelHelpClose)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }
}
