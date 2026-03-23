// ProgressionsConfig.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Container for all progression and direction configurations.</summary>
public readonly record struct ProgressionsConfig(
    PrimaryDirectionsConfig PrimaryDirections,
    TransitsConfig Transits,
    SecondaryDirectionsConfig SecondaryDirections,
    SymbolicDirectionsConfig SymbolicDirections,
    SolarReturnConfig SolarReturn)
{
    public static ProgressionsConfig Default => new(
        PrimaryDirectionsConfig.Default,
        TransitsConfig.Default,
        SecondaryDirectionsConfig.Default,
        SymbolicDirectionsConfig.Default,
        SolarReturnConfig.Default);
}
