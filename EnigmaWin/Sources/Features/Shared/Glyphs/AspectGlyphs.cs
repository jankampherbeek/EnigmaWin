// AspectGlyphs.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.Glyphs;

/// <summary>
/// Default Unicode glyphs for astrological aspects.
/// Use GlyphSelector to retrieve glyphs; it falls back to these defaults
/// when no override is defined in the configuration.
/// </summary>
public static class AspectGlyphs
{
    private static IReadOnlyDictionary<Aspects, string> Glyphs { get; } = new Dictionary<Aspects, string>
    {
        [Aspects.Conjunction]    = "\uE700",
        [Aspects.Opposition]     = "\uE710",
        [Aspects.Trine]          = "\uE720",
        [Aspects.Square]         = "\uE730",
        [Aspects.Septile]        = "\uE810",
        [Aspects.Sextile]        = "\uE740",
        [Aspects.Quintile]       = "\uE790",
        [Aspects.Semisextile]    = "\uE750",
        [Aspects.Semisquare]     = "\uE770",
        [Aspects.Semiquintile]   = "\uE830",
        [Aspects.Biquintile]     = "\uE800",
        [Aspects.Inconjunct]     = "\uE760",
        [Aspects.Sesquiquadrate] = "\uE780",
        [Aspects.Tridecile]      = "\uE840",
        [Aspects.Biseptile]      = "\uE850",
        [Aspects.Triseptile]     = "\uE860",
        [Aspects.Novile]         = "\uE870",
        [Aspects.Binovile]       = "\uE880",
        [Aspects.Quadranovile]   = "\uE890",
        [Aspects.Undecile]       = "\uE900",
        [Aspects.Centile]        = "\uE910",
        [Aspects.Vigintile]      = "\uE820",
    };

    /// <summary>Returns the glyph for an aspect, or an empty string if none is defined.</summary>
    public static string Glyph(Aspects aspect) =>
        Glyphs.TryGetValue(aspect, out var g) ? g : string.Empty;
}
