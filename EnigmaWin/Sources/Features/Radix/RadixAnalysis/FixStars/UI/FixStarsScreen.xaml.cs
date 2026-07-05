// FixStarsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.FixStars.UI;

public partial class FixStarsScreen : UserControl
{
    public FixStarsScreen()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is FixStarsViewModel vm)
                vm.Calculate();
        };
    }
}
