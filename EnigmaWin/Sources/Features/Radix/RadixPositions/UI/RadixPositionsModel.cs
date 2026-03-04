using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public sealed class RadixPositionsModel
{
    public sealed record PlanetPositionRow(
        string Planet,
        string Longitude,
        string Latitude,
        string RightAscension,
        string Declination,
        string Distance,
        string Azimuth,
        string Altitude);

    public sealed record CuspPositionRow(
        string Cusp,
        string Longitude,
        string RightAscension,
        string Declination,
        string Azimuth,
        string Altitude);

    public (IReadOnlyList<PlanetPositionRow> PlanetRows, IReadOnlyList<CuspPositionRow> CuspRows) BuildRows(FullChart chart)
    {
        var planetRows = BuildPlanetRows(chart);
        var cuspRows = BuildCuspRows(chart);
        return (planetRows, cuspRows);
    }

    private static List<PlanetPositionRow> BuildPlanetRows(FullChart chart)
    {
        var orderedFactors = new[]
        {
            Factors.Sun,
            Factors.Moon,
            Factors.Mercury,
            Factors.Venus,
            Factors.Mars,
            Factors.Jupiter,
            Factors.Saturn,
            Factors.Pluto
        };

        var result = new List<PlanetPositionRow>();
        foreach (var factor in orderedFactors)
        {
            if (!chart.Coordinates.TryGetValue(factor, out var fullPosition))
            {
                continue;
            }

            var ecliptical = fullPosition.Ecliptical.FirstOrDefault();
            var equatorial = fullPosition.Equatorial.FirstOrDefault();
            var horizontal = fullPosition.Horizontal.FirstOrDefault();

            if (ecliptical == null && equatorial == null && horizontal == null)
            {
                continue;
            }

            result.Add(new PlanetPositionRow(
                Planet: factor.ToString(),
                Longitude: FormatDms(ecliptical?.MainPos),
                Latitude: FormatDms(ecliptical?.Deviation),
                RightAscension: FormatDms(equatorial?.MainPos),
                Declination: FormatDms(equatorial?.Deviation),
                Distance: FormatDistance(ecliptical?.Distance),
                Azimuth: FormatDms(horizontal?.Azimuth),
                Altitude: FormatDms(horizontal?.Altitude)
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
            result.Add(new CuspPositionRow(
                Cusp: $"Cusp {i + 1}",
                Longitude: PositionInDegreesConversion.DoubleToDms(cusp.Longitude),
                RightAscension: PositionInDegreesConversion.DoubleToDms(cusp.RightAscension),
                Declination: PositionInDegreesConversion.DoubleToDms(cusp.Declination),
                Azimuth: PositionInDegreesConversion.DoubleToDms(cusp.Horizontal.Azimuth),
                Altitude: PositionInDegreesConversion.DoubleToDms(cusp.Horizontal.Altitude)
            ));
        }

        return result;
    }

    private static string FormatDms(double? value)
    {
        return value.HasValue
            ? PositionInDegreesConversion.DoubleToDms(value.Value)
            : "-";
    }

    private static string FormatDistance(double? value)
    {
        return value.HasValue
            ? value.Value.ToString("F6", CultureInfo.InvariantCulture)
            : "-";
    }
}
