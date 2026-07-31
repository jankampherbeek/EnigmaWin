// SynastryAspectRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Display row for a single cross-chart aspect, shown from one person's perspective.</summary>
public sealed class SynastryAspectRow
{
    public string RadixGlyph   { get; }
    public string AspectGlyph  { get; }
    public string PartnerGlyph { get; }
    public string OrbText      { get; }
    public bool   IsOddRow     { get; }

    public SynastryAspectRow(string radixGlyph, string aspectGlyph, string partnerGlyph, string orbText, int index)
    {
        RadixGlyph   = radixGlyph;
        AspectGlyph  = aspectGlyph;
        PartnerGlyph = partnerGlyph;
        OrbText      = orbText;
        IsOddRow     = index % 2 != 0;
    }
}
