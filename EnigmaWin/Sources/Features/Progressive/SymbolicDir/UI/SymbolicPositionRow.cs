// SymbolicPositionRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.SymbolicDir.UI;

/// <summary>Display row for a single symbolic direction position (longitude only — no declination).</summary>
public sealed class SymbolicPositionRow
{
    public string FactorGlyph   { get; }
    public string LongitudeText { get; }
    public string SignGlyph     { get; }
    public bool   IsOddRow      { get; }

    public SymbolicPositionRow(Factors factor, double longitude, int index)
    {
        FactorGlyph = GlyphSelector.GetGlyphForFactor(factor);
        IsOddRow    = index % 2 != 0;

        var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
        if (ok && sign.HasValue)
        {
            LongitudeText = dms;
            SignGlyph     = GlyphSelector.GetGlyphForSign(sign.Value);
        }
        else
        {
            LongitudeText = $"{longitude:F4}°";
            SignGlyph     = string.Empty;
        }
    }
}
