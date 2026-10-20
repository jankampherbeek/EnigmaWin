// HarmonicWheelPlotDataBuilder.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Builds a WheelPlotData from a list of harmonic positions.
/// - If the harmonic ASC is present in the positions, it is placed at the left (as in the other charts)
///   and 12 equal cusps are added, each a multiple of 30° from the harmonic ASC. The harmonic MC can fall anywhere.
/// - Without a harmonic ASC, 0° Aries is placed at the left and no cusps are included.
/// - ASC and MC, if present, are included as regular planet items so the canvas can draw their
///   indicator lines at the correct harmonic longitudes.
/// - Aspect items are calculated between the harmonic positions, using the aspects and orbs
///   from the configuration. Without a configuration no aspect items are included.
/// </summary>
public static class HarmonicWheelPlotDataBuilder
{
    public static WheelPlotData Build(
        IReadOnlyList<(Factors Factor, double Longitude)> harmonicPositions,
        UserConfiguration? config = null)
    {
        double? ascLong = harmonicPositions.Any(p => p.Factor == Factors.Ascendant)
            ? harmonicPositions.First(p => p.Factor == Factors.Ascendant).Longitude
            : null;
        double? mcLong = harmonicPositions.Any(p => p.Factor == Factors.Mc)
            ? harmonicPositions.First(p => p.Factor == Factors.Mc).Longitude
            : null;
        var wheelAscLong = ascLong ?? 0.0;   // 0° Aries at the left when there is no harmonic ASC
        var cusps        = ascLong.HasValue ? EqualCusps(ascLong.Value) : [];

        var items = new List<WheelPlotItem>();
        foreach (var (factor, longitude) in harmonicPositions)
        {
            var mundane = WheelGeometry.MundaneAngle(longitude, wheelAscLong);
            items.Add(new WheelPlotItem(
                Factor:            factor,
                Glyph:             GlyphSelector.GetGlyphForFactor(factor),
                EclipticLongitude: longitude,
                MundaneAngle:      mundane,
                PlotAngle:         mundane,
                PositionText:      DrawCusps.CuspPositionText(longitude),
                SpeedType:         SpeedType.Direct));   // speed is not meaningful for harmonic positions
        }

        var resolved    = GlyphOverlapResolver.Resolve(items);
        var aspectItems = BuildAspectItems(harmonicPositions, resolved, config);

        return new WheelPlotData(
            AscendantLongitude: wheelAscLong,
            McLongitude:        mcLong ?? 0.0,   // not used; the canvas takes the MC from the planet items
            CuspLongitudes:     cusps,
            PlanetItems:        resolved,
            HasTime:            ascLong.HasValue,
            AspectItems:        aspectItems);
    }

    private static WheelAspectItem[] BuildAspectItems(
        IReadOnlyList<(Factors Factor, double Longitude)> harmonicPositions,
        WheelPlotItem[] planetItems,
        UserConfiguration? config)
    {
        if (config is null) return [];

        var foundAspects = AspectsOrchestrator.Calculate(
            harmonicPositions, config.FactorConfig, config.AspectConfig, config.OrbConfig);
        return WheelPlotDataBuilder.ToAspectItems(foundAspects, planetItems, config.AspectConfig);
    }

    /// <summary>Equal house cusps: cusp n (0-based) is at ascLong + n * 30°.</summary>
    private static double[] EqualCusps(double ascLong) =>
        Enumerable.Range(0, 12).Select(i => WheelGeometry.Normalise(ascLong + i * 30.0)).ToArray();
}
