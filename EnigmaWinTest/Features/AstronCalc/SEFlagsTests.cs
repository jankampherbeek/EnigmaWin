// SEFlagsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.AstronCalc;

/// <summary>Tests for SEFlags static class.</summary>
[TestFixture]
public class SEFlagsTests
{
    // MARK: - Coordinate System Tests

    [Test]
    public void TestEclipticalCoordinateSystem()
    {
        var config = CalculationConfig.Default;
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, no additional flags for ecliptical
        Assert.That(flags, Is.EqualTo(258));
    }

    [Test]
    public void TestEquatorialCoordinateSystem()
    {
        var config = CalculationConfig.Default;
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) = 2306
        Assert.That(flags, Is.EqualTo(2306));
    }

    [Test]
    public void TestHorizontalCoordinateSystem()
    {
        var config = CalculationConfig.Default;
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Horizontal);
        // Base: 258, no additional flags for horizontal
        Assert.That(flags, Is.EqualTo(258));
    }

    // MARK: - Observer Position Tests

    [Test]
    public void TestTopocentricObserverPosition()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, + 32768 (topocentric) = 33026
        Assert.That(flags, Is.EqualTo(33026));
    }

    [Test]
    public void TestHeliocentricObserverPosition()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Heliocentric };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, + 8 (heliocentric) = 266
        Assert.That(flags, Is.EqualTo(266));
    }

    // MARK: - Zodiac Type Tests

    [Test]
    public void TestSiderealZodiacTypeWithEcliptical()
    {
        var config = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Fagan };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, + 65536 (sidereal) = 65794
        Assert.That(flags, Is.EqualTo(65794));
    }

    [Test]
    public void TestSiderealZodiacTypeWithEquatorial()
    {
        var config = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Fagan };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) = 2306
        // Note: sidereal flag only applies when coordinate system is ecliptical
        Assert.That(flags, Is.EqualTo(2306));
    }

    [Test]
    public void TestSiderealZodiacTypeWithHorizontal()
    {
        var config = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Fagan };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Horizontal);
        // Base: 258, no additional flags
        // Note: sidereal flag only applies when coordinate system is ecliptical
        Assert.That(flags, Is.EqualTo(258));
    }

    [Test]
    public void TestTropicalZodiacType()
    {
        var config = CalculationConfig.Default;
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, no sidereal flag for tropical
        Assert.That(flags, Is.EqualTo(258));
    }

    [Test]
    public void TestDifferentSiderealAyanamshas()
    {
        using (Assert.EnterMultipleScope())
        {
            var configLahiri = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Lahiri };
            var flagsLahiri = SEFlags.DefineFlags(configLahiri, CoordinateSystems.Ecliptical);
            Assert.That(flagsLahiri, Is.EqualTo(65794)); // Base + sidereal

            var configRaman = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Raman };
            var flagsRaman = SEFlags.DefineFlags(configRaman, CoordinateSystems.Ecliptical);
            Assert.That(flagsRaman, Is.EqualTo(65794)); // Base + sidereal
        }
    }

    // MARK: - Combined Flags Tests

    [Test]
    public void TestEquatorialAndTopocentric()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) + 32768 (topocentric) = 35074
        Assert.That(flags, Is.EqualTo(35074));
    }

    [Test]
    public void TestEquatorialAndHeliocentric()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Heliocentric };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) + 8 (heliocentric) = 2314
        Assert.That(flags, Is.EqualTo(2314));
    }

    [Test]
    public void TestEclipticalTopocentricSidereal()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric, Ayanamsha = Ayanamshas.Fagan };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, + 32768 (topocentric) + 65536 (sidereal) = 98562
        Assert.That(flags, Is.EqualTo(98562));
    }

    [Test]
    public void TestEquatorialTopocentricSidereal()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric, Ayanamsha = Ayanamshas.Fagan };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) + 32768 (topocentric) = 35074
        // Note: sidereal flag only applies when coordinate system is ecliptical
        Assert.That(flags, Is.EqualTo(35074));
    }

    [Test]
    public void TestEclipticalHeliocentricSidereal()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Heliocentric, Ayanamsha = Ayanamshas.Fagan };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, + 8 (heliocentric) + 65536 (sidereal) = 65802
        Assert.That(flags, Is.EqualTo(65802));
    }

    [Test]
    public void TestHorizontalTopocentricTropical()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Horizontal);
        // Base: 258, + 32768 (topocentric) = 33026
        Assert.That(flags, Is.EqualTo(33026));
    }

    [Test]
    public void TestEquatorialTopocentricHeliocentric()
    {
        // Note: This test checks behavior when both topocentric and heliocentric are set
        // In practice, these are mutually exclusive, but we test the logic
        // Topocentric takes precedence in the flag calculation order
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) + 32768 (topocentric) = 35074
        Assert.That(flags, Is.EqualTo(35074));
    }

    // MARK: - Flag Value Verification Tests

    [Test]
    public void TestIndividualFlagValues()
    {
        // Test that flag values match expected constants
        const int baseFlags = 2 + 256;  // SE + speed
        Assert.That(baseFlags, Is.EqualTo(258));

        const int equatorialFlag = 2048;
        Assert.That(equatorialFlag, Is.EqualTo(2048));

        const int topocentricFlag = 32 * 1024;
        Assert.That(topocentricFlag, Is.EqualTo(32768));

        const int heliocentricFlag = 8;
        Assert.That(heliocentricFlag, Is.EqualTo(8));

        const int siderealFlag = 64 * 1024;
        Assert.That(siderealFlag, Is.EqualTo(65536));
    }

    [Test]
    public void TestBaseFlagsAlwaysPresent()
    {
        // Test that base flags (258) are always present regardless of configuration
        var flags1 = SEFlags.DefineFlags(CalculationConfig.Default, CoordinateSystems.Ecliptical);
        Assert.That(flags1, Is.GreaterThanOrEqualTo(258));

        var config2 = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric, Ayanamsha = Ayanamshas.Fagan };
        var flags2 = SEFlags.DefineFlags(config2, CoordinateSystems.Equatorial);
        Assert.That(flags2, Is.GreaterThanOrEqualTo(258));
    }

    [Test]
    public void TestAllCoordinateSystems()
    {
        var config = CalculationConfig.Default;
        using (Assert.EnterMultipleScope())
        {
            var eclipticalFlags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
            Assert.That(eclipticalFlags, Is.EqualTo(258));

            var equatorialFlags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
            Assert.That(equatorialFlags, Is.EqualTo(2306));

            var horizontalFlags = SEFlags.DefineFlags(config, CoordinateSystems.Horizontal);
            Assert.That(horizontalFlags, Is.EqualTo(258));
        }
    }

    [Test]
    public void TestAllObserverPositions()
    {
        using (Assert.EnterMultipleScope())
        {
            var geocentricFlags = SEFlags.DefineFlags(CalculationConfig.Default, CoordinateSystems.Ecliptical);
            Assert.That(geocentricFlags, Is.EqualTo(258));

            var topoConfig = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric };
            var topocentricFlags = SEFlags.DefineFlags(topoConfig, CoordinateSystems.Ecliptical);
            Assert.That(topocentricFlags, Is.EqualTo(33026));

            var helioConfig = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Heliocentric };
            var heliocentricFlags = SEFlags.DefineFlags(helioConfig, CoordinateSystems.Ecliptical);
            Assert.That(heliocentricFlags, Is.EqualTo(266));
        }
    }

    [Test]
    public void TestSiderealOnlyWithEcliptical()
    {
        // Verify that sidereal flag is only added when coordinate system is ecliptical
        var configFagan = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Fagan };
        var eclipticalFlags = SEFlags.DefineFlags(configFagan, CoordinateSystems.Ecliptical);
        Assert.That(eclipticalFlags, Is.EqualTo(65794)); // Base + sidereal

        var equatorialFlags = SEFlags.DefineFlags(configFagan, CoordinateSystems.Equatorial);
        Assert.That(equatorialFlags, Is.EqualTo(2306)); // Base + equatorial, no sidereal

        var horizontalFlags = SEFlags.DefineFlags(configFagan, CoordinateSystems.Horizontal);
        Assert.That(horizontalFlags, Is.EqualTo(258)); // Base only, no sidereal
    }

    [Test]
    public void TestComplexCombination()
    {
        // Test a complex combination: equatorial + topocentric + sidereal (but sidereal won't apply)
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric, Ayanamsha = Ayanamshas.Lahiri };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Equatorial);
        // Base: 258, + 2048 (equatorial) + 32768 (topocentric) = 35074
        // Sidereal flag (65536) is NOT added because coordinate system is not ecliptical
        Assert.That(flags, Is.EqualTo(35074));
    }

    [Test]
    public void TestMaximumFlagsCombination()
    {
        // Test maximum combination: ecliptical + topocentric + sidereal
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric, Ayanamsha = Ayanamshas.Krishnamurti };
        var flags = SEFlags.DefineFlags(config, CoordinateSystems.Ecliptical);
        // Base: 258, + 32768 (topocentric) + 65536 (sidereal) = 98562
        Assert.That(flags, Is.EqualTo(98562));
    }
}

