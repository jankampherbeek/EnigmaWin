// BlackMoonCorrectionTypes.cs
// EnigmaWin
// Created by Jan Kampherbeek on 24-01-2026

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Black Moon correction types for astrological calculations.</summary>
public enum BlackMoonCorrectionTypes
{
    Duval = 0,
    Swisseph = 1,
    Interpolated = 2
}

/// <summary>Extension methods for the BlackMoonCorrectionTypes enum.</summary>
public static class BlackMoonCorrectionTypesExtensions
{
    /// <summary>Localized name key for this black moon correction type.</summary>
    /// <param name="blackMoonCorrectionType">The black moon correction type.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this BlackMoonCorrectionTypes blackMoonCorrectionType)
    {
        return blackMoonCorrectionType switch
        {
            BlackMoonCorrectionTypes.Duval => "enum.blackmooncorrrectiontype.duval",
            BlackMoonCorrectionTypes.Swisseph => "enum.blackmooncorrectiontype.swisseph",
            BlackMoonCorrectionTypes.Interpolated => "enum.blackmooncorrectiontype.interpolated",
            _ => string.Empty
        };
    }

    /// <summary>Find black moon correction type for a given index.</summary>
    /// <param name="index">The index (raw value).</param>
    /// <returns>The black moon correction type if found, null otherwise.</returns>
    public static BlackMoonCorrectionTypes? FromIndex(int index)
    {
        return Enum.IsDefined(typeof(BlackMoonCorrectionTypes), index)
            ? (BlackMoonCorrectionTypes)index
            : null;
    }
}
