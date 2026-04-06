// DeclStripItem.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

/// <summary>Single entry displayed in the declination strip diagram and the declination table.</summary>
public record DeclStripItem(
    Factors Factor,
    string  Glyph,
    double  Declination,   // positive = north, negative = south
    double  Longitude);    // ecliptic longitude 0–360
