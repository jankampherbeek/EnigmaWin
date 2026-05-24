// RadixOverviewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

internal static class RadixOverviewModel
{
    internal static string FormattedJulianDay(double julianDay)
        => julianDay.ToString("F4");
}
