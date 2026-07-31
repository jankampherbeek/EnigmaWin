// SynastryMidpointRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Synastry.UI;

public sealed record SynastryMidpointRow(
    string Glyph1,
    string Glyph2,
    string MidpointDms,
    string MidpointSignGlyph,
    string PartnerGlyph,
    string OrbText,
    string ExactnessText,
    bool   IsEvenRow);
