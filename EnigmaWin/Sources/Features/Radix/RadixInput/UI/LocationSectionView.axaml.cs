// LocationSectionView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Location;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class LocationSectionView : UserControl
{
    public LocationSectionView()
    {
        InitializeComponent();
    }

    private RadixInputViewModel? Vm => DataContext as RadixInputViewModel;

    private void OnCountrySelected(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: LocationCountry country })
            Vm?.SelectCountry(country);
    }

    private void OnCitySelected(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: LocationCity city })
            Vm?.SelectCity(city);
    }
}
