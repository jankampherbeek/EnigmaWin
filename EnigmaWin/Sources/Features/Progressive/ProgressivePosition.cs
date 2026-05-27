// ProgressivePosition.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive;

/// <summary>Ecliptical longitude and equatorial declination for one factor in a progressive calculation.</summary>
/// <param name="Longitude">Ecliptical longitude in degrees (0–360).</param>
/// <param name="Declination">Declination in degrees; negative for southern declination.</param>
public sealed record ProgressivePosition(double Longitude, double Declination);
