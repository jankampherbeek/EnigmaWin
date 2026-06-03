// LtsWheelMark.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;

/// <summary>One mark on the LTS wheel overlay (tick + label).</summary>
public sealed record LtsWheelMark(
    string Label,
    double MundaneAngle,
    bool   IsMonth);   // true = prenatal month, false = age in years
