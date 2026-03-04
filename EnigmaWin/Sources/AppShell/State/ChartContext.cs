using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.AppShell.State;

public sealed partial class ChartContext : ObservableObject, IChartContext
{
    [ObservableProperty]
    private FullChart? _currentChart;
}
