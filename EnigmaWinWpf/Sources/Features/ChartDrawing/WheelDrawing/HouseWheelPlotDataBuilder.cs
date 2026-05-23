// HouseWheelPlotDataBuilder.cs
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
/// Builds a WheelPlotData for house-based wheels.
/// Planet positions are mapped proportionally into equal 30° house sectors
/// rather than being placed at their ecliptic longitude.
/// </summary>
public static class HouseWheelPlotDataBuilder
{
    public static WheelPlotData Build(FullChart chart, UserConfiguration? config = null)
    {
        var ascLong = chart.HousePositions.Ascendant.Longitude;
        var mcLong  = chart.HousePositions.Midheaven.Longitude;
        var cusps   = Array.ConvertAll(chart.HousePositions.Cusps, c => c.Longitude);

        var calcConfig = config?.CalculationConfig ?? CalculationConfig.Default;
        var items      = new List<WheelPlotItem>();

        var drawnFactors = config != null
            ? config.FactorConfig.Settings.Where(s => s.IsDrawn).Select(s => s.Factor).ToHashSet()
            : null;

        foreach (var (factor, position) in chart.Coordinates)
        {
            if (factor.CalculationType() == CalculationTypes.Mundane) continue;
            if (factor.CalculationType() == CalculationTypes.Unknown) continue;
            if (position.Ecliptical.Length == 0) continue;
            if (drawnFactors != null && !drawnFactors.Contains(factor)) continue;

            var eclPos     = position.Ecliptical[0].MainPos;
            var speed      = position.Ecliptical[0].MainPosSpeed;
            var houseAngle = EclipticToHouseAngle(eclPos, cusps);
            var glyph      = GlyphSelector.GetGlyphForFactor(factor);
            var speedType  = SpeedOrchestrator.Determine(speed, factor, calcConfig);
            var text       = PositionText(eclPos, speedType);

            items.Add(new WheelPlotItem(
                Factor: factor,
                Glyph: glyph,
                EclipticLongitude: eclPos,
                MundaneAngle: houseAngle,
                PlotAngle: houseAngle,
                PositionText: text,
                SpeedType: speedType));
        }

        var resolved = GlyphOverlapResolver.Resolve(items);

        return new WheelPlotData(
            AscendantLongitude: ascLong,
            McLongitude: mcLong,
            CuspLongitudes: cusps,
            PlanetItems: resolved,
            HasTime: true,
            AspectItems: []);
    }

    public static double EclipticToHouseAngle(double longitude, double[] cusps)
    {
        if (cusps.Length < 12) return 90.0;

        var lon = WheelGeometry.Normalise(longitude);

        for (var i = 0; i < 12; i++)
        {
            var c1 = cusps[i];
            var c2 = cusps[(i + 1) % 12];

            double span, offset;
            if (c2 > c1)
            {
                if (lon < c1 || lon >= c2) continue;
                span   = c2 - c1;
                offset = lon - c1;
            }
            else
            {
                if (lon < c1 && lon >= c2) continue;
                span   = c2 + 360.0 - c1;
                offset = lon >= c1 ? lon - c1 : lon + 360.0 - c1;
            }

            var fraction = span > 0 ? offset / span : 0.0;
            return WheelGeometry.Normalise(90.0 + i * 30.0 + fraction * 30.0);
        }

        return 90.0;
    }

    private static string PositionText(double longitude, SpeedType speedType)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(Math.Abs(inSign) * 60);
        var base_    = $"{totalMin / 60}°{totalMin % 60:D2}'";
        return speedType == SpeedType.Direct ? base_ : $"{base_} {speedType.Abbreviation()}";
    }
}
