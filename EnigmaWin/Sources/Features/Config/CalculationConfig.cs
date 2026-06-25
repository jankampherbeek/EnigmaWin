// CalculationConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Configuration for astrological calculation settings.</summary>
public readonly record struct CalculationConfig(
    HouseSystems HouseSystem,
    Ayanamshas Ayanamsha,
    ObserverPositions ObserverPosition,
    ProjectionTypes ProjectionType,
    LunarNodeTypes LunarNodeType,
    LotsTypes LotsType,
    int StationaryPercentage = 10,
    int SlowPercentage = 20,
    double HomeLatitude = 0.0,
    double HomeLongitude = 0.0)
{
    /// <summary>Default configuration (Placidus, Tropical, Geocentric, etc.).</summary>
    public static CalculationConfig Default => new(
        HouseSystems.Placidus,
        Ayanamshas.Tropical,
        ObserverPositions.Geocentric,
        ProjectionTypes.TwoDimensional,
        LunarNodeTypes.MeanNode,
        LotsTypes.Sect,
        StationaryPercentage: 10,
        SlowPercentage: 20,
        HomeLatitude: 0.0,
        HomeLongitude: 0.0);
}
