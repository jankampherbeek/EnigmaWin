// LotsOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Lots;

public enum LotType { Fortune, Spirit, Eros, Victory, Necessity, Courage, Nemesis }

public sealed record LotResult(LotType Type, double Longitude);

/// <summary>Calculates the seven classic Hellenistic lots.
/// A night chart is defined as a chart where the Sun is below the horizon.
/// When 'useSect' is deselected, or the chart is a day chart, the day formulas are used.</summary>
public static class LotsOrchestrator
{
    public static bool IsNightChart(FullChart chart) =>
        chart.Coordinates.TryGetValue(Factors.Sun, out var sun)
        && sun.Horizontal.Length > 0
        && sun.Horizontal[0].Altitude < 0.0;

    public static List<LotResult> Calculate(FullChart chart, bool useSect)
    {
        var useNightFormula = IsNightChart(chart) && useSect;

        var ascendant = chart.HousePositions.Ascendant.Longitude;
        var sun     = Longitude(Factors.Sun, chart);
        var moon    = Longitude(Factors.Moon, chart);
        var venus   = Longitude(Factors.Venus, chart);
        var jupiter = Longitude(Factors.Jupiter, chart);
        var mercury = Longitude(Factors.Mercury, chart);
        var mars    = Longitude(Factors.Mars, chart);
        var saturn  = Longitude(Factors.Saturn, chart);

        var fortune   = Normalize(ascendant + (useNightFormula ? sun - moon : moon - sun));
        var spirit    = Normalize(ascendant + (useNightFormula ? moon - sun : sun - moon));
        var eros      = Normalize(ascendant + (useNightFormula ? spirit - venus : venus - spirit));
        var victory   = Normalize(ascendant + (useNightFormula ? spirit - jupiter : jupiter - spirit));
        var necessity = Normalize(ascendant + (useNightFormula ? mercury - fortune : fortune - mercury));
        var courage   = Normalize(ascendant + (useNightFormula ? mars - fortune : fortune - mars));
        var nemesis   = Normalize(ascendant + (useNightFormula ? saturn - fortune : fortune - saturn));

        return
        [
            new LotResult(LotType.Fortune, fortune),
            new LotResult(LotType.Spirit, spirit),
            new LotResult(LotType.Eros, eros),
            new LotResult(LotType.Victory, victory),
            new LotResult(LotType.Necessity, necessity),
            new LotResult(LotType.Courage, courage),
            new LotResult(LotType.Nemesis, nemesis),
        ];
    }

    private static double Longitude(Factors factor, FullChart chart) =>
        chart.Coordinates.TryGetValue(factor, out var pos) && pos.Ecliptical.Length > 0
            ? pos.Ecliptical[0].MainPos
            : 0.0;

    private static double Normalize(double value) => RangeUtil.ValueToRange(value, 0.0, 360.0);
}
