// ObliquityView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;

namespace EnigmaWin.Sources.Features.Calculators.UI;

public partial class ObliquityView : UserControl
{
    public ObliquityView()
    {
        InitializeComponent();
    }

    private void OnCalculateObliquityClicked(object sender, RoutedEventArgs e)
    {
        (DataContext as ObliquityViewModel)?.CalculateObliquity();
    }
}
