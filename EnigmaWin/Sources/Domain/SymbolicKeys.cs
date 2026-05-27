// SymbolicKeys.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>The method (arc key) used for symbolic directions.</summary>
public enum SymbolicKeys
{
    TrueSun   = 0,
    MeanSun   = 1,
    OneDegree = 2
}

/// <summary>Extension methods for the SymbolicKeys enum.</summary>
public static class SymbolicKeysExtensions
{
    /// <summary>Localized name key for this symbolic direction method.</summary>
    public static string LocalizedName(this SymbolicKeys key)
    {
        return key switch
        {
            SymbolicKeys.TrueSun   => "enum.symbolicmethod.truesun",
            SymbolicKeys.MeanSun   => "enum.symbolicmethod.meansun",
            SymbolicKeys.OneDegree => "enum.symbolicmethod.onedegree",
            _ => string.Empty
        };
    }

    /// <summary>Get a SymbolicKeys enum value from an index.</summary>
    public static SymbolicKeys? FromIndex(int index)
    {
        var allCases = Enum.GetValues<SymbolicKeys>();
        if (index < 0 || index >= allCases.Length)
            return null;
        return allCases[index];
    }
}
