// SignGlyphs.cs
// EnigmaWin

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.Glyphs;

/// <summary>
/// Default Unicode glyphs for zodiac signs.
/// Use GlyphSelector to retrieve glyphs; it falls back to these defaults
/// when no override is defined in the configuration.
/// </summary>
public static class SignGlyphs
{
    private static IReadOnlyDictionary<Signs, string> Glyphs { get; } = new Dictionary<Signs, string>
    {
        [Signs.Aries]       = "\uE000",
        [Signs.Taurus]      = "\uE001",
        [Signs.Gemini]      = "\uE002",
        [Signs.Cancer]      = "\uE003",
        [Signs.Leo]         = "\uE004",
        [Signs.Virgo]       = "\uE005",
        [Signs.Libra]       = "\uE006",
        [Signs.Scorpio]     = "\uE007",
        [Signs.Sagittarius] = "\uE008",
        [Signs.Capricorn]   = "\uE009",
        [Signs.Aquarius]    = "\uE010",
        [Signs.Pisces]      = "\uE011",
    };

    /// <summary>Returns the glyph for a sign, or an empty string if none is defined.</summary>
    public static string Glyph(Signs sign) =>
        Glyphs.TryGetValue(sign, out var g) ? g : string.Empty;
}
