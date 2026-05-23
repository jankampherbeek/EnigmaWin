// ZodiacTypeWheelViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// ViewModel for the zodiac-based horoscope wheel.
/// Holds toggle state and builds WheelPlotData from the active chart context.
/// </summary>
public partial class ZodiacTypeWheelViewModel : ObservableObject
{
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;

    [ObservableProperty] private bool _isBlackWhite = false;
    [ObservableProperty] private bool _hideAspects  = false;
    [ObservableProperty] private bool _hideTime     = false;

    public ZodiacTypeWheelViewModel(IChartSession chartSession, IConfigContext configContext)
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
            return EffectiveData(raw, HideTime);
        }
    }

    /// <summary>
    /// When HideTime is active: resets AscendantLongitude to 0 so that 0° Aries sits at
    /// 9 o'clock, remaps all planet and aspect angles accordingly, and sets HasTime = false.
    /// Otherwise returns the data unchanged.
    /// </summary>
    public static WheelPlotData EffectiveData(WheelPlotData data, bool hideTime = false)
    {
        if (!hideTime) return data;

        var ascLong = data.AscendantLongitude;

        var remappedPlanets = new WheelPlotItem[data.PlanetItems.Length];
        for (var i = 0; i < data.PlanetItems.Length; i++)
        {
            var item  = data.PlanetItems[i];
            var angle = WheelGeometry.MundaneAngle(item.EclipticLongitude, 0);
            remappedPlanets[i] = new WheelPlotItem(
                Factor:            item.Factor,
                Glyph:             item.Glyph,
                EclipticLongitude: item.EclipticLongitude,
                MundaneAngle:      angle,
                PlotAngle:         angle,
                PositionText:      item.PositionText,
                SpeedType:         item.SpeedType);
        }
        remappedPlanets = GlyphOverlapResolver.Resolve(remappedPlanets);

        var remappedAspects = new WheelAspectItem[data.AspectItems.Length];
        for (var i = 0; i < data.AspectItems.Length; i++)
        {
            var item = data.AspectItems[i];
            remappedAspects[i] = new WheelAspectItem(
                Angle1:    WheelGeometry.Normalise(item.Angle1 + ascLong),
                Angle2:    WheelGeometry.Normalise(item.Angle2 + ascLong),
                Color:     item.Color,
                Exactness: item.Exactness,
                Aspect:    item.Aspect);
        }

        return new WheelPlotData(
            AscendantLongitude: 0,
            McLongitude:        data.McLongitude,
            CuspLongitudes:     data.CuspLongitudes,
            PlanetItems:        remappedPlanets,
            HasTime:            false,
            AspectItems:        remappedAspects);
    }
}
