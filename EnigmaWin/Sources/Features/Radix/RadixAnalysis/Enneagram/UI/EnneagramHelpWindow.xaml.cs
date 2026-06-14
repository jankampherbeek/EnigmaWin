// EnneagramHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;

public partial class EnneagramHelpWindow : Window
{
    public EnneagramHelpWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
