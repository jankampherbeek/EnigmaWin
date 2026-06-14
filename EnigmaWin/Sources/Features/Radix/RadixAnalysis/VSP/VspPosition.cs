// VspPosition.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP;

/// <summary>A single Venus Star Point position.</summary>
/// <param name="SequenceId">Chronological sequence number 1–5.</param>
/// <param name="Jd">Julian Day of the exact conjunction.</param>
/// <param name="Phenomenon">Inferior or superior conjunction.</param>
/// <param name="Longitude">Ecliptic longitude of the Sun at conjunction (0–360°).</param>
public record VspPosition(int SequenceId, double Jd, VenusPhenomena Phenomenon, double Longitude);
