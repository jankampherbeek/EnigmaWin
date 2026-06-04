// AgePointWheelMark.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

/// <summary>One mark on the AgePoint wheel overlay (tick + age label).</summary>
public sealed record AgePointWheelMark(
    string Label,
    double MundaneAngle);
