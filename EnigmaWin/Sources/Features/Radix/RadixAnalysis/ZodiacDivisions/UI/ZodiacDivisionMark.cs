// ZodiacDivisionMark.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;

/// <summary>The four division glyphs for one factor, positioned at its mundane angle on the wheel.</summary>
public record ZodiacDivisionMark(
    double MundaneAngle,
    string SignGlyph,
    string DecanGlyph,
    string DodecatGlyph,
    string BoundGlyph);
