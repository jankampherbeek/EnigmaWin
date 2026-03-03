using System.Collections.Generic;
using System.Globalization;
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

            if (fullPosition.Ecliptical.Length == 0 || fullPosition.Equatorial.Length == 0 || fullPosition.Horizontal.Length == 0)
            {
                continue;
            }

            var ecliptical = fullPosition.Ecliptical[0];
            var equatorial = fullPosition.Equatorial[0];
            var horizontal = fullPosition.Horizontal[0];

            result.Add(new PlanetPositionRow(
                Planet: factor.ToString(),
                Longitude: PositionInDegreesConversion.DoubleToDms(ecliptical.MainPos),
                Latitude: PositionInDegreesConversion.DoubleToDms(ecliptical.Deviation),
                RightAscension: PositionInDegreesConversion.DoubleToDms(equatorial.MainPos),
                Declination: PositionInDegreesConversion.DoubleToDms(equatorial.Deviation),
                Distance: ecliptical.Distance.ToString("F6", CultureInfo.InvariantCulture),
                Azimuth: PositionInDegreesConversion.DoubleToDms(horizontal.Azimuth),
                Altitude: PositionInDegreesConversion.DoubleToDms(horizontal.Altitude)
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
}
