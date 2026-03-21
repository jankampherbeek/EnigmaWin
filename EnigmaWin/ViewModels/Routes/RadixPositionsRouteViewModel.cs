using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using System.ComponentModel;

namespace EnigmaWin.ViewModels.Routes;

public sealed class RadixPositionsRouteViewModel : ObservableObject
{
    private readonly IChartSession _chartSession;

    public RadixPositionsRouteViewModel(IChartSession chartSession)
    {
        _chartSession = chartSession;
        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnSessionPropertyChanged;
    }

    public FullChart? SelectedChart => _chartSession.SelectedChart;

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IChartSession.SelectedChart))
            OnPropertyChanged(nameof(SelectedChart));
    }
}
