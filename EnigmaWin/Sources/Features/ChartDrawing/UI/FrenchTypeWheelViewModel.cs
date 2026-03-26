// FrenchTypeWheelViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// ViewModel for the French-style horoscope wheel.
/// Holds toggle state and builds WheelPlotData from the active chart context.
/// Uses the same ZodiacTypeWheelViewModel.EffectiveData helper for HideTime logic.
/// </summary>
public partial class FrenchTypeWheelViewModel : ObservableObject
{
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;

    [ObservableProperty] private bool _isBlackWhite = false;
    [ObservableProperty] private bool _hideTime     = false;

    public FrenchTypeWheelViewModel(IChartSession chartSession, IConfigContext configContext)
    {
        _chartSession  = chartSession;
        _configContext = configContext;
    }

    // MARK: - Plot data

    /// <summary>The plot data to draw, with HideTime applied when active.</summary>
    public WheelPlotData CurrentPlotData
    {
        get
        {
            var chart = _chartSession.SelectedChart;
            if (chart is null) return WheelPlotData.Empty;
            var raw = WheelPlotDataBuilder.Build(chart, _configContext.ActiveConfig);
            return ZodiacTypeWheelViewModel.EffectiveData(raw, HideTime);
        }
    }
}
