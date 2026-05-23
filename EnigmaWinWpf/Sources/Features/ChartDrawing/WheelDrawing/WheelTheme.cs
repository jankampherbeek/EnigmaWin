// WheelTheme.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows.Media;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Theme wrapper that switches between full-colour and black-and-white rendering.
/// </summary>
public class WheelTheme
{
    public bool IsBlackWhite { get; }

    private WheelTheme(bool isBlackWhite) => IsBlackWhite = isBlackWhite;

    public static readonly WheelTheme Color      = new(false);
    public static readonly WheelTheme BlackWhite = new(true);

    public Color OuterCircleBackground  => IsBlackWhite ? Colors.White : WheelColors.OuterCircleBackground;
    public Color SignRingBackground     => IsBlackWhite ? Colors.White : WheelColors.SignRingBackground;
    public Color HouseRingBackground    => IsBlackWhite ? Colors.White : WheelColors.HouseRingBackground;
    public Color AspectCircleBackground => IsBlackWhite ? Colors.White : WheelColors.AspectCircleBackground;

    public Color CircleStroke     => IsBlackWhite ? Colors.Black : WheelColors.CircleStroke;
    public Color DegreeTickStroke => IsBlackWhite ? Colors.Black : WheelColors.DegreeTickStroke;
    public Color SignSeparator    => IsBlackWhite ? Colors.Black : WheelColors.SignSeparator;

    public Color SignGlyph => IsBlackWhite ? Colors.Black : WheelColors.SignGlyph;

    public Color SignSectorColor(Signs sign) =>
        IsBlackWhite ? Colors.Transparent : WheelColors.ElementColorForSign(sign);

    public Color CuspLine          => IsBlackWhite ? System.Windows.Media.Color.FromRgb(0x99, 0x99, 0x99) : WheelColors.CuspLine;
    public Color CuspText          => IsBlackWhite ? Colors.Black : WheelColors.CuspText;
    public Color CardinalIndicator => IsBlackWhite ? Colors.Black : WheelColors.CardinalIndicator;

    public Color PlanetGlyph       => IsBlackWhite ? Colors.Black : WheelColors.PlanetGlyph;
    public Color PlanetText        => IsBlackWhite ? Colors.Black : WheelColors.PlanetText;
    public Color PlanetConnectLine => IsBlackWhite ? Colors.Black : WheelColors.PlanetConnectLine;

    public Color AspectLineColor(Color original) =>
        IsBlackWhite ? System.Windows.Media.Color.FromRgb(0x40, 0x40, 0x40) : original;
}
