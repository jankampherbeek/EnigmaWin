// CalculationConfig.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Configuration for astrological calculation settings.</summary>
public readonly record struct CalculationConfig(
    HouseSystems HouseSystem,
    Ayanamshas Ayanamsha,
    ObserverPositions ObserverPosition,
    ProjectionTypes ProjectionType,
    BlackMoonCorrectionTypes BlackMoonCorrectionType,
    LunarNodeTypes LunarNodeType,
    LotsTypes LotsType,
    int StationaryPercentage = 10,
    int SlowPercentage = 20)
{
    /// <summary>Default configuration (Placidus, Tropical, Geocentric, etc.).</summary>
    public static CalculationConfig Default => new(
        HouseSystems.Placidus,
        Ayanamshas.Tropical,
        ObserverPositions.Geocentric,
        ProjectionTypes.TwoDimensional,
        BlackMoonCorrectionTypes.Duval,
        LunarNodeTypes.MeanNode,
        LotsTypes.Sect,
        StationaryPercentage: 10,
        SlowPercentage: 20);
}
