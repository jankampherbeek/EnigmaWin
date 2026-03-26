// DialPlotDataBuilder.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026
//
// Converts a FullChart into WheelPlotData for Ebertin-style dial wheels.
// Unlike WheelPlotDataBuilder, angles equal the ecliptic longitude directly —
// no rotation by ascendant. Cusp lines and aspects are not used.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Speed;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Builds WheelPlotData for Ebertin-style 360° dial wheels.
/// Dial angle = ecliptic longitude (Aries at top, fixed ring — no ascendant rotation).
/// ASC and MC are added as regular items and can be filtered out by HideTime.
/// </summary>
public static class DialPlotDataBuilder
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
            var text      = DialPositionText(eclPos, speedType);

            // For the dial: mundaneAngle == eclipticLongitude (no ascendant rotation).
            items.Add(new WheelPlotItem(
                Factor:            factor,
                Glyph:             glyph,
                EclipticLongitude: eclPos,
                MundaneAngle:      eclPos,
                PlotAngle:         eclPos,
                PositionText:      text,
                SpeedType:         speedType));
        }

        // Add ASC and MC as regular items so the overlap resolver treats them like planets.
        items.Add(new WheelPlotItem(
            Factor:            Factors.Ascendant,
            Glyph:             GlyphSelector.GetGlyphForFactor(Factors.Ascendant),
            EclipticLongitude: ascLong,
            MundaneAngle:      ascLong,
            PlotAngle:         ascLong,
            PositionText:      DialPositionText(ascLong),
            SpeedType:         SpeedType.Direct));

        items.Add(new WheelPlotItem(
            Factor:            Factors.Mc,
            Glyph:             GlyphSelector.GetGlyphForFactor(Factors.Mc),
            EclipticLongitude: mcLong,
            MundaneAngle:      mcLong,
            PlotAngle:         mcLong,
            PositionText:      DialPositionText(mcLong),
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

    // MARK: - HideTime

    /// <summary>
    /// When hideTime is true: removes ASC and MC items and sets HasTime = false.
    /// Otherwise returns data unchanged.
    /// </summary>
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

    // MARK: - Helpers

    private static string DialPositionText(double longitude, SpeedType speedType = SpeedType.Direct)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        var deg      = totalMin / 60;
        var min      = totalMin % 60;
        var base_    = $"{deg}°{min:D2}'";
        return speedType == SpeedType.Direct ? base_ : $"{base_} {speedType.Abbreviation()}";
    }
}
