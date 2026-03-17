// ConfigData.cs
// EnigmaWin
// Created by Jan Kampherbeek on 24-01-2026

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Configuration data for astrological calculations (mirrors Swift ConfigData struct).</summary>
public readonly record struct ConfigData(
    HouseSystems HouseSystem,
    Ayanamshas Ayanamsha,
    ObserverPositions ObserverPosition,
    ProjectionTypes ProjectionType,
    BlackMoonCorrectionTypes BlackMoonCorrectionType,
    LunarNodeTypes LunarNodeType,
    LotsTypes LotsType,
    string Language)
{
    /// <summary>Default configuration (Placidus, Tropical, Geocentric, etc.) for tests and initial UI state.</summary>
    /// <remarks>Language is empty by default, which means the system locale is used.</remarks>
    public static ConfigData Default => new(
        HouseSystems.Placidus,
        Ayanamshas.Tropical,
        ObserverPositions.Geocentric,
        ProjectionTypes.TwoDimensional,
        BlackMoonCorrectionTypes.Duval,
        LunarNodeTypes.MeanNode,
        LotsTypes.Sect,
        Language: string.Empty);
}
