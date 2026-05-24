// WheelColors.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows.Media;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>Color constants for wheel drawing elements.</summary>
public static class WheelColors
{
    public static readonly Color OuterCircleBackground  = Color.FromRgb(0xF0, 0xF8, 0xFF);
    public static readonly Color SignRingBackground     = Color.FromRgb(0xAF, 0xEE, 0xEE);
    public static readonly Color HouseRingBackground   = Color.FromRgb(0xFF, 0xFA, 0xF0);
    public static readonly Color AspectCircleBackground = Color.FromRgb(0xF0, 0xF8, 0xFF);

    public static readonly Color CircleStroke      = Color.FromRgb(0x33, 0x33, 0xCC);
    public static readonly Color DegreeTickStroke  = Color.FromRgb(0x33, 0x33, 0xCC);

    public static readonly Color SignGlyph      = Color.FromRgb(0x33, 0x33, 0xCC);
    public static readonly Color SignSeparator  = Color.FromRgb(0x33, 0x33, 0xCC);

    public static readonly Color FireElement  = Color.FromArgb(0x66, 0xFF, 0x00, 0x00);
    public static readonly Color EarthElement = Color.FromArgb(0x66, 0x8B, 0x45, 0x13);
    public static readonly Color AirElement   = Color.FromArgb(0x66, 0x00, 0x00, 0xFF);
    public static readonly Color WaterElement = Color.FromArgb(0x66, 0x00, 0xB3, 0x4D);

    public static readonly Color CuspLine          = Color.FromRgb(0x46, 0x82, 0xB4);
    public static readonly Color CuspText          = Color.FromRgb(0x8B, 0x45, 0x13);
    public static readonly Color CardinalIndicator = Color.FromRgb(0x8B, 0x45, 0x13);

    public static readonly Color PlanetGlyph       = Color.FromRgb(0x2F, 0x4F, 0x4F);
    public static readonly Color PlanetText        = Color.FromRgb(0x2F, 0x4F, 0x4F);
    public static readonly Color PlanetConnectLine = Color.FromRgb(0x2F, 0x4F, 0x4F);

    public static readonly Color HardAspect   = Colors.Red;
    public static readonly Color SoftAspect   = Colors.Green;
    public static readonly Color MinorAspect  = Colors.Gray;
    public static readonly Color Inconjunct   = Colors.Purple;

    public static Color ElementColor(int signIndex) => (signIndex % 4) switch
    {
        0 => FireElement,
        1 => EarthElement,
        2 => AirElement,
        3 => WaterElement,
        _ => FireElement
    };

    public static Color ElementColorForSign(Signs sign) =>
        ElementColor((int)sign - 1);
}
