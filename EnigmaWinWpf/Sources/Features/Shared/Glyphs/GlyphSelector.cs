// GlyphSelector.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.Glyphs;

/// <summary>Selects glyphs for astrological factors, aspects and signs.</summary>
public static class GlyphSelector
{
    /// <summary>Returns the glyph for an aspect.</summary>
    public static string GetGlyphForAspect(Aspects aspect) =>
        AspectGlyphs.Glyph(aspect);

    /// <summary>Returns the glyph for a factor.</summary>
    public static string GetGlyphForFactor(Factors factor) =>
        FactorGlyphs.Glyph(factor);

    /// <summary>Returns the glyph for a sign.</summary>
    public static string GetGlyphForSign(Signs sign) =>
        SignGlyphs.Glyph(sign);
}
