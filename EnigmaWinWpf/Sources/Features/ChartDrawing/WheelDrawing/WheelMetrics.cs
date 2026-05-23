// WheelMetrics.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Radius fractions, font sizes, and stroke widths for wheel layouts.
/// All values are fractions of the outer radius unless noted otherwise.
/// </summary>
public static class WheelMetrics
{
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
    public const double InnerConnection   = 0.69;
    public const double OuterAspect       = 0.44;
    public const double Vsp               = 0.39;

    public const double SignGlyphFontFraction   = 0.080;
    public const double PlanetGlyphFontFraction = 0.069;
    public const double CardinalFontFraction    = 0.046;
    public const double PositionTextFraction    = 0.029;
    public const double VspTextFraction         = 0.043;

    public const double StrokeFraction      = 0.0057;
    public const double ConnectLineFraction = 0.0029;
    public const double AspectLineFraction  = 0.017;

    public const double CuspLineOpacity       = 0.5;
    public const double ConnectLineOpacity    = 0.25;
    public const double AspectOpacity         = 0.4;
    public const double ElementSectorOpacity  = 0.4;

    public const double MinGlyphDistance = 6.0;

    public static double Radius(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double FontSize(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double StrokeWidth(double fraction, double outerRadius) =>
        outerRadius * fraction;
}
