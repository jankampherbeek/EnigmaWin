// ProjectionTypes.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Projection types for chart calculations.</summary>
public enum ProjectionTypes
{
    TwoDimensional = 0,
    ObliqueLongitude = 1
}

/// <summary>Extension methods for the ProjectionTypes enum.</summary>
public static class ProjectionTypesExtensions
{
    /// <summary>Localized name key for this projection type.</summary>
    /// <param name="projectionType">The projection type.</param>
    /// <returns>The localized name key.</returns>
    public static string LocalizedName(this ProjectionTypes projectionType)
    {
        return projectionType switch
        {
            ProjectionTypes.TwoDimensional => "enum.projectiontype.twodimensional",
            ProjectionTypes.ObliqueLongitude => "enum.projectiontype.obliquelongitude",
            _ => string.Empty
        };
    }

    /// <summary>Get a ProjectionTypes enum value from an index.</summary>
    /// <param name="index">The index (0-based).</param>
    /// <returns>The ProjectionTypes enum value, or null if the index is out of range.</returns>
    public static ProjectionTypes? FromIndex(int index)
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        if (index < 0 || index >= allCases.Length)
        {
            return null;
        }
        return allCases[index];
    }
}

