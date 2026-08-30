// CountingsDomain.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Countings;

/// <summary>The 3 crosses and 4 elements shown in the Countings screen.</summary>
public enum CountingsGroup
{
    Cardinal, Fixed, Mutable, Fire, Earth, Air, Water
}

/// <summary>One row of the Elements or Crosses count table: how many active factors fall in a sign
/// belonging to this group.</summary>
public sealed record CountingsLine(CountingsGroup Group, int Count);

public static class CountingsDomain
{
    public static CountingsGroup? CrossFor(int sign) => sign switch
    {
        1 or 4 or 7 or 10 => CountingsGroup.Cardinal,
        2 or 5 or 8 or 11 => CountingsGroup.Fixed,
        3 or 6 or 9 or 12 => CountingsGroup.Mutable,
        _ => null
    };

    public static CountingsGroup? ElementFor(int sign) => sign switch
    {
        1 or 5 or 9 => CountingsGroup.Fire,
        2 or 6 or 10 => CountingsGroup.Earth,
        3 or 7 or 11 => CountingsGroup.Air,
        4 or 8 or 12 => CountingsGroup.Water,
        _ => null
    };
}
