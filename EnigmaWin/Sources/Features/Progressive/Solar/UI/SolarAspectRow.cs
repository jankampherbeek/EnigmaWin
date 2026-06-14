// SolarAspectRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.Solar.UI;

/// <summary>Display row for a solar-return → radix aspect.</summary>
public sealed class SolarAspectRow
{
    public string SolarGlyph  { get; }
    public string AspectGlyph { get; }
    public string RadixGlyph  { get; }
    public string OrbText     { get; }
    public bool   IsOddRow    { get; }

    public SolarAspectRow(string solarGlyph, string aspectGlyph, string radixGlyph, string orbText, int index)
    {
        SolarGlyph  = solarGlyph;
        AspectGlyph = aspectGlyph;
        RadixGlyph  = radixGlyph;
        OrbText     = orbText;
        IsOddRow    = index % 2 != 0;
    }
}
