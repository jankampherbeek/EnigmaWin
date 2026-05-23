// GlyphsConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>Glyph selection for a single zodiac sign.</summary>
public readonly record struct SignGlyphSetting(Signs Sign, string Glyph);

/// <summary>Glyph selection for a single celestial factor.</summary>
public readonly record struct FactorGlyphSetting(Factors Factor, string Glyph);

/// <summary>Glyph selection for a single aspect.</summary>
public readonly record struct AspectGlyphSetting(Aspects Aspect, string Glyph);

/// <summary>Configuration for glyph selections for all signs, factors and aspects.</summary>
public readonly record struct GlyphsConfig(
    IReadOnlyList<SignGlyphSetting> SignGlyphs,
    IReadOnlyList<FactorGlyphSetting> FactorGlyphs,
    IReadOnlyList<AspectGlyphSetting> AspectGlyphs)
{
    public static GlyphsConfig Default => new(DefaultSignGlyphs, DefaultFactorGlyphs, DefaultAspectGlyphs);

    public static IReadOnlyList<SignGlyphSetting> DefaultSignGlyphs =>
        Enum.GetValues<Signs>().Select(s => new SignGlyphSetting(s, s switch
        {
            Signs.Aries       => "\uE000",
            Signs.Taurus      => "\uE001",
            Signs.Gemini      => "\uE002",
            Signs.Cancer      => "\uE003",
            Signs.Leo         => "\uE004",
            Signs.Virgo       => "\uE005",
            Signs.Libra       => "\uE006",
            Signs.Scorpio     => "\uE007",
            Signs.Sagittarius => "\uE008",
            Signs.Capricorn   => "\uE009",
            Signs.Aquarius    => "\uE010",
            Signs.Pisces      => "\uE011",
            _                 => "?"
        })).ToList();

    public static IReadOnlyList<FactorGlyphSetting> DefaultFactorGlyphs =>
        Enum.GetValues<Factors>().Select(f => new FactorGlyphSetting(f, f switch
        {
            Factors.Sun                 => "\uE200",
            Factors.Moon                => "\uE201",
            Factors.Mercury             => "\uE202",
            Factors.Venus               => "\uE203",
            Factors.Earth               => "\uE204",
            Factors.Mars                => "\uE205",
            Factors.Jupiter             => "\uE206",
            Factors.Saturn              => "\uE207",
            Factors.Uranus              => "\uE208",
            Factors.Neptune             => "\uE209",
            Factors.Pluto               => "\uE210",
            Factors.NorthNodeMean       => "\uE520",
            Factors.NorthNodeTrue       => "\uE520",
            Factors.Chiron              => "\uE400",
            Factors.PersephoneRam       => "\uE608",
            Factors.HermesRam           => "\uE609",
            Factors.DemeterRam          => "\uE610",
            Factors.CupidoUra           => "\uE600",
            Factors.HadesUra            => "\uE601",
            Factors.ZeusUra             => "\uE602",
            Factors.KronosUra           => "\uE603",
            Factors.ApollonUra          => "\uE604",
            Factors.AdmetosUra          => "\uE605",
            Factors.VulcanusUra         => "\uE606",
            Factors.PoseidonUra         => "\uE607",
            Factors.Eris                => "\uE407",
            Factors.Pholus              => "\uE402",
            Factors.Ceres               => "\uE411",
            Factors.Pallas              => "\uE412",
            Factors.Juno                => "\uE413",
            Factors.Vesta               => "\uE414",
            Factors.Isis                => "\uE611",
            Factors.Nessus              => "\uE401",
            Factors.Huya                => "\uE417",
            Factors.Varuna              => "\uE403",
            Factors.Ixion               => "\uE404",
            Factors.Quaoar              => "\uE405",
            Factors.Haumea              => "\uE406",
            Factors.Orcus               => "\uE409",
            Factors.Makemake            => "\uE410",
            Factors.Sedna               => "\uE408",
            Factors.Hygieia             => "\uE415",
            Factors.Astraea             => "\uE416",
            Factors.ApogeeMean          => "\uE530",
            Factors.ApogeeCorrected     => "\uE531",
            Factors.ApogeeInterpolated  => "\uE531",
            Factors.PersephoneCarteret  => "\uE612",
            Factors.VulcanusCarteret    => "\uE613",
            Factors.PerigeeInterpolated => "\u2609",   // TODO: add proper glyph
            Factors.Priapus             => "\uE535",
            Factors.PriapusCorrected    => "\u2609",   // TODO: add proper glyph
            Factors.Dragon              => "\u2609",   // TODO: add proper glyph
            Factors.Beast               => "\u2609",   // TODO: add proper glyph
            Factors.SouthNodeMean       => "\uE521",
            Factors.SouthNodeTrue       => "\uE521",
            Factors.BlackSun            => "\uE534",
            Factors.Diamond             => "\uE536",
            Factors.Ascendant           => "\uE500",
            Factors.Mc                  => "\uE501",
            Factors.EastPoint           => "\uE503",
            Factors.Vertex              => "\uE502",
            Factors.ZeroAries           => "\uE000",
            Factors.FortunaSect         => "\uF400",
            Factors.FortunaNoSect       => "\uF400",
            _                           => "?"
        })).ToList();

    public static IReadOnlyList<AspectGlyphSetting> DefaultAspectGlyphs =>
        Enum.GetValues<Aspects>().Select(a => new AspectGlyphSetting(a, a switch
        {
            Aspects.Conjunction    => "\uE700",
            Aspects.Opposition     => "\uE710",
            Aspects.Trine          => "\uE720",
            Aspects.Square         => "\uE730",
            Aspects.Septile        => "\uE810",
            Aspects.Sextile        => "\uE740",
            Aspects.Quintile       => "\uE790",
            Aspects.Semisextile    => "\uE750",
            Aspects.Semisquare     => "\uE770",
            Aspects.Semiquintile   => "\uE830",
            Aspects.Biquintile     => "\uE800",
            Aspects.Inconjunct     => "\uE760",
            Aspects.Sesquiquadrate => "\uE780",
            Aspects.Tridecile      => "\uE840",
            Aspects.Biseptile      => "\uE850",
            Aspects.Triseptile     => "\uE860",
            Aspects.Novile         => "\uE870",
            Aspects.Binovile       => "\uE880",
            Aspects.Quadranovile   => "\uE890",
            Aspects.Undecile       => "\uE900",
            Aspects.Centile        => "\uE910",
            Aspects.Vigintile      => "\uE820",
            _                      => "?"
        })).ToList();
}
