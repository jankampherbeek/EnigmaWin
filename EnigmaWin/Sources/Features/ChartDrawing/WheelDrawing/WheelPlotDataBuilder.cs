// WheelPlotDataBuilder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Speed;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Builds a WheelPlotData from a FullChart for zodiac-based and French-style wheels.
/// Aspects are included when a UserConfiguration is provided.
/// </summary>
public static class WheelPlotDataBuilder
{
    /// <summary>Builds WheelPlotData from a FullChart and optional user configuration.</summary>
    public static WheelPlotData Build(FullChart chart, UserConfiguration? config = null)
    {
        var ascLong = chart.HousePositions.Ascendant.Longitude;
        var mcLong  = chart.HousePositions.Midheaven.Longitude;
        var cusps   = Array.ConvertAll(chart.HousePositions.Cusps, c => c.Longitude);

        var items = new List<WheelPlotItem>();
        var calcConfig = config?.CalculationConfig ?? CalculationConfig.Default;

        var drawnFactors = config != null
            ? config.FactorConfig.Settings.Where(s => s.IsDrawn).Select(s => s.Factor).ToHashSet()
            : null;

        foreach (var (factor, position) in chart.Coordinates)
        {
            if (factor.CalculationType() == CalculationTypes.Mundane) continue;
            if (factor.CalculationType() == CalculationTypes.Unknown) continue;
            if (position.Ecliptical.Length == 0) continue;
            if (drawnFactors != null && !drawnFactors.Contains(factor)) continue;

            var eclPos  = position.Ecliptical[0].MainPos;
            var speed   = position.Ecliptical[0].MainPosSpeed;
            var mundane = WheelGeometry.MundaneAngle(eclPos, ascLong);
            var glyph   = GlyphSelector.GetGlyphForFactor(factor);
            var speedType = SpeedOrchestrator.Determine(speed, factor, calcConfig);
            var text    = PositionText(eclPos, speedType);

            items.Add(new WheelPlotItem(
                Factor: factor,
                Glyph: glyph,
                EclipticLongitude: eclPos,
                MundaneAngle: mundane,
                PlotAngle: mundane,
                PositionText: text,
                SpeedType: speedType));
        }

        var resolved    = GlyphOverlapResolver.Resolve(items);
        var aspectItems = config is not null
            ? BuildAspectItems(chart, resolved, ascLong, config)
            : [];

        return new WheelPlotData(
            AscendantLongitude: ascLong,
            McLongitude: mcLong,
            CuspLongitudes: cusps,
            PlanetItems: resolved,
            HasTime: true,
            AspectItems: aspectItems);
    }

    // MARK: - Aspect items

    private static WheelAspectItem[] BuildAspectItems(
        FullChart chart,
        WheelPlotItem[] planetItems,
        double ascLong,
        UserConfiguration config)
    {
        var foundAspects = AspectsOrchestrator.Calculate(
            chart,
            config.FactorConfig,
            config.AspectConfig,
            config.OrbConfig);

        if (foundAspects.Count == 0) return [];

        // Map factor → mundane angle for drawn planets only
        var angleMap = new Dictionary<Factors, double>();
        foreach (var item in planetItems)
            angleMap[item.Factor] = item.MundaneAngle;

        // Map aspect → color from config
        var colorMap = new Dictionary<Aspects, Color>();
        foreach (var setting in config.AspectConfig.Settings)
        {
            var c = setting.Color;
            colorMap[setting.Aspect] = Color.FromArgb(
                (byte)(c.Opacity * 255),
                (byte)(c.Red   * 255),
                (byte)(c.Green * 255),
                (byte)(c.Blue  * 255));
        }

        var result = new List<WheelAspectItem>();
        foreach (var found in foundAspects)
        {
            if (!angleMap.TryGetValue(found.Factor1, out var angle1)) continue;
            if (!angleMap.TryGetValue(found.Factor2, out var angle2)) continue;

            colorMap.TryGetValue(found.Aspect, out var color);
            var exactness = found.MaxOrb > 0
                ? Math.Max(0.0, 1.0 - found.Orb / found.MaxOrb)
                : 1.0;

            result.Add(new WheelAspectItem(angle1, angle2, color, exactness, found.Aspect));
        }

        return [.. result];
    }

    // MARK: - Helpers

    /// <summary>
    /// Formats the position as "deg°min'" within the sign,
    /// with the speed abbreviation appended when not direct.
    /// E.g. "15°23'" or "15°23' R".
    /// </summary>
    private static string PositionText(double longitude, SpeedType speedType)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        var deg      = totalMin / 60;
        var min      = totalMin % 60;
        var base_    = $"{deg}°{min:D2}'";
        return speedType == SpeedType.Direct ? base_ : $"{base_} {speedType.Abbreviation()}";
    }
}
