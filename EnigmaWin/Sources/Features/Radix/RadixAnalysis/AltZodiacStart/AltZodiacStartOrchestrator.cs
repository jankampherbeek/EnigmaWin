// AltZodiacStartOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart;

/// <summary>
/// Calculates positions in an alternative zodiac: one that starts (0° of the first sign) at the ecliptic
/// longitude of a user-chosen factor instead of at the vernal equinox point.
/// </summary>
public static class AltZodiacStartOrchestrator
{
    /// <summary>Converts a real ecliptic longitude into its position in the alternative zodiac.</summary>
    /// <param name="longitude">Real ecliptic longitude, 0 &lt;= value &lt; 360.</param>
    /// <param name="zodiacStart">Real ecliptic longitude that defines 0° of the alternative zodiac.</param>
    /// <returns>
    /// The longitude within the alternative zodiac (0 &lt;= value &lt; 360), where every 30° step corresponds to one
    /// of the 12 traditional signs, starting with Aries.
    /// </returns>
    public static double ShiftedLongitude(double longitude, double zodiacStart) =>
        WheelGeometry.Normalise(longitude - zodiacStart);

    /// <summary>
    /// Replaces the position texts of the plotted factors (e.g. "12°34'") with their positions in the alternative
    /// zodiac. Glyph placement is left untouched: it depends on the real longitude, not on the zodiac's start.
    /// </summary>
    public static WheelPlotData WithShiftedPositionTexts(WheelPlotData data, double zodiacStart) => data with
    {
        PlanetItems = data.PlanetItems
            .Select(item => item with
            {
                PositionText = PositionText(ShiftedLongitude(item.EclipticLongitude, zodiacStart), item.SpeedType)
            })
            .ToArray()
    };

    /// <summary>Degrees and minutes within the sign, plus a speed marker (e.g. "R") unless the factor is direct.</summary>
    public static string PositionText(double longitude, SpeedType speedType)
    {
        var text = DrawCusps.CuspPositionText(longitude);
        return speedType == SpeedType.Direct ? text : $"{text} {speedType.Abbreviation()}";
    }

    /// <summary>
    /// Ecliptic longitude of a factor in the chart. Mundane points (Ascendant, MC, East Point, Vertex) are taken
    /// from the house positions; all other factors from their calculated ecliptic position.
    /// </summary>
    /// <returns>The longitude, or null when the chart has no position for the factor.</returns>
    public static double? LongitudeOf(Factors factor, FullChart chart)
    {
        if (factor.CalculationType() == CalculationTypes.Mundane)
        {
            var hp = chart.HousePositions;
            return factor switch
            {
                Factors.Ascendant => hp.Ascendant.Longitude,
                Factors.Mc        => hp.Midheaven.Longitude,
                Factors.EastPoint => hp.Eastpoint.Longitude,
                Factors.Vertex    => hp.Vertex.Longitude,
                _                 => null
            };
        }
        return chart.Coordinates.TryGetValue(factor, out var position) && position.Ecliptical.Length > 0
            ? position.Ecliptical[0].MainPos
            : null;
    }
}
