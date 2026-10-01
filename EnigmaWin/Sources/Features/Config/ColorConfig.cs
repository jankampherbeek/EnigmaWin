// ColorConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>A serializable RGBA color for use in configuration and persistence.</summary>
/// <remarks>Component values are in the 0–1 range.</remarks>
public readonly record struct ColorConfig(
    double Red,
    double Green,
    double Blue,
    double Opacity = 1.0)
{
    /// <summary>Default background color for a zodiac sign, based on its element.</summary>
    /// <remarks>Opaque equivalents of the element colors (40% opacity) as drawn over the sign ring background.</remarks>
    public static ColorConfig DefaultSignColor(Signs sign) => sign switch
    {
        Signs.Aries or Signs.Leo or Signs.Sagittarius    => FireSignColor,
        Signs.Taurus or Signs.Virgo or Signs.Capricorn   => EarthSignColor,
        Signs.Gemini or Signs.Libra or Signs.Aquarius    => AirSignColor,
        _                                                => WaterSignColor
    };

    private static readonly ColorConfig FireSignColor  = new(207 / 255.0, 143 / 255.0, 143 / 255.0);
    private static readonly ColorConfig EarthSignColor = new(161 / 255.0, 170 / 255.0, 150 / 255.0);
    private static readonly ColorConfig AirSignColor   = new(105 / 255.0, 143 / 255.0, 245 / 255.0);
    private static readonly ColorConfig WaterSignColor = new(105 / 255.0, 214 / 255.0, 174 / 255.0);
}
