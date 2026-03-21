using System;

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

internal static class RadixOverviewModel
{
    internal static string FormattedJulianDay(double julianDay)
        => julianDay.ToString("F4");
}
