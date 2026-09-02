// ProgressionsConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Container for all progression and direction configurations.</summary>
public readonly record struct ProgressionsConfig(
    PrimaryDirectionsConfig PrimaryDirections,
    TransitsConfig Transits,
    SecondaryDirectionsConfig SecondaryDirections,
    SymbolicDirectionsConfig SymbolicDirections,
    SolarReturnConfig SolarReturn,
    ProgressiveCalendarConfig ProgressiveCalendar)
{
    public static ProgressionsConfig Default => new(
        PrimaryDirectionsConfig.Default,
        TransitsConfig.Default,
        SecondaryDirectionsConfig.Default,
        SymbolicDirectionsConfig.Default,
        SolarReturnConfig.Default,
        ProgressiveCalendarConfig.Default);
}
