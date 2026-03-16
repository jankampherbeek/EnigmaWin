// FactorGlyphs.cs
// EnigmaWin

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.Glyphs;

/// <summary>
/// Default Unicode glyphs for astrological factors.
/// Use GlyphSelector to retrieve glyphs; it falls back to these defaults
/// when no override is defined in the configuration.
/// </summary>
public static class FactorGlyphs
{
    private static IReadOnlyDictionary<Factors, string> Glyphs { get; } = new Dictionary<Factors, string>
    {
        [Factors.Sun]                   = "\uE200",
        [Factors.Moon]                  = "\uE201",
        [Factors.Mercury]               = "\uE202",
        [Factors.Venus]                 = "\uE203",
        [Factors.Earth]                 = "\uE204",
        [Factors.Mars]                  = "\uE205",
        [Factors.Jupiter]               = "\uE206",
        [Factors.Saturn]                = "\uE207",
        [Factors.Uranus]                = "\uE208",
        [Factors.Neptune]               = "\uE209",
        [Factors.Pluto]                 = "\uE210",
        [Factors.NorthNodeMean]         = "\uE520",
        [Factors.NorthNodeTrue]         = "\uE520",
        [Factors.Chiron]                = "\uE400",
        [Factors.PersephoneRam]         = "\uE608",
        [Factors.HermesRam]             = "\uE609",
        [Factors.DemeterRam]            = "\uE610",
        [Factors.CupidoUra]             = "\uE600",
        [Factors.HadesUra]              = "\uE601",
        [Factors.ZeusUra]               = "\uE602",
        [Factors.KronosUra]             = "\uE603",
        [Factors.ApollonUra]            = "\uE604",
        [Factors.AdmetosUra]            = "\uE605",
        [Factors.VulcanusUra]           = "\uE606",
        [Factors.PoseidonUra]           = "\uE607",
        [Factors.Eris]                  = "\uE407",
        [Factors.Pholus]                = "\uE402",
        [Factors.Ceres]                 = "\uE411",
        [Factors.Pallas]                = "\uE412",
        [Factors.Juno]                  = "\uE413",
        [Factors.Vesta]                 = "\uE414",
        [Factors.Isis]                  = "\uE611",
        [Factors.Nessus]                = "\uE401",
        [Factors.Huya]                  = "\uE417",
        [Factors.Varuna]                = "\uE403",
        [Factors.Ixion]                 = "\uE404",
        [Factors.Quaoar]                = "\uE405",
        [Factors.Haumea]                = "\uE406",
        [Factors.Orcus]                 = "\uE409",
        [Factors.Makemake]              = "\uE410",
        [Factors.Sedna]                 = "\uE408",
        [Factors.Hygieia]               = "\uE415",
        [Factors.Astraea]               = "\uE416",
        [Factors.ApogeeMean]            = "\uE530",
        [Factors.ApogeeCorrected]       = "\uE531",
        [Factors.ApogeeInterpolated]    = "\uE531",
        [Factors.PersephoneCarteret]    = "\uE612",
        [Factors.VulcanusCarteret]      = "\uE613",
        [Factors.PerigeeInterpolated]   = "\u2609",   // TODO: add dedicated glyph for perigee
        [Factors.Priapus]               = "\uE535",
        [Factors.PriapusCorrected]      = "\u2609",   // TODO: add dedicated glyph for Priapus corrected
        [Factors.Dragon]                = "\u2609",   // TODO: add dedicated glyph for Dragon
        [Factors.Beast]                 = "\u2609",   // TODO: add dedicated glyph for Beast
        [Factors.SouthNodeMean]         = "\uE521",
        [Factors.SouthNodeTrue]         = "\uE521",
        [Factors.BlackSun]              = "\uE534",
        [Factors.Diamond]               = "\uE536",
        [Factors.Ascendant]             = "\uE500",
        [Factors.Mc]                    = "\uE501",
        [Factors.EastPoint]             = "\uE503",
        [Factors.Vertex]                = "\uE502",
        [Factors.ZeroAries]             = "\uE000",
        [Factors.FortunaSect]           = "\uF400",
        [Factors.FortunaNoSect]         = "\uF400",
    };

    /// <summary>Returns the glyph for a factor, or an empty string if none is defined.</summary>
    public static string Glyph(Factors factor) =>
        Glyphs.TryGetValue(factor, out var g) ? g : string.Empty;
}
