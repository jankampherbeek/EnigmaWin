// ColorConfig.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

namespace EnigmaWin.Sources.Features.Config;

/// <summary>A serializable RGBA color for use in configuration and persistence.</summary>
/// <remarks>Component values are in the 0–1 range.</remarks>
public readonly record struct ColorConfig(
    double Red,
    double Green,
    double Blue,
    double Opacity = 1.0)
{
    /// <summary>Light blue — default background color for zodiac signs.</summary>
    public static ColorConfig DefaultSignColor => new(0.678, 0.847, 0.902);
}
