// FactorGlyphs.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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
        [Factors.Sun]                   = "",
        [Factors.Moon]                  = "",
        [Factors.Mercury]               = "",
        [Factors.Venus]                 = "",
        [Factors.Earth]                 = "",
        [Factors.Mars]                  = "",
        [Factors.Jupiter]               = "",
        [Factors.Saturn]                = "",
        [Factors.Uranus]                = "",
        [Factors.Neptune]               = "",
        [Factors.Pluto]                 = "",
        [Factors.NorthNodeMean]         = "",
        [Factors.NorthNodeTrue]         = "",
        [Factors.Chiron]                = "",
        [Factors.PersephoneRam]         = "",
        [Factors.HermesRam]             = "",
        [Factors.DemeterRam]            = "",
        [Factors.CupidoUra]             = "",
        [Factors.HadesUra]              = "",
        [Factors.ZeusUra]               = "",
        [Factors.KronosUra]             = "",
        [Factors.ApollonUra]            = "",
        [Factors.AdmetosUra]            = "",
        [Factors.VulcanusUra]           = "",
        [Factors.PoseidonUra]           = "",
        [Factors.Eris]                  = "",
        [Factors.Pholus]                = "",
        [Factors.Ceres]                 = "",
        [Factors.Pallas]                = "",
        [Factors.Juno]                  = "",
        [Factors.Vesta]                 = "",
        [Factors.Isis]                  = "",
        [Factors.Nessus]                = "",
        [Factors.Huya]                  = "",
        [Factors.Varuna]                = "",
        [Factors.Ixion]                 = "",
        [Factors.Quaoar]                = "",
        [Factors.Haumea]                = "",
        [Factors.Orcus]                 = "",
        [Factors.Makemake]              = "",
        [Factors.Sedna]                 = "",
        [Factors.Hygieia]               = "",
        [Factors.Astraea]               = "",
        [Factors.ApogeeMean]            = "",
        [Factors.ApogeeKoch]            = "",
        [Factors.ApogeeDuval]           = "",
        [Factors.ApogeeInterpolated]    = "",
        [Factors.PersephoneCarteret]    = "",
        [Factors.VulcanusCarteret]      = "",
        [Factors.PerigeeInterpolated]   = "☉",   // TODO: add dedicated glyph for perigee
        [Factors.Priapus]               = "",
        [Factors.PriapusKoch]           = "",
        [Factors.PriapusDuval]          = "",
        [Factors.PriapusInterpolated]   = "",
        [Factors.Dragon]                = "",
        [Factors.Beast]                 = "",
        [Factors.SouthNodeMean]         = "",
        [Factors.SouthNodeTrue]         = "",
        [Factors.BlackSun]              = "",
        [Factors.Diamond]               = "",
        [Factors.Ascendant]             = "",
        [Factors.Mc]                    = "",
        [Factors.EastPoint]             = "",
        [Factors.Vertex]                = "",
        [Factors.ZeroAries]             = "",
        [Factors.FortunaSect]           = "",
        [Factors.FortunaNoSect]         = "",
        [Factors.LogTimeScale]          = "*",
    };

    /// <summary>Returns the glyph for a factor, or an empty string if none is defined.</summary>
    public static string Glyph(Factors factor) =>
        Glyphs.TryGetValue(factor, out var g) ? g : string.Empty;
}