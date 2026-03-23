// UserConfigurationTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.Config;

[TestFixture]
public class UserConfigurationTests
{
    [Test]
    public void Default_HasExpectedName()
    {
        var config = UserConfiguration.Default;
        Assert.That(config.Name, Is.EqualTo("Default"));
    }

    [Test]
    public void Default_IsActive()
    {
        Assert.That(UserConfiguration.Default.IsActive, Is.True);
    }

    [Test]
    public void Default_HasNewId()
    {
        var a = UserConfiguration.Default;
        var b = UserConfiguration.Default;
        // Each call creates a new instance with a new Guid
        Assert.That(a.Id, Is.Not.EqualTo(b.Id));
    }

    [Test]
    public void Default_CalculationConfig_MatchesCalculationConfigDefault()
    {
        var c = UserConfiguration.Default.CalculationConfig;
        Assert.That(c, Is.EqualTo(CalculationConfig.Default));
    }

    [Test]
    public void Default_FactorConfig_HasSettings()
    {
        var settings = UserConfiguration.Default.FactorConfig.Settings;
        Assert.That(settings, Is.Not.Empty);
    }

    [Test]
    public void Default_AspectConfig_HasSettings()
    {
        var settings = UserConfiguration.Default.AspectConfig.Settings;
        Assert.That(settings, Is.Not.Empty);
    }

    [Test]
    public void Default_GlyphsConfig_HasGlyphs()
    {
        var glyphs = UserConfiguration.Default.GlyphsConfig;
        Assert.That(glyphs.SignGlyphs,   Is.Not.Empty);
        Assert.That(glyphs.FactorGlyphs, Is.Not.Empty);
        Assert.That(glyphs.AspectGlyphs, Is.Not.Empty);
    }

    [Test]
    public void Default_OrbConfig_HasPositiveBaseOrb()
    {
        Assert.That(UserConfiguration.Default.OrbConfig.AspectBaseOrb, Is.GreaterThan(0.0));
    }

    [Test]
    public void Default_ProgressionsConfig_HasSubConfigs()
    {
        var p = UserConfiguration.Default.ProgressionsConfig;
        Assert.That(p.Transits.Factors,                   Is.Not.Empty);
        Assert.That(p.PrimaryDirections.Promissors,        Is.Not.Empty);
        Assert.That(p.SecondaryDirections.Factors,         Is.Not.Empty);
        Assert.That(p.SymbolicDirections.Factors,          Is.Not.Empty);
    }
}

[TestFixture]
public class CalculationConfigTests
{
    [Test]
    public void Default_HasExpectedValues()
    {
        var c = CalculationConfig.Default;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(c.HouseSystem,            Is.EqualTo(HouseSystems.Placidus));
            Assert.That(c.Ayanamsha,              Is.EqualTo(Ayanamshas.Tropical));
            Assert.That(c.ObserverPosition,       Is.EqualTo(ObserverPositions.Geocentric));
            Assert.That(c.ProjectionType,          Is.EqualTo(ProjectionTypes.TwoDimensional));
            Assert.That(c.BlackMoonCorrectionType, Is.EqualTo(BlackMoonCorrectionTypes.Duval));
            Assert.That(c.LunarNodeType,           Is.EqualTo(LunarNodeTypes.MeanNode));
            Assert.That(c.LotsType,                Is.EqualTo(LotsTypes.Sect));
            Assert.That(c.StationaryPercentage,    Is.EqualTo(10));
            Assert.That(c.SlowPercentage,          Is.EqualTo(20));
        }
    }

    [Test]
    public void WithExpression_CreatesNewInstance()
    {
        var original = CalculationConfig.Default;
        var modified = original with { HouseSystem = HouseSystems.Koch };
        Assert.That(modified.HouseSystem, Is.EqualTo(HouseSystems.Koch));
        Assert.That(original.HouseSystem, Is.EqualTo(HouseSystems.Placidus));
    }
}

[TestFixture]
public class FactorConfigTests
{
    [Test]
    public void Default_SunHasOrbPercentage100()
    {
        var settings = FactorConfig.Default.Settings;
        var sun = settings.First(s => s.Factor == Factors.Sun);
        Assert.That(sun.OrbPercentage, Is.EqualTo(100));
        Assert.That(sun.IsUsed,        Is.True);
    }

    [Test]
    public void Default_MercuryHasOrbPercentage85()
    {
        var mercury = FactorConfig.Default.Settings.First(s => s.Factor == Factors.Mercury);
        Assert.That(mercury.OrbPercentage, Is.EqualTo(85));
    }

    [Test]
    public void Default_SaturnHasOrbPercentage75()
    {
        var saturn = FactorConfig.Default.Settings.First(s => s.Factor == Factors.Saturn);
        Assert.That(saturn.OrbPercentage, Is.EqualTo(75));
    }

    [Test]
    public void Default_PersephoneRamIsInactive()
    {
        var p = FactorConfig.Default.Settings.First(s => s.Factor == Factors.PersephoneRam);
        Assert.That(p.IsUsed,  Is.False);
        Assert.That(p.IsDrawn, Is.False);
    }

    [Test]
    public void Default_ContainsAllFactors()
    {
        var count    = FactorConfig.Default.Settings.Count;
        var allCount = Enum.GetValues<Factors>().Length;
        Assert.That(count, Is.EqualTo(allCount));
    }
}

