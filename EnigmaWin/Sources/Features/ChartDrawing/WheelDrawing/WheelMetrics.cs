// WheelMetrics.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Radius fractions, font sizes, and stroke widths for wheel layouts.
/// All values are fractions of the outer radius unless noted otherwise.
/// </summary>
public static class WheelMetrics
{
    // MARK: - Radius fractions (relative to outerRadius)
    public const double OuterCircle       = 0.98;
    public const double CardinalIndicator = 0.93;
    public const double OuterSign         = 0.89;
    public const double SignGlyph         = 0.84;
    public const double OuterHouse        = 0.79;
    public const double CuspText          = 0.76;
    public const double Degrees           = 0.775;
    public const double Degrees5          = 0.760;
    public const double PlanetText        = 0.64;
    public const double PlanetGlyph       = 0.54;
    public const double OuterConnection   = 0.48;
    public const double OuterAspect       = 0.44;
    public const double Vsp               = 0.39;

    // MARK: - Font size fractions (relative to outerRadius)
    public const double SignGlyphFontFraction   = 0.080;   // ~28px at 350px radius
    public const double PlanetGlyphFontFraction = 0.069;   // ~24px at 350px radius
    public const double CardinalFontFraction    = 0.046;   // ~16px at 350px radius
    public const double PositionTextFraction    = 0.029;   // ~10px at 350px radius
    public const double VspTextFraction         = 0.043;   // ~15px at 350px radius

    // MARK: - Stroke width fractions (relative to outerRadius)
    public const double StrokeFraction      = 0.0057;  // ~2px at 350px radius
    public const double ConnectLineFraction = 0.0029;  // ~1px at 350px radius
    public const double AspectLineFraction  = 0.017;   // ~6px at 350px radius

    // MARK: - Opacity
    public const double CuspLineOpacity    = 0.5;
    public const double ConnectLineOpacity = 0.25;
    public const double AspectOpacity      = 0.4;
    public const double ElementSectorOpacity = 0.4;

    // MARK: - Anti-overlap
    public const double MinGlyphDistance = 6.0;   // degrees

    // MARK: - Computed values for a given outerRadius

    public static double Radius(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double FontSize(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double StrokeWidth(double fraction, double outerRadius) =>
        outerRadius * fraction;
}
