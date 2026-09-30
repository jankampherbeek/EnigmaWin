// DstCertainty.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Domain;

/// <summary>
/// Flags date/location combinations where the historical DST rule cannot be trusted.
/// DST in the US was a local (city/state) option before the Uniform Time Act took effect in 1967,
/// so pre-1967 US DST data is unreliable regardless of what the bundled timezone data resolves to.
/// </summary>
public static class DstCertainty
{
    /// <param name="countryCode">ISO country code of the selected city, or null when no city is selected.</param>
    /// <param name="year">The year of the date.</param>
    public static bool IsUncertain(string? countryCode, int year) => countryCode == "US" && year < 1967;
}
