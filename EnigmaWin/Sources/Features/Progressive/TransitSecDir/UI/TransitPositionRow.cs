// TransitPositionRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;

/// <summary>Display row for a single transit position.</summary>
public sealed class TransitPositionRow
{
    public string FactorGlyph   { get; }
    public string LongitudeText { get; }
    public string SignGlyph     { get; }
    public string DeclinationText { get; }
    public bool   IsOddRow      { get; }

    public TransitPositionRow(Factors factor, ProgressivePosition position, int index)
    {
        FactorGlyph = GlyphSelector.GetGlyphForFactor(factor);
        IsOddRow    = index % 2 != 0;

        var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(position.Longitude);
        if (ok && sign.HasValue)
        {
            LongitudeText = dms;
            SignGlyph     = GlyphSelector.GetGlyphForSign(sign.Value);
        }
        else
        {
            LongitudeText = $"{position.Longitude:F4}°";
            SignGlyph     = string.Empty;
        }

        DeclinationText = PositionInDegreesConversion.DoubleToDms(position.Declination);
    }
}
