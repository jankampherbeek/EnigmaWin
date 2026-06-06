// ZodiacDivisionsOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.ZodiacDivisions;

[TestFixture]
public class ZodiacDivisionsOrchestratorTests
{
    // MARK: - Signs

    [Test]
    public void Signs_AriesAt0_15_29999_ReturnsIndex0()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.Signs), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(15.0, ZodiacDivisionMethod.Signs), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(29.999, ZodiacDivisionMethod.Signs), Is.EqualTo(0));
    }

    [Test]
    public void Signs_TaurusAt30_45_ReturnsIndex1()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(30.0, ZodiacDivisionMethod.Signs), Is.EqualTo(1));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(45.0, ZodiacDivisionMethod.Signs), Is.EqualTo(1));
    }

    [Test]
    public void Signs_PiscesAt330_359999_ReturnsIndex11()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(330.0, ZodiacDivisionMethod.Signs), Is.EqualTo(11));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(359.999, ZodiacDivisionMethod.Signs), Is.EqualTo(11));
    }

    [Test]
    public void Signs_LibraAt180_ReturnsIndex6()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.Signs), Is.EqualTo(6));
    }

    // MARK: - DecansPlanet

    [Test]
    public void DecansPlanet_Decans1Through7_ReturnCorrectPlanetIndices()
    {
        // Decan 1 (0–9.999°): Mars (4)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(4));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(5.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(4));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(9.999, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(4));
        // Decan 2 (10–19.999°): Sun (0)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(10.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(15.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(0));
        // Decan 3 (20–29.999°): Venus (3)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(20.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(3));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(25.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(3));
        // Decan 4 (30–39.999°): Mercury (2)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(30.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(2));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(35.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(2));
        // Decan 5 (40–49.999°): Moon (1)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(40.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(1));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(45.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(1));
        // Decan 6 (50–59.999°): Saturn (6)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(50.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(6));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(55.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(6));
        // Decan 7 (60–69.999°): Jupiter (5)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(60.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(5));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(65.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(5));
    }

    [Test]
    public void DecansPlanet_At180_ReturnsMoon()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(1));
    }

    // MARK: - DecansSign

    [Test]
    public void DecansSign_AriesDecansMapToAriesLeoSagittarius()
    {
        // 1st decan of Aries (0–9.999°): Aries (0)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(5.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(0));
        // 2nd decan (10–19.999°): Leo (4)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(10.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(4));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(15.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(4));
        // 3rd decan (20–29.999°): Sagittarius (8)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(20.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(8));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(25.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(8));
    }

    [Test]
    public void DecansSign_At180_ReturnsLibra()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.DecansSign), Is.EqualTo(6));
    }

    // MARK: - DodecatsOriginal

    [Test]
    public void DodecatsOriginal_AriesSubportions1_2_3_ReturnExpectedSigns()
    {
        // 0–2.499°: Aries (0)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(1.0, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(0));
        // 2.5–4.999°: Taurus (1)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(2.5, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(1));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(4.0, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(1));
        // 5.0–7.499°: Gemini (2)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(5.0, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(2));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(7.0, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(2));
    }

    [Test]
    public void DodecatsOriginal_At180_ReturnsLibra()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.DodecatsOriginal), Is.EqualTo(6));
    }

    // MARK: - DodecatsPaulus

    [Test]
    public void DodecatsPaulus_0deg_30deg_45deg_ReturnExpectedSigns()
    {
        // 0° × 13 = 0 → Aries (0)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.DodecatsPaulus), Is.EqualTo(0));
        // 30° × 13 = 390 → 390 - 360 = 30 → Taurus (1)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(30.0, ZodiacDivisionMethod.DodecatsPaulus), Is.EqualTo(1));
        // 45° × 13 = 585 → 585 - 360 = 225 → Scorpio (7)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(45.0, ZodiacDivisionMethod.DodecatsPaulus), Is.EqualTo(7));
    }

    [Test]
    public void DodecatsPaulus_At180_ReturnsLibra()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.DodecatsPaulus), Is.EqualTo(6));
    }

    // MARK: - BoundsEgyptian

    [Test]
    public void BoundsEgyptian_AriesBounds_ReturnJupiterVenusMercuryMarsSaturn()
    {
        // 0–5.999°: Jupiter (5)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.BoundsEgyptian), Is.EqualTo(5));
        // 6–11.999°: Venus (3)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(6.0, ZodiacDivisionMethod.BoundsEgyptian), Is.EqualTo(3));
        // 12–19.999°: Mercury (2)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(12.0, ZodiacDivisionMethod.BoundsEgyptian), Is.EqualTo(2));
        // 20–24.999°: Mars (4)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(20.0, ZodiacDivisionMethod.BoundsEgyptian), Is.EqualTo(4));
        // 25–29.999°: Saturn (6)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(25.0, ZodiacDivisionMethod.BoundsEgyptian), Is.EqualTo(6));
    }

    [Test]
    public void BoundsEgyptian_At180_ReturnsSaturn()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.BoundsEgyptian), Is.EqualTo(6));
    }

    // MARK: - BoundsPtolemy

    [Test]
    public void BoundsPtolemy_AriesBounds_ReturnJupiterVenusMercuryMarsSaturn()
    {
        // 0–5.999°: Jupiter (5)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.BoundsPtolemy), Is.EqualTo(5));
        // 6–13.999°: Venus (3)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(6.0, ZodiacDivisionMethod.BoundsPtolemy), Is.EqualTo(3));
        // 14–20.999°: Mercury (2)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(14.0, ZodiacDivisionMethod.BoundsPtolemy), Is.EqualTo(2));
        // 21–25.999°: Mars (4)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(21.0, ZodiacDivisionMethod.BoundsPtolemy), Is.EqualTo(4));
        // 26–29.999°: Saturn (6)
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(26.0, ZodiacDivisionMethod.BoundsPtolemy), Is.EqualTo(6));
    }

    [Test]
    public void BoundsPtolemy_At180_ReturnsSaturn()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(180.0, ZodiacDivisionMethod.BoundsPtolemy), Is.EqualTo(6));
    }

    // MARK: - Invalid longitudes

    [Test]
    public void AllMethods_OutOfBoundsLongitudes_ReturnMinusOne()
    {
        var methods = new[]
        {
            ZodiacDivisionMethod.Signs, ZodiacDivisionMethod.DecansPlanet, ZodiacDivisionMethod.DecansSign,
            ZodiacDivisionMethod.DodecatsOriginal, ZodiacDivisionMethod.DodecatsPaulus,
            ZodiacDivisionMethod.BoundsEgyptian, ZodiacDivisionMethod.BoundsPtolemy
        };
        foreach (var method in methods)
        {
            Assert.That(ZodiacDivisionsOrchestrator.Calculate(-0.1, method), Is.EqualTo(-1), $"method={method}, lon=-0.1");
            Assert.That(ZodiacDivisionsOrchestrator.Calculate(360.0, method), Is.EqualTo(-1), $"method={method}, lon=360");
            Assert.That(ZodiacDivisionsOrchestrator.Calculate(-1.0, method), Is.EqualTo(-1), $"method={method}, lon=-1");
            Assert.That(ZodiacDivisionsOrchestrator.Calculate(720.0, method), Is.EqualTo(-1), $"method={method}, lon=720");
        }
    }

    // MARK: - Boundary values

    [Test]
    public void Signs_BoundaryLongitudes_0_30_359999()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.Signs), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(30.0, ZodiacDivisionMethod.Signs), Is.EqualTo(1));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(359.999, ZodiacDivisionMethod.Signs), Is.EqualTo(11));
    }

    [Test]
    public void DecansPlanet_ExactBoundaries_0_10_20()
    {
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(0.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(4));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(10.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(0));
        Assert.That(ZodiacDivisionsOrchestrator.Calculate(20.0, ZodiacDivisionMethod.DecansPlanet), Is.EqualTo(3));
    }
}
