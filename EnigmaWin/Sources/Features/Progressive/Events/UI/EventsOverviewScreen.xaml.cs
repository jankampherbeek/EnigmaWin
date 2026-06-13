// EventsOverviewScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EnigmaWin.Sources.Features.Progressive.Events.UI;

public partial class EventsOverviewScreen : UserControl
{
    private EventsOverviewViewModel Vm => (EventsOverviewViewModel)DataContext;

    public EventsOverviewScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not EventsOverviewViewModel vm) return;
        MessageBox.Show(vm.LabelHelp, vm.LabelTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnRowClicked(object sender, MouseButtonEventArgs e)
    {
        if (sender is Grid { DataContext: EventRow row })
            Vm.SelectEvent(row.Event);
    }

    private void OnEditMenuClicked(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem { Tag: EventRow row })
            Vm.NavigateToEditEventCommand.Execute(row.Event);
    }

    private void OnDeleteMenuClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: EventRow row }) return;
        var message = Vm.DeleteMessageFor(row.Event);
        var result  = MessageBox.Show(message, Vm.LabelDeleteTitle,
                          MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
            _ = Vm.DeleteEventAsync(row.Event);
    }
}
