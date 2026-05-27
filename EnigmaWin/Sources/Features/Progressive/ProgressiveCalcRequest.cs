// ProgressiveCalcRequest.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive;

/// <summary>
/// Request for a progressive calculation.
/// Contains the Julian Day of the event, the progressive method, and — for
/// age-based techniques (e.g. secondary directions) — the natal Julian Day.
/// </summary>
public sealed record ProgressiveCalcRequest(
    double JulianDay,
    ProgressiveMethods Method,
    double? NatalJulianDay = null);
