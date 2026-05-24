// FrenchWheelMetrics.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Radius fractions for the French-style wheel layout.
/// All values are fractions of the outer radius unless noted otherwise.
/// In the French wheel the zodiac sign ring is narrow and sits inside the house ring;
/// planets are drawn farther out (beyond the house ring), and degree ticks project
/// outward from the zodiac ring boundary.
/// </summary>
public static class FrenchWheelMetrics
{
    public const double OuterCircle = 0.98;
    public const double OuterHouse  = 0.78;
    public const double DegreeR     = 0.67;
    public const double OuterSign   = 0.60;
    public const double OuterAspect = 0.36;

    public const double PlanetGlyph = 0.83;
    public const double PlanetText  = 0.92;

    public const double Tick1 = 0.018;
    public const double Tick5 = 0.040;

    public static double SignGlyph => (OuterAspect + OuterSign) / 2.0;
    public static double HouseNumber => (DegreeR + OuterHouse) / 2.0;
    public static double CardinalGlyph => (DegreeR + OuterHouse) / 2.0;

    public static double Radius(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double FontSize(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double StrokeWidth(double fraction, double outerRadius) =>
        outerRadius * fraction;
}
