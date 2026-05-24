// Dial90PlotDataBuilder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Speed;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Builds WheelPlotData for the 90° dial wheel.
/// Each factor is mapped to (longitude mod 90) × 4, so the full dial range
/// represents one quarter of the zodiac compressed into 360 visual degrees.
/// </summary>
public static class Dial90PlotDataBuilder
{
    public static WheelPlotData Build(FullChart chart, UserConfiguration? config = null)
    {
        var ascLong    = chart.HousePositions.Ascendant.Longitude;
        var mcLong     = chart.HousePositions.Midheaven.Longitude;
        var calcConfig = config?.CalculationConfig ?? CalculationConfig.Default;

        var drawnFactors = config != null
            ? config.FactorConfig.Settings.Where(s => s.IsDrawn).Select(s => s.Factor).ToHashSet()
            : null;

        var items = new List<WheelPlotItem>();

        foreach (var (factor, position) in chart.Coordinates)
        {
            if (factor.CalculationType() == CalculationTypes.Mundane) continue;
            if (factor.CalculationType() == CalculationTypes.Unknown)  continue;
            if (position.Ecliptical.Length == 0)                       continue;
            if (drawnFactors != null && !drawnFactors.Contains(factor)) continue;

            var eclPos    = position.Ecliptical[0].MainPos;
            var speed     = position.Ecliptical[0].MainPosSpeed;
            var speedType = SpeedOrchestrator.Determine(speed, factor, calcConfig);
            var glyph     = GlyphSelector.GetGlyphForFactor(factor);
            var dialAngle = Dial90Angle(eclPos);
            var text      = Dial90PositionText(eclPos, speedType);

            items.Add(new WheelPlotItem(
                Factor:            factor,
                Glyph:             glyph,
                EclipticLongitude: eclPos,
                MundaneAngle:      dialAngle,
                PlotAngle:         dialAngle,
                PositionText:      text,
                SpeedType:         speedType));
        }

        var ascDialAngle = Dial90Angle(ascLong);
        items.Add(new WheelPlotItem(
            Factor:            Factors.Ascendant,
            Glyph:             GlyphSelector.GetGlyphForFactor(Factors.Ascendant),
            EclipticLongitude: ascLong,
            MundaneAngle:      ascDialAngle,
            PlotAngle:         ascDialAngle,
            PositionText:      Dial90PositionText(ascLong),
            SpeedType:         SpeedType.Direct));

        var mcDialAngle = Dial90Angle(mcLong);
        items.Add(new WheelPlotItem(
            Factor:            Factors.Mc,
            Glyph:             GlyphSelector.GetGlyphForFactor(Factors.Mc),
            EclipticLongitude: mcLong,
            MundaneAngle:      mcDialAngle,
            PlotAngle:         mcDialAngle,
            PositionText:      Dial90PositionText(mcLong),
            SpeedType:         SpeedType.Direct));

        var resolved = GlyphOverlapResolver.Resolve(items);

        return new WheelPlotData(
            AscendantLongitude: ascLong,
            McLongitude:        mcLong,
            CuspLongitudes:     [],
            PlanetItems:        resolved,
            HasTime:            true,
            AspectItems:        []);
    }

    public static WheelPlotData EffectiveData(WheelPlotData data, bool hideTime)
    {
        if (!hideTime) return data;

        var filtered = data.PlanetItems
            .Where(p => p.Factor != Factors.Ascendant && p.Factor != Factors.Mc)
            .ToArray();

        return new WheelPlotData(
            AscendantLongitude: data.AscendantLongitude,
            McLongitude:        data.McLongitude,
            CuspLongitudes:     [],
            PlanetItems:        filtered,
            HasTime:            false,
            AspectItems:        []);
    }

    private static double Dial90Angle(double longitude) => (longitude % 90.0) * 4.0;

    private static string Dial90PositionText(double longitude, SpeedType speedType = SpeedType.Direct)
    {
        var inDial   = longitude % 90.0;
        var totalMin = (int)(Math.Abs(inDial) * 60);
        var deg      = totalMin / 60;
        var min      = totalMin % 60;
        var base_    = $"{deg}°{min:D2}'";
        return speedType == SpeedType.Direct ? base_ : $"{base_} {speedType.Abbreviation()}";
    }
}