[TestFixture]
public class AspectConfigTests
{
    [Test]
    public void Default_ConjunctionHasOrbPercentage100()
    {
        var conj = AspectConfig.Default.Settings.First(s => s.Aspect == Aspects.Conjunction);
        Assert.That(conj.OrbPercentage, Is.EqualTo(100));
        Assert.That(conj.IsUsed,        Is.True);
    }

    [Test]
    public void Default_SextileHasOrbPercentage60()
    {
        var sextile = AspectConfig.Default.Settings.First(s => s.Aspect == Aspects.Sextile);
        Assert.That(sextile.OrbPercentage, Is.EqualTo(60));
    }

    [Test]
    public void Default_SeptileIsInactive()
    {
        var septile = AspectConfig.Default.Settings.First(s => s.Aspect == Aspects.Septile);
        Assert.That(septile.IsUsed, Is.False);
    }

    [Test]
    public void Default_ContainsAllAspects()
    {
        var count    = AspectConfig.Default.Settings.Count;
        var allCount = Enum.GetValues<Aspects>().Length;
        Assert.That(count, Is.EqualTo(allCount));
    }

    [Test]
    public void DefaultColor_ConjunctionIsBlue()
    {
        var color = AspectSettings.DefaultColor(Aspects.Conjunction);
        Assert.That(color.Blue, Is.EqualTo(1.0));
        Assert.That(color.Red,  Is.EqualTo(0.0));
    }
}

[TestFixture]
public class OrbConfigTests
{
    [Test]
    public void Default_HasExpectedValues()
    {
        var o = OrbConfig.Default;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(o.OrbSystem,     Is.EqualTo(OrbSystems.Procentual));
            Assert.That(o.AspectBaseOrb, Is.EqualTo(10.0));
            Assert.That(o.MidpointOrb,  Is.EqualTo(1.6));
            Assert.That(o.HarmonicOrb,  Is.EqualTo(2.0));
            Assert.That(o.ParallelOrb,  Is.EqualTo(1.0));
        }
    }
}

[TestFixture]
public class GlyphsConfigTests
{
    [Test]
    public void Default_SignGlyphs_HasTwelveEntries()
    {
        Assert.That(GlyphsConfig.Default.SignGlyphs.Count, Is.EqualTo(12));
    }

    [Test]
    public void Default_AspectGlyphs_HasAllAspects()
    {
        Assert.That(GlyphsConfig.Default.AspectGlyphs.Count,
            Is.EqualTo(Enum.GetValues<Aspects>().Length));
    }

    [Test]
    public void Default_FactorGlyphs_HasAllFactors()
    {
        Assert.That(GlyphsConfig.Default.FactorGlyphs.Count,
            Is.EqualTo(Enum.GetValues<Factors>().Length));
    }

    [Test]
    public void Default_AriesGlyph_IsCorrect()
    {
        var aries = GlyphsConfig.Default.SignGlyphs.First(s => s.Sign == Signs.Aries);
        Assert.That(aries.Glyph, Is.EqualTo("\uE000"));
    }
}

[TestFixture]
public class GlyphCandidatesTests
{
    [Test]
    public void ForSign_Capricorn_HasTwoCandidates()
    {
        var candidates = GlyphCandidates.ForSign(Signs.Capricorn);
        Assert.That(candidates.Count, Is.EqualTo(2));
    }

    [Test]
    public void ForSign_Aries_HasOneCandidate()
    {
        Assert.That(GlyphCandidates.ForSign(Signs.Aries).Count, Is.EqualTo(1));
    }

    [Test]
    public void ForFactor_Pluto_HasSixCandidates()
    {
        Assert.That(GlyphCandidates.ForFactor(Factors.Pluto).Count, Is.EqualTo(6));
    }

    [Test]
    public void ForAspect_Conjunction_HasOneCandidate()
    {
        Assert.That(GlyphCandidates.ForAspect(Aspects.Conjunction).Count, Is.EqualTo(1));
    }
}

[TestFixture]
public class ProgressionsConfigTests
{
    [Test]
    public void Default_Transits_HasTwelveFactors()
    {
        Assert.That(TransitsConfig.Default.Factors.Count, Is.EqualTo(12));
    }

    [Test]
    public void Default_PrimaryDirections_PromissorsAreClassicPlanets()
    {
        var promissors = PrimaryDirectionsConfig.Default.Promissors;
        Assert.That(promissors, Contains.Item(Factors.Sun));
        Assert.That(promissors, Contains.Item(Factors.Saturn));
        Assert.That(promissors.Count, Is.EqualTo(7));
    }

    [Test]
    public void Default_PrimaryDirections_TimeKeyIsNaibod()
    {
        Assert.That(PrimaryDirectionsConfig.Default.TimeKey, Is.EqualTo(PrimaryTimeKeys.Naibod));
    }

    [Test]
    public void Default_SolarReturn_MethodIsTropicalParallax()
    {
        Assert.That(SolarReturnConfig.Default.Method, Is.EqualTo(SolarMethods.TropicalParallax));
    }

    [Test]
    public void Default_SymbolicDirections_TimeKeyIsOneDegree()
    {
        Assert.That(SymbolicDirectionsConfig.Default.TimeKey, Is.EqualTo(SymbolicTimeKeys.OneDegree));
    }
}
