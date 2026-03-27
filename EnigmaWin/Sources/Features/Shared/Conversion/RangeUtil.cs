// RangeUtil.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Shared.Conversion;

/// <summary>Utility functions for range operations.</summary>
public static class RangeUtil
{
    /// <summary>Normalize a value to a range [lowerLimit, upperLimit).</summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="lowerLimit">The lower limit (inclusive).</param>
    /// <param name="upperLimit">The upper limit (exclusive).</param>
    /// <returns>The normalized value within the specified range.</returns>
    public static double ValueToRange(double value, double lowerLimit, double upperLimit)
    {
        var range = upperLimit - lowerLimit;
        var normalized = ((value - lowerLimit) % range + range) % range + lowerLimit;
        return normalized;
    }
}
