// EnneagramScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;

public partial class EnneagramScreen : UserControl
{
    public EnneagramScreen()
    {
        InitializeComponent();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var help = new EnneagramHelpWindow();
        help.ShowDialog();
    }
}
