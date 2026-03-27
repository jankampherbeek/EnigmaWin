// HouseTypeWheelViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// ViewModel for the house-based horoscope wheel.
/// Holds toggle state and builds WheelPlotData from the active chart context.
/// </summary>
public partial class HouseTypeWheelViewModel : ObservableObject
{
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;

    [ObservableProperty] private bool _isBlackWhite = false;
    [ObservableProperty] private bool _hideTime     = false;

    public HouseTypeWheelViewModel(IChartSession chartSession, IConfigContext configContext)
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
            var raw = HouseWheelPlotDataBuilder.Build(chart, _configContext.ActiveConfig);
            return EffectiveData(raw, HideTime);
        }
    }

    // MARK: - EffectiveData

    /// <summary>
    /// When HideTime is active: sets HasTime = false so cusp lines and cardinal labels
    /// are hidden, leaving only the sign ring and planet positions.
    /// </summary>
    public static WheelPlotData EffectiveData(WheelPlotData data, bool hideTime = false)
    {
        if (!hideTime) return data;

        return new WheelPlotData(
            AscendantLongitude: data.AscendantLongitude,
            McLongitude:        data.McLongitude,
            CuspLongitudes:     data.CuspLongitudes,
            PlanetItems:        data.PlanetItems,
            HasTime:            false,
            AspectItems:        data.AspectItems);
    }
}
