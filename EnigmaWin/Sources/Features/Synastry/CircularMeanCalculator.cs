// CircularMeanCalculator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Synastry;

/// <summary>Computes the circular mean of a set of angles (degrees), used for blending
/// ecliptic longitudes and mundane angles across multiple charts (e.g. a Synastry composite).</summary>
public static class CircularMeanCalculator
{
    /// <summary>Returns the circular mean of the given angles in degrees, normalized to [0, 360).
    /// Computed via unit-vector averaging so wraparound near 0°/360° is handled correctly.</summary>
    public static double Calculate(IEnumerable<double> anglesDegrees)
    {
        var sumX = 0.0;
        var sumY = 0.0;
        var count = 0;

        foreach (var angle in anglesDegrees)
        {
            var rad = angle * Math.PI / 180.0;
            sumX += Math.Cos(rad);
            sumY += Math.Sin(rad);
            count++;
        }

        if (count == 0) return 0.0;

        var meanRad = Math.Atan2(sumY / count, sumX / count);
        var meanDeg = meanRad * 180.0 / Math.PI;
        return meanDeg < 0 ? meanDeg + 360.0 : meanDeg;
    }
}
