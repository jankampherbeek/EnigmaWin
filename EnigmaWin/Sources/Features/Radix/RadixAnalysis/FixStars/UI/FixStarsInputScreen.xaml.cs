// FixStarsInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;

public partial class FixStarsInputScreen : UserControl
{
    public FixStarsInputScreen()
    {
        InitializeComponent();
    }

    private void OnCalculateClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is FixStarsViewModel vm)
            vm.Calculate();
    }
}
