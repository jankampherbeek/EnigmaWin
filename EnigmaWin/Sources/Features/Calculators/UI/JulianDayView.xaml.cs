// JulianDayView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Calculators.UI;

public partial class JulianDayView : UserControl
{
    private JulianDayViewModel? Vm => DataContext as JulianDayViewModel;

    public JulianDayView()
    {
        InitializeComponent();
    }

    private void OnCalculateJulianDayClicked(object sender, RoutedEventArgs e)
    {
        // Calculation will be implemented in a future session.
    }

    private void OnCalculateDateTimeClicked(object sender, RoutedEventArgs e)
    {
        // Calculation will be implemented in a future session.
    }
}
