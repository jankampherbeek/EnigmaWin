// SynastryLocationPicker.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows.Controls;
using EnigmaWin.Sources.Features.Location;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryLocationPicker : UserControl
{
    public SynastryLocationPicker() => InitializeComponent();

    private SynastryLocationPickerViewModel? Vm => DataContext as SynastryLocationPickerViewModel;

    private void OnCountrySelected(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Button { DataContext: LocationCountry country })
            Vm?.SelectCountry(country);
    }

    private void OnCitySelected(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Button { DataContext: LocationCity city })
            Vm?.SelectCity(city);
    }
}
