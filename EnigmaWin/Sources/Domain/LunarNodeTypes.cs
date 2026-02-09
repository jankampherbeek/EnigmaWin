// LunarNodeTypes.cs
// EnigmaWin
// Created by Jan Kampherbeek on 24-01-2026

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Lunar node types for astrological calculations.</summary>
public enum LunarNodeTypes
{
    MeanNode = 0,
    TrueNode = 1
}

/// <summary>Extension methods for the LunarNodeTypes enum.</summary>
public static class LunarNodeTypesExtensions
{
    /// <summary>Localized name key for this lunar node type.</summary>
    /// <param name="lunarNodeType">The lunar node type.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this LunarNodeTypes lunarNodeType)
    {
        return lunarNodeType switch
        {
            LunarNodeTypes.MeanNode => "enum.lunarnodetypes.mean",
            LunarNodeTypes.TrueNode => "enum.lunarnodetypes.true",
            _ => string.Empty
        };
    }

    /// <summary>Find lunar node type for a given index.</summary>
    /// <param name="index">The index (raw value).</param>
    /// <returns>The lunar node type if found, null otherwise.</returns>
    public static LunarNodeTypes? FromIndex(int index)
    {
        return Enum.IsDefined(typeof(LunarNodeTypes), index)
            ? (LunarNodeTypes)index
            : null;
    }
}
