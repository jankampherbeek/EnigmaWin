// GlyphSelectorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWintest.Features.Glyphs;

[TestFixture]
public class GlyphSelectorTests
{
    // MARK: - Factor glyph tests

    [Test]
    public void TestGetGlyphForFactorAllCasesNonEmpty()
    {
        foreach (var factor in Enum.GetValues<Factors>())
        {
            var glyph = GlyphSelector.GetGlyphForFactor(factor);
            if (factor != Factors.AgePoint)
            {
                Assert.That(glyph, Is.Not.Empty, $"Expected non-empty glyph for factor {factor}");
            }
        }
    }

    [Test]
    public void TestGetGlyphForFactorMajorPlanets()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Sun),     Is.EqualTo("\uE200"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Moon),    Is.EqualTo("\uE201"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Mercury), Is.EqualTo("\uE202"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Venus),   Is.EqualTo("\uE203"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Mars),    Is.EqualTo("\uE205"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Jupiter), Is.EqualTo("\uE206"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Saturn),  Is.EqualTo("\uE207"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Uranus),  Is.EqualTo("\uE208"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Neptune), Is.EqualTo("\uE209"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Pluto),   Is.EqualTo("\uE210"));
        }
    }

    [Test]
    public void TestGetGlyphForFactorNodes()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.NorthNodeMean), Is.EqualTo("\uE520"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.NorthNodeTrue), Is.EqualTo("\uE520"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.SouthNodeMean), Is.EqualTo("\uE521"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.SouthNodeTrue), Is.EqualTo("\uE521"));
        }
    }

    [Test]
    public void TestGetGlyphForFactorMundane()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Ascendant), Is.EqualTo("\uE500"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Mc),        Is.EqualTo("\uE501"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.Vertex),    Is.EqualTo("\uE502"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.EastPoint), Is.EqualTo("\uE503"));
        }
    }

    [Test]
    public void TestGetGlyphForFactorUranian()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.CupidoUra),   Is.EqualTo("\uE600"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.HadesUra),    Is.EqualTo("\uE601"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.ZeusUra),     Is.EqualTo("\uE602"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.KronosUra),   Is.EqualTo("\uE603"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.ApollonUra),  Is.EqualTo("\uE604"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.AdmetosUra),  Is.EqualTo("\uE605"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.VulcanusUra), Is.EqualTo("\uE606"));
            Assert.That(GlyphSelector.GetGlyphForFactor(Factors.PoseidonUra), Is.EqualTo("\uE607"));
        }
    }

    // MARK: - Sign glyph tests

    [Test]
    public void TestGetGlyphForSignAllCasesNonEmpty()
    {
        foreach (var sign in Enum.GetValues<Signs>())
        {
            var glyph = GlyphSelector.GetGlyphForSign(sign);
            Assert.That(glyph, Is.Not.Empty, $"Expected non-empty glyph for sign {sign}");
        }
    }

    [Test]
    public void TestGetGlyphForSignAllValues()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Aries),       Is.EqualTo("\uE000"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Taurus),      Is.EqualTo("\uE001"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Gemini),      Is.EqualTo("\uE002"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Cancer),      Is.EqualTo("\uE003"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Leo),         Is.EqualTo("\uE004"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Virgo),       Is.EqualTo("\uE005"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Libra),       Is.EqualTo("\uE006"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Scorpio),     Is.EqualTo("\uE007"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Sagittarius), Is.EqualTo("\uE008"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Capricorn),   Is.EqualTo("\uE009"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Aquarius),    Is.EqualTo("\uE010"));
            Assert.That(GlyphSelector.GetGlyphForSign(Signs.Pisces),      Is.EqualTo("\uE011"));
        }
    }

    // MARK: - Aspect glyph tests

    [Test]
    public void TestGetGlyphForAspectAllCasesNonEmpty()
    {
        foreach (var aspect in Enum.GetValues<Aspects>())
        {
            var glyph = GlyphSelector.GetGlyphForAspect(aspect);
            Assert.That(glyph, Is.Not.Empty, $"Expected non-empty glyph for aspect {aspect}");
        }
    }

    [Test]
    public void TestGetGlyphForAspectClassical()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Conjunction), Is.EqualTo("\uE700"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Opposition),  Is.EqualTo("\uE710"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Trine),       Is.EqualTo("\uE720"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Square),      Is.EqualTo("\uE730"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Sextile),     Is.EqualTo("\uE740"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Inconjunct),  Is.EqualTo("\uE760"));
        }
    }

    [Test]
    public void TestGetGlyphForAspectMinor()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Semisextile),    Is.EqualTo("\uE750"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Semisquare),     Is.EqualTo("\uE770"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Sesquiquadrate), Is.EqualTo("\uE780"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Quintile),       Is.EqualTo("\uE790"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Biquintile),     Is.EqualTo("\uE800"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Septile),        Is.EqualTo("\uE810"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Semiquintile),   Is.EqualTo("\uE830"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Tridecile),      Is.EqualTo("\uE840"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Biseptile),      Is.EqualTo("\uE850"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Triseptile),     Is.EqualTo("\uE860"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Novile),         Is.EqualTo("\uE870"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Binovile),       Is.EqualTo("\uE880"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Quadranovile),   Is.EqualTo("\uE890"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Vigintile),      Is.EqualTo("\uE820"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Undecile),       Is.EqualTo("\uE900"));
            Assert.That(GlyphSelector.GetGlyphForAspect(Aspects.Centile),        Is.EqualTo("\uE910"));
        }
    }
}
