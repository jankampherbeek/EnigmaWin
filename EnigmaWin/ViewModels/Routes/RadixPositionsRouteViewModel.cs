using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using System.ComponentModel;

namespace EnigmaWin.ViewModels.Routes;

public sealed class RadixPositionsRouteViewModel : ObservableObject
{
    private readonly IChartContext _chartContext;

    public RadixPositionsRouteViewModel(IChartContext chartContext)
    {
        _chartContext = chartContext;
        if (_chartContext is INotifyPropertyChanged notifyChart)
        {
            notifyChart.PropertyChanged += OnChartContextPropertyChanged;
        }
    }

    public FullChart? CurrentChart => _chartContext.CurrentChart;

    private void OnChartContextPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IChartContext.CurrentChart))
        {
            OnPropertyChanged(nameof(CurrentChart));
        }
    }
}
