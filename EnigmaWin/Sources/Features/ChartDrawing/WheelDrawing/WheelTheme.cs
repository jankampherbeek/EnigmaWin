// WheelTheme.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Media;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Theme wrapper that switches between full-colour and black-and-white rendering.
/// Pass the relevant theme instance to all Draw* helpers.
/// </summary>
public class WheelTheme
{
    public bool IsBlackWhite { get; }

    private WheelTheme(bool isBlackWhite) => IsBlackWhite = isBlackWhite;

    public static readonly WheelTheme Color      = new(false);
    public static readonly WheelTheme BlackWhite = new(true);

    // MARK: - Circle fills
    public Color OuterCircleBackground  => IsBlackWhite ? Colors.White : WheelColors.OuterCircleBackground;
    public Color SignRingBackground     => IsBlackWhite ? Colors.White : WheelColors.SignRingBackground;
    public Color HouseRingBackground    => IsBlackWhite ? Colors.White : WheelColors.HouseRingBackground;
    public Color AspectCircleBackground => IsBlackWhite ? Colors.White : WheelColors.AspectCircleBackground;

    // MARK: - Strokes
    public Color CircleStroke     => IsBlackWhite ? Colors.Black : WheelColors.CircleStroke;
    public Color DegreeTickStroke => IsBlackWhite ? Colors.Black : WheelColors.DegreeTickStroke;
    public Color SignSeparator    => IsBlackWhite ? Colors.Black : WheelColors.SignSeparator;

    // MARK: - Signs
    public Color SignGlyph => IsBlackWhite ? Colors.Black : WheelColors.SignGlyph;

    public Color SignSectorColor(Signs sign) =>
        IsBlackWhite ? Colors.Transparent : WheelColors.ElementColorForSign(sign);

    // MARK: - Cusps
    public Color CuspLine          => IsBlackWhite ? Avalonia.Media.Color.FromRgb(0x99, 0x99, 0x99) : WheelColors.CuspLine;
    public Color CuspText          => IsBlackWhite ? Colors.Black : WheelColors.CuspText;
    public Color CardinalIndicator => IsBlackWhite ? Colors.Black : WheelColors.CardinalIndicator;

    // MARK: - Planets
    public Color PlanetGlyph       => IsBlackWhite ? Colors.Black : WheelColors.PlanetGlyph;
    public Color PlanetText        => IsBlackWhite ? Colors.Black : WheelColors.PlanetText;
    public Color PlanetConnectLine => IsBlackWhite ? Colors.Black : WheelColors.PlanetConnectLine;

    // MARK: - Aspects
    public Color AspectLineColor(Color original) =>
        IsBlackWhite ? Avalonia.Media.Color.FromRgb(0x40, 0x40, 0x40) : original;
}
