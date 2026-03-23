// DisplayConfig.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>The visual layout style for the astrological chart drawing.</summary>
public enum DrawingTypes
{
    SignBased = 0,
    HouseBased = 1,
    French = 2
}

/// <summary>Extension methods for the DrawingTypes enum.</summary>
public static class DrawingTypesExtensions
{
    /// <summary>Localized name key for this drawing type.</summary>
    public static string LocalizedName(this DrawingTypes dt) => dt switch
    {
        DrawingTypes.SignBased  => "enum.drawingtype.signbased",
        DrawingTypes.HouseBased => "enum.drawingtype.housebased",
        DrawingTypes.French     => "enum.drawingtype.french",
        _                       => string.Empty
    };
}

/// <summary>Color override for a single zodiac sign.</summary>
public readonly record struct SignColorOverride(Signs Sign, ColorConfig Color);

/// <summary>Configuration for display and visual settings.</summary>
public readonly record struct DisplayConfig(
    DrawingTypes DrawingType,
    IReadOnlyList<SignColorOverride> SignColors)
{
    public static DisplayConfig Default => new(
        DrawingTypes.SignBased,
        []);
}
