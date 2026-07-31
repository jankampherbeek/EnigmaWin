// SynastryDeclinationRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Display row for a single cross-chart parallel or contra-parallel, shown from one person's perspective.</summary>
public sealed class SynastryDeclinationRow
{
    public string RadixGlyph   { get; }
    public string MatchGlyph   { get; }
    public string PartnerGlyph { get; }
    public string OrbText      { get; }
    public bool   IsOddRow     { get; }

    public SynastryDeclinationRow(string radixGlyph, string matchGlyph, string partnerGlyph, string orbText, int index)
    {
        RadixGlyph   = radixGlyph;
        MatchGlyph   = matchGlyph;
        PartnerGlyph = partnerGlyph;
        OrbText      = orbText;
        IsOddRow     = index % 2 != 0;
    }
}
