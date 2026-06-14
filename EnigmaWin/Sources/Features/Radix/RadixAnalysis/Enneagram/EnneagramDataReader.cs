// EnneagramDataReader.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram;

public static class EnneagramDataReader
{
    public static List<EnneagramData> ReadSigns(bool useUpdated) =>
        Read(useUpdated ? "enneagram2012-signs.csv" : "enneagram-signs.csv");

    public static List<EnneagramData> ReadHouses(bool useUpdated) =>
        Read(useUpdated ? "enneagram2012-houses.csv" : "enneagram-houses.csv");

    private static List<EnneagramData> Read(string fileName)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "enneagram", fileName);
        var result = new List<EnneagramData>();

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#')) continue;

            var parts = line.Split(',');
            if (parts.Length != 11)
                throw new FormatException($"Expected 11 values, got {parts.Length}: {line}");

            if (!int.TryParse(parts[0], out var point) || !int.TryParse(parts[1], out var index))
                throw new FormatException($"Invalid point/index in: {line}");

            var factors = new double[9];
            for (var i = 0; i < 9; i++)
            {
                if (!double.TryParse(parts[i + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out factors[i]))
                    throw new FormatException($"Invalid factor at pos {i + 2}: {line}");
            }

            result.Add(new EnneagramData(point, index, factors));
        }

        return result;
    }
}
