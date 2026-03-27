// DisplayConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>The visual layout style for the astrological chart drawing.</summary>
public enum DrawingTypes
{
    SignBased = 0,
    HouseBased = 1,
    French = 2,
    Ring = 3,
    Dial360 = 4,
    Dial90 = 5,
    Dial45 = 6
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
        DrawingTypes.Ring       => "enum.drawingtype.ring",
        DrawingTypes.Dial360    => "enum.drawingtype.dial360",
        DrawingTypes.Dial90     => "enum.drawingtype.dial90",
        DrawingTypes.Dial45     => "enum.drawingtype.dial45",
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
