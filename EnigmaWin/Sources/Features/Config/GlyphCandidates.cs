// GlyphCandidates.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>
/// Static lookup of available glyph candidates per sign, factor and aspect.
/// Items with one candidate have no alternative. Items with multiple candidates
/// allow the user to choose in the configuration UI.
/// </summary>
public static class GlyphCandidates
{
    public static IReadOnlyList<string> ForSign(Signs sign) => sign switch
    {
        Signs.Aries       => ["\uE000"],
        Signs.Taurus      => ["\uE001"],
        Signs.Gemini      => ["\uE002"],
        Signs.Cancer      => ["\uE003"],
        Signs.Leo         => ["\uE004"],
        Signs.Virgo       => ["\uE005"],
        Signs.Libra       => ["\uE006"],
        Signs.Scorpio     => ["\uE007"],
        Signs.Sagittarius => ["\uE008"],
        Signs.Capricorn   => ["\uE009", "\uE012"],
        Signs.Aquarius    => ["\uE010"],
        Signs.Pisces      => ["\uE011"],
        _                 => ["?"]
    };

    public static IReadOnlyList<string> ForFactor(Factors factor) => factor switch
    {
        Factors.Sun                 => ["\uE200", "\uE300"],
        Factors.Moon                => ["\uE201"],
        Factors.Mercury             => ["\uE202", "\uE301"],
        Factors.Venus               => ["\uE203"],
        Factors.Earth               => ["\uE204"],
        Factors.Mars                => ["\uE205", "\uE302"],
        Factors.Jupiter             => ["\uE206", "\uE303"],
        Factors.Saturn              => ["\uE207", "\uE304"],
        Factors.Uranus              => ["\uE208", "\uE305", "\uE306"],
        Factors.Neptune             => ["\uE209", "\uE307"],
        Factors.Pluto               => ["\uE210", "\uE308", "\uE309", "\uE310", "\uE311", "\uE312"],
        Factors.NorthNodeMean       => ["\uE520"],
        Factors.NorthNodeTrue       => ["\uE520"],
        Factors.Chiron              => ["\uE400", "\uE450"],
        Factors.PersephoneRam       => ["\uE608"],
        Factors.HermesRam           => ["\uE609"],
        Factors.DemeterRam          => ["\uE610"],
        Factors.CupidoUra           => ["\uE600"],
        Factors.HadesUra            => ["\uE601"],
        Factors.ZeusUra             => ["\uE602"],
        Factors.KronosUra           => ["\uE603"],
        Factors.ApollonUra          => ["\uE604"],
        Factors.AdmetosUra          => ["\uE605"],
        Factors.VulcanusUra         => ["\uE606"],
        Factors.PoseidonUra         => ["\uE607"],
        Factors.Eris                => ["\uE407", "\uE451", "\uE452", "\uE453", "\uE454", "\uE455", "\uE456"],
        Factors.Pholus              => ["\uE402"],
        Factors.Ceres               => ["\uE411"],
        Factors.Pallas              => ["\uE412"],
        Factors.Juno                => ["\uE413"],
        Factors.Vesta               => ["\uE414"],
        Factors.Isis                => ["\uE611"],
        Factors.Nessus              => ["\uE401"],
        Factors.Huya                => ["\uE417"],
        Factors.Varuna              => ["\uE403"],
        Factors.Ixion               => ["\uE404"],
        Factors.Quaoar              => ["\uE405"],
        Factors.Haumea              => ["\uE406"],
        Factors.Orcus               => ["\uE409"],
        Factors.Makemake            => ["\uE410"],
        Factors.Sedna               => ["\uE408"],
        Factors.Hygieia             => ["\uE415", "\uE457"],
        Factors.Astraea             => ["\uE416"],
        Factors.ApogeeMean          => ["\uE530"],
        Factors.ApogeeKoch          => [""],
        Factors.ApogeeDuval         => [""],
        Factors.ApogeeInterpolated  => [""],
        Factors.PersephoneCarteret  => ["\uE612"],
        Factors.VulcanusCarteret    => ["\uE613"],
        Factors.Priapus             => ["\uE535"],
        Factors.PriapusKoch         => [""],
        Factors.PriapusDuval        => [""],
        Factors.PriapusInterpolated => [""],
        Factors.Dragon              => ["", ""],
        Factors.Beast               => ["", ""],
        Factors.SouthNodeMean       => ["\uE521"],
        Factors.SouthNodeTrue       => ["\uE521"],
        Factors.BlackSun            => ["\uE534"],
        Factors.Diamond             => ["\uE536"],
        Factors.Ascendant           => ["\uE500", "\uE550"],
        Factors.Mc                  => ["\uE501", "\uE551"],
        Factors.EastPoint           => ["\uE503"],
        Factors.Vertex              => ["\uE502"],
        Factors.ZeroAries           => ["\uE000"],
        Factors.FortunaSect         => ["\uF400"],
        Factors.FortunaNoSect       => ["\uF400"],
        _                           => ["?"]
    };

    public static IReadOnlyList<string> ForAspect(Aspects aspect) => aspect switch
    {
        Aspects.Conjunction    => ["\uE700"],
        Aspects.Opposition     => ["\uE710"],
        Aspects.Trine          => ["\uE720"],
        Aspects.Square         => ["\uE730"],
        Aspects.Septile        => ["\uE810"],
        Aspects.Sextile        => ["\uE740"],
        Aspects.Quintile       => ["\uE790"],
        Aspects.Semisextile    => ["\uE750"],
        Aspects.Semisquare     => ["\uE770"],
        Aspects.Semiquintile   => ["\uE830"],
        Aspects.Biquintile     => ["\uE800"],
        Aspects.Inconjunct     => ["\uE760"],
        Aspects.Sesquiquadrate => ["\uE780"],
        Aspects.Tridecile      => ["\uE840"],
        Aspects.Biseptile      => ["\uE850"],
        Aspects.Triseptile     => ["\uE860"],
        Aspects.Novile         => ["\uE870"],
        Aspects.Binovile       => ["\uE880"],
        Aspects.Quadranovile   => ["\uE890"],
        Aspects.Undecile       => ["\uE900"],
        Aspects.Centile        => ["\uE910"],
        Aspects.Vigintile      => ["\uE820"],
        _                      => ["?"]
    };
}