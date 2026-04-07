// DeclDiagramItem.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

/// <summary>Single factor entry displayed in the declination diagram and its position table.</summary>
public record DeclDiagramItem(
    Factors Factor,
    string  Glyph,
    double  Longitude,     // ecliptic longitude 0–360
    double  Declination,   // positive = north, negative = south
    string  LongitudeDms,
    string  SignGlyph,
    string  DeclinationDms);
