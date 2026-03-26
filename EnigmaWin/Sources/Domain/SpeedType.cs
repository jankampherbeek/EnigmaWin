// SpeedType.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

namespace EnigmaWin.Sources.Domain;

/// <summary>Type of movement speed for a celestial factor.</summary>
public enum SpeedType
{
    Direct = 0,
    Retrograde = 1,
    Slow = 2,
    SlowRetrograde = 3,
    StationaryRetrograde = 4,
    StationaryDirect = 5
}

public static class SpeedTypeExtensions
{
    /// <summary>Resource bundle key for localized name.</summary>
    public static string RbKey(this SpeedType speedType) => speedType switch
    {
        SpeedType.Direct               => "enum.speedtype.direct",
        SpeedType.Retrograde           => "enum.speedtype.retrograde",
        SpeedType.Slow                 => "enum.speedtype.slow",
        SpeedType.SlowRetrograde       => "enum.speedtype.slowretrograde",
        SpeedType.StationaryRetrograde => "enum.speedtype.stationaryretrograde",
        SpeedType.StationaryDirect     => "enum.speedtype.stationarydirect",
        _                              => string.Empty
    };

    /// <summary>Short abbreviation used in chart wheels. Direct is not shown.</summary>
    public static string Abbreviation(this SpeedType speedType) => speedType switch
    {
        SpeedType.Direct               => "D",
        SpeedType.Retrograde           => "R",
        SpeedType.Slow                 => "L",
        SpeedType.SlowRetrograde       => "LR",
        SpeedType.StationaryRetrograde => "SR",
        SpeedType.StationaryDirect     => "SD",
        _                              => string.Empty
    };
}
