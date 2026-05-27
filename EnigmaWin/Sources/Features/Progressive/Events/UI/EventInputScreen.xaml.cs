// EventInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Location;

namespace EnigmaWin.Sources.Features.Progressive.Events.UI;

public partial class EventInputScreen : UserControl
{
    private EventInputViewModel Vm => (EventInputViewModel)DataContext;

    public EventInputScreen() => InitializeComponent();

    private void OnCountrySelected(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is LocationCountry country)
            Vm.SelectCountry(country);
    }

    private void OnCitySelected(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is LocationCity city)
            Vm.SelectCity(city);
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) =>
        Vm.CancelCommand.Execute(null);

    private void OnHelpClicked(object sender, RoutedEventArgs e) =>
        MessageBox.Show(Vm.LabelHelp, Vm.LabelTitle, MessageBoxButton.OK, MessageBoxImage.Information);
}
