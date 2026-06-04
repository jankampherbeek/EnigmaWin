// AgePointOverviewRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

/// <summary>Display row for a single entry in the AgePoint overview table.</summary>
public sealed class AgePointOverviewRow
{
    public string Label         { get; }
    public string LongitudeText { get; }
    public string SignGlyph     { get; }
    public bool   IsOddRow      { get; }

    public AgePointOverviewRow(string label, double longitude, int index)
    {
        Label    = label;
        IsOddRow = index % 2 != 0;

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
