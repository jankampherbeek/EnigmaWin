// GlyphSelector.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Shared.Glyphs;

/// <summary>Selects glyphs for astrological factors, aspects and signs.</summary>
public static class GlyphSelector
{
    private static Dictionary<Factors, string> _factorOverrides = [];
    private static Dictionary<Signs,   string> _signOverrides   = [];
    private static Dictionary<Aspects, string> _aspectOverrides = [];

    /// <summary>
    /// Applies user-configured glyph choices. Call this whenever the active config changes.
    /// Only entries that differ from the FactorGlyphs/SignGlyphs/AspectGlyphs defaults are stored.
    /// </summary>
    public static void Configure(GlyphsConfig glyphs)
    {
        _factorOverrides = [];
        foreach (var s in glyphs.FactorGlyphs)
            if (!string.IsNullOrEmpty(s.Glyph) && s.Glyph != FactorGlyphs.Glyph(s.Factor))
                _factorOverrides[s.Factor] = s.Glyph;

        _signOverrides = [];
        foreach (var s in glyphs.SignGlyphs)
            if (!string.IsNullOrEmpty(s.Glyph) && s.Glyph != SignGlyphs.Glyph(s.Sign))
                _signOverrides[s.Sign] = s.Glyph;

        _aspectOverrides = [];
        foreach (var s in glyphs.AspectGlyphs)
            if (!string.IsNullOrEmpty(s.Glyph) && s.Glyph != AspectGlyphs.Glyph(s.Aspect))
                _aspectOverrides[s.Aspect] = s.Glyph;
    }

    /// <summary>Returns the glyph for a factor, preferring the user-configured choice.</summary>
    public static string GetGlyphForFactor(Factors factor) =>
        _factorOverrides.TryGetValue(factor, out var g) ? g : FactorGlyphs.Glyph(factor);

    /// <summary>Returns the glyph for a sign, preferring the user-configured choice.</summary>
    public static string GetGlyphForSign(Signs sign) =>
        _signOverrides.TryGetValue(sign, out var g) ? g : SignGlyphs.Glyph(sign);

    /// <summary>Returns the glyph for an aspect, preferring the user-configured choice.</summary>
    public static string GetGlyphForAspect(Aspects aspect) =>
        _aspectOverrides.TryGetValue(aspect, out var g) ? g : AspectGlyphs.Glyph(aspect);
}
