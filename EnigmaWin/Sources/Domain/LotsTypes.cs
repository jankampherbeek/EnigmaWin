// LotsTypes.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Lots types for astrological calculations.</summary>
public enum LotsTypes
{
    Sect = 0,
    NoSect = 1
}

/// <summary>Extension methods for the LotsTypes enum.</summary>
public static class LotsTypesExtensions
{
    /// <summary>Localized name key for this lots type.</summary>
    /// <param name="lotsType">The lots type.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this LotsTypes lotsType)
    {
        return lotsType switch
        {
            LotsTypes.Sect => "enum.lotstypes.sect",
            LotsTypes.NoSect => "enum.lotstypes.nosect",
            _ => string.Empty
        };
    }

    /// <summary>Find lots type for a given index.</summary>
    /// <param name="index">The index (raw value).</param>
    /// <returns>The lots type if found, null otherwise.</returns>
    public static LotsTypes? FromIndex(int index)
    {
        return Enum.IsDefined(typeof(LotsTypes), index)
            ? (LotsTypes)index
            : null;
    }
}
