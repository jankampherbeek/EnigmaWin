// DeclDiagramItem.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public record DeclDiagramItem(
    Factors Factor,
    string  Glyph,
    double  Longitude,
    double  Declination,
    string  LongitudeDms,
    string  SignGlyph,
    string  DeclinationDms);
