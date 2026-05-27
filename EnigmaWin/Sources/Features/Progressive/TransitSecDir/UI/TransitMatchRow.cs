// TransitMatchRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;

/// <summary>Display row for a single aspect or parallel match between a transit and radix factor.</summary>
public sealed class TransitMatchRow
{
    public string TransitGlyph { get; }
    public string MatchGlyph   { get; }
    public string RadixGlyph   { get; }
    public string OrbText      { get; }
    public string ExactnessText { get; }
    public bool   IsOddRow     { get; }

    public TransitMatchRow(
        string transitGlyph,
        string matchGlyph,
        string radixGlyph,
        string orbText,
        int exactness,
        int index)
    {
        TransitGlyph  = transitGlyph;
        MatchGlyph    = matchGlyph;
        RadixGlyph    = radixGlyph;
        OrbText       = orbText;
        ExactnessText = $"{exactness}%";
        IsOddRow      = index % 2 != 0;
    }
}
