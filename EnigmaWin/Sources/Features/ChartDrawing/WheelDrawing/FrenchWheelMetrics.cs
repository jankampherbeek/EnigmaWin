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
    // MARK: - Radius fractions (relative to outerRadius)
    public const double OuterCircle = 0.98;  // outer wheel boundary
    public const double OuterHouse  = 0.78;  // outer edge of house ring
    public const double DegreeR     = 0.67;  // inner edge of house ring / outer edge of degree tick zone
    public const double OuterSign   = 0.60;  // outer edge of zodiac sign ring
    public const double OuterAspect = 0.36;  // outer edge of aspect circle

    public const double PlanetGlyph = 0.83;  // planet glyph radius (outside the house ring)
    public const double PlanetText  = 0.92;  // planet position text radius

    // Degree tick lengths (added to OuterSign outward)
    public const double Tick1 = 0.018; // short (1°) tick
    public const double Tick5 = 0.040; // long  (5°) tick

    // Sign glyph sits at the midpoint of the sign ring
    public static double SignGlyph => (OuterAspect + OuterSign) / 2.0;

    // House numbers sit at the midpoint of the house ring
    public static double HouseNumber => (DegreeR + OuterHouse) / 2.0;

    // Cardinal glyphs (ASC/MC) also in the house ring midpoint
    public static double CardinalGlyph => (DegreeR + OuterHouse) / 2.0;

    // MARK: - Computed values for a given outerRadius
    public static double Radius(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double FontSize(double fraction, double outerRadius) =>
        outerRadius * fraction;

    public static double StrokeWidth(double fraction, double outerRadius) =>
        outerRadius * fraction;
}
