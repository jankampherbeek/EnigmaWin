using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public sealed class RadixPositionsModel
{
    public sealed record PlanetPositionRow(
        string Glyph,
        string LongitudeDms,
        string LongitudeSignGlyph,
        string Latitude,
        string RightAscension,
        string Declination,
        string Distance,
        string Azimuth,
        string Altitude);

    public sealed record CuspPositionRow(
        string Cusp,
        string LongitudeDms,
        string LongitudeSignGlyph,
        string RightAscension,
        string Declination,
        string Azimuth,
        string Altitude);

    public (IReadOnlyList<PlanetPositionRow> PlanetRows, IReadOnlyList<CuspPositionRow> CuspRows) BuildRows(FullChart chart, FactorConfig? factorConfig = null)
    {
        var planetRows = BuildPlanetRows(chart, factorConfig);
        var cuspRows = BuildCuspRows(chart);
        return (planetRows, cuspRows);
    }

    private static List<PlanetPositionRow> BuildPlanetRows(FullChart chart, FactorConfig? factorConfig)
    {
        IEnumerable<Factors> orderedFactors;
        if (factorConfig != null)
        {
            orderedFactors = factorConfig.Value.Settings
                .Where(s => s.IsUsed)
                .Select(s => s.Factor);
        }
        else
        {
            orderedFactors = new[]
            {
                Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus,
                Factors.Mars, Factors.Jupiter, Factors.Saturn, Factors.Pluto
            };
        }

        var result = new List<PlanetPositionRow>();
        foreach (var factor in orderedFactors)
        {
            if (!chart.Coordinates.TryGetValue(factor, out var fullPosition))
                continue;

            var ecliptical = fullPosition.Ecliptical.FirstOrDefault();
            var equatorial = fullPosition.Equatorial.FirstOrDefault();
            var horizontal = fullPosition.Horizontal.FirstOrDefault();

            if (ecliptical == null && equatorial == null && horizontal == null)
                continue;

            var (longitudeDms, longitudeSignGlyph) = FormatLongitude(ecliptical?.MainPos);

            result.Add(new PlanetPositionRow(
                Glyph:              GlyphSelector.GetGlyphForFactor(factor),
                LongitudeDms:       longitudeDms,
                LongitudeSignGlyph: longitudeSignGlyph,
                Latitude:           FormatDms(ecliptical?.Deviation),
                RightAscension:     FormatDms(equatorial?.MainPos),
                Declination:        FormatDms(equatorial?.Deviation),
                Distance:           FormatDistance(ecliptical?.Distance),
                Azimuth:            FormatDms(horizontal?.Azimuth),
                Altitude:           FormatDms(horizontal?.Altitude)
            ));
        }

        return result;
    }

    private static List<CuspPositionRow> BuildCuspRows(FullChart chart)
    {
        var result = new List<CuspPositionRow>();
        for (var i = 0; i < chart.HousePositions.Cusps.Length; i++)
        {
            var cusp = chart.HousePositions.Cusps[i];
            var (longitudeDms, longitudeSignGlyph) = FormatLongitude(cusp.Longitude);

            result.Add(new CuspPositionRow(
                Cusp:               (i + 1).ToString(),
                LongitudeDms:       longitudeDms,
                LongitudeSignGlyph: longitudeSignGlyph,
                RightAscension:     PositionInDegreesConversion.DoubleToDms(cusp.RightAscension),
                Declination:        PositionInDegreesConversion.DoubleToDms(cusp.Declination),
                Azimuth:            PositionInDegreesConversion.DoubleToDms(cusp.Horizontal.Azimuth),
                Altitude:           PositionInDegreesConversion.DoubleToDms(cusp.Horizontal.Altitude)
            ));
        }

        return result;
    }

    private static (string Dms, string SignGlyph) FormatLongitude(double? value)
    {
        if (!value.HasValue)
            return ("-", string.Empty);

        var (dmsString, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(value.Value);
        if (!success || sign == null)
            return (PositionInDegreesConversion.DoubleToDms(value.Value), string.Empty);

        return (dmsString, GlyphSelector.GetGlyphForSign(sign.Value));
    }

    private static string FormatDms(double? value) =>
        value.HasValue ? PositionInDegreesConversion.DoubleToDms(value.Value) : "-";

    private static string FormatDistance(double? value) =>
        value.HasValue ? value.Value.ToString("F6", CultureInfo.InvariantCulture) : "-";
}
