// RingTypeWheelViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// ViewModel for the Ring-style horoscope wheel.
/// Uses the same ZodiacTypeWheelViewModel.EffectiveData helper for HideTime logic.
/// </summary>
public partial class RingTypeWheelViewModel : ObservableObject
{
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;

    [ObservableProperty] private bool _isBlackWhite = false;
    [ObservableProperty] private bool _hideTime     = false;

    public RingTypeWheelViewModel(IChartSession chartSession, IConfigContext configContext)
    {
        _chartSession  = chartSession;
        _configContext = configContext;
    }

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
