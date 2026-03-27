// ConfigDataTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.Config;

/// <summary>Tests for CalculationConfig record.</summary>
[TestFixture]
public class ConfigDataTests
{
    // MARK: - Initialization Tests

    [Test]
    public void TestConfigDataInitialization()
    {
        var config = new CalculationConfig(
            HouseSystems.Placidus,
            Ayanamshas.Tropical,
            ObserverPositions.Geocentric,
            ProjectionTypes.TwoDimensional,
            BlackMoonCorrectionTypes.Duval,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.HouseSystem, Is.EqualTo(HouseSystems.Placidus));
            Assert.That(config.Ayanamsha, Is.EqualTo(Ayanamshas.Tropical));
            Assert.That(config.ObserverPosition, Is.EqualTo(ObserverPositions.Geocentric));
            Assert.That(config.ProjectionType, Is.EqualTo(ProjectionTypes.TwoDimensional));
            Assert.That(config.BlackMoonCorrectionType, Is.EqualTo(BlackMoonCorrectionTypes.Duval));
            Assert.That(config.LunarNodeType, Is.EqualTo(LunarNodeTypes.MeanNode));
            Assert.That(config.LotsType, Is.EqualTo(LotsTypes.Sect));
        }
    }

    [Test]
    public void TestConfigDataWithDefaultValues()
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            Ayanamshas.Tropical,
            ObserverPositions.Geocentric,
            ProjectionTypes.TwoDimensional,
            BlackMoonCorrectionTypes.Duval,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.HouseSystem, Is.EqualTo(HouseSystems.NoHouses));
            Assert.That(config.Ayanamsha, Is.EqualTo(Ayanamshas.Tropical));
            Assert.That(config.ObserverPosition, Is.EqualTo(ObserverPositions.Geocentric));
            Assert.That(config.ProjectionType, Is.EqualTo(ProjectionTypes.TwoDimensional));
            Assert.That(config.BlackMoonCorrectionType, Is.EqualTo(BlackMoonCorrectionTypes.Duval));
            Assert.That(config.LunarNodeType, Is.EqualTo(LunarNodeTypes.MeanNode));
            Assert.That(config.LotsType, Is.EqualTo(LotsTypes.Sect));
        }
    }

    [Test]
    public void TestConfigDataDefaultStatic()
    {
        var config = CalculationConfig.Default;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.HouseSystem, Is.EqualTo(HouseSystems.Placidus));
            Assert.That(config.Ayanamsha, Is.EqualTo(Ayanamshas.Tropical));
            Assert.That(config.ObserverPosition, Is.EqualTo(ObserverPositions.Geocentric));
            Assert.That(config.ProjectionType, Is.EqualTo(ProjectionTypes.TwoDimensional));
            Assert.That(config.BlackMoonCorrectionType, Is.EqualTo(BlackMoonCorrectionTypes.Duval));
            Assert.That(config.LunarNodeType, Is.EqualTo(LunarNodeTypes.MeanNode));
            Assert.That(config.LotsType, Is.EqualTo(LotsTypes.Sect));
        }
    }

    // MARK: - Property Access Tests

    [Test]
    public void TestHouseSystemProperty()
    {
        var config = CalculationConfig.Default with { HouseSystem = HouseSystems.Regiomontanus };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.HouseSystem, Is.EqualTo(HouseSystems.Regiomontanus));
            Assert.That((int)config.HouseSystem, Is.EqualTo(4));
        }
    }

    [Test]
    public void TestAyanamshaProperty()
    {
        var config = CalculationConfig.Default with { Ayanamsha = Ayanamshas.Lahiri };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.Ayanamsha, Is.EqualTo(Ayanamshas.Lahiri));
            Assert.That((int)config.Ayanamsha, Is.EqualTo(2));
        }
    }

    [Test]
    public void TestObserverPositionProperty()
    {
        var config = CalculationConfig.Default with { ObserverPosition = ObserverPositions.Topocentric };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.ObserverPosition, Is.EqualTo(ObserverPositions.Topocentric));
            Assert.That((int)config.ObserverPosition, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestProjectionTypeProperty()
    {
        var config = CalculationConfig.Default with { ProjectionType = ProjectionTypes.ObliqueLongitude };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.ProjectionType, Is.EqualTo(ProjectionTypes.ObliqueLongitude));
            Assert.That((int)config.ProjectionType, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestBlackMoonCorrectionTypeProperty()
    {
        var config = CalculationConfig.Default with { BlackMoonCorrectionType = BlackMoonCorrectionTypes.Swisseph };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.BlackMoonCorrectionType, Is.EqualTo(BlackMoonCorrectionTypes.Swisseph));
            Assert.That((int)config.BlackMoonCorrectionType, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestLunarNodeTypeProperty()
    {
        var config = CalculationConfig.Default with { LunarNodeType = LunarNodeTypes.TrueNode };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.LunarNodeType, Is.EqualTo(LunarNodeTypes.TrueNode));
            Assert.That((int)config.LunarNodeType, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestLotsTypeProperty()
    {
        var config = CalculationConfig.Default with { LotsType = LotsTypes.NoSect };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.LotsType, Is.EqualTo(LotsTypes.NoSect));
            Assert.That((int)config.LotsType, Is.EqualTo(1));
        }
    }

    // MARK: - With Expression (Immutability) Tests

    [Test]
    public void TestConfigDataWithExpression()
    {
        var config = CalculationConfig.Default with { HouseSystem = HouseSystems.Koch, Ayanamsha = Ayanamshas.Lahiri };
        Assert.That(config.HouseSystem, Is.EqualTo(HouseSystems.Koch));
        Assert.That(config.Ayanamsha, Is.EqualTo(Ayanamshas.Lahiri));
        Assert.That(CalculationConfig.Default.HouseSystem, Is.EqualTo(HouseSystems.Placidus));
    }

    // MARK: - All Enum Cases Tests

    [Test]
    public void TestAllHouseSystems()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var config = CalculationConfig.Default with { HouseSystem = houseSystem };
            Assert.That(config.HouseSystem, Is.EqualTo(houseSystem),
                $"House system {houseSystem} should be set correctly");
        }
    }

    [Test]
    public void TestAllAyanamshas()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var config = CalculationConfig.Default with { Ayanamsha = ayanamsha };
            Assert.That(config.Ayanamsha, Is.EqualTo(ayanamsha),
                $"Ayanamsha {ayanamsha} should be set correctly");
        }
    }

    [Test]
    public void TestAllObserverPositions()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        foreach (var observerPosition in allCases)
        {
            var config = CalculationConfig.Default with { ObserverPosition = observerPosition };
            Assert.That(config.ObserverPosition, Is.EqualTo(observerPosition),
                $"Observer position {observerPosition} should be set correctly");
        }
    }

    [Test]
    public void TestAllProjectionTypes()
    {
        var allCases = Enum.GetValues<ProjectionTypes>();
        foreach (var projectionType in allCases)
        {
            var config = CalculationConfig.Default with { ProjectionType = projectionType };
            Assert.That(config.ProjectionType, Is.EqualTo(projectionType),
                $"Projection type {projectionType} should be set correctly");
        }
    }

    [Test]
    public void TestAllBlackMoonCorrectionTypes()
    {
        var allCases = Enum.GetValues<BlackMoonCorrectionTypes>();
        foreach (var blackMoonCorrectionType in allCases)
        {
            var config = CalculationConfig.Default with { BlackMoonCorrectionType = blackMoonCorrectionType };
            Assert.That(config.BlackMoonCorrectionType, Is.EqualTo(blackMoonCorrectionType),
                $"Black moon correction type {blackMoonCorrectionType} should be set correctly");
        }
    }

    [Test]
    public void TestAllLunarNodeTypes()
    {
        var allCases = Enum.GetValues<LunarNodeTypes>();
        foreach (var lunarNodeType in allCases)
        {
            var config = CalculationConfig.Default with { LunarNodeType = lunarNodeType };
            Assert.That(config.LunarNodeType, Is.EqualTo(lunarNodeType),
                $"Lunar node type {lunarNodeType} should be set correctly");
        }
    }

    [Test]
    public void TestAllLotsTypes()
    {
        var allCases = Enum.GetValues<LotsTypes>();
        foreach (var lotsType in allCases)
        {
            var config = CalculationConfig.Default with { LotsType = lotsType };
            Assert.That(config.LotsType, Is.EqualTo(lotsType),
                $"Lots type {lotsType} should be set correctly");
        }
    }

    // MARK: - Combination Tests

    [Test]
    public void TestDifferentCombinations()
    {
        var config1 = new CalculationConfig(
            HouseSystems.Placidus,
            Ayanamshas.Tropical,
            ObserverPositions.Geocentric,
            ProjectionTypes.TwoDimensional,
            BlackMoonCorrectionTypes.Duval,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config1.HouseSystem, Is.EqualTo(HouseSystems.Placidus));
            Assert.That(config1.Ayanamsha, Is.EqualTo(Ayanamshas.Tropical));
            Assert.That(config1.ObserverPosition, Is.EqualTo(ObserverPositions.Geocentric));
        }

        var config2 = new CalculationConfig(
            HouseSystems.Regiomontanus,
            Ayanamshas.Lahiri,
            ObserverPositions.Topocentric,
            ProjectionTypes.ObliqueLongitude,
            BlackMoonCorrectionTypes.Swisseph,
            LunarNodeTypes.TrueNode,
            LotsTypes.NoSect);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config2.HouseSystem, Is.EqualTo(HouseSystems.Regiomontanus));
            Assert.That(config2.Ayanamsha, Is.EqualTo(Ayanamshas.Lahiri));
            Assert.That(config2.ObserverPosition, Is.EqualTo(ObserverPositions.Topocentric));
        }
    }

    // MARK: - Edge Cases Tests

    [Test]
    public void TestFirstEnumValues()
    {
        var config = new CalculationConfig(
            HouseSystems.NoHouses,
            Ayanamshas.Tropical,
            ObserverPositions.Geocentric,
            ProjectionTypes.TwoDimensional,
            BlackMoonCorrectionTypes.Duval,
            LunarNodeTypes.MeanNode,
            LotsTypes.Sect);
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)config.HouseSystem, Is.EqualTo(0));
            Assert.That((int)config.Ayanamsha, Is.EqualTo(0));
            Assert.That((int)config.ObserverPosition, Is.EqualTo(0));
            Assert.That((int)config.ProjectionType, Is.EqualTo(0));
            Assert.That((int)config.BlackMoonCorrectionType, Is.EqualTo(0));
            Assert.That((int)config.LunarNodeType, Is.EqualTo(0));
            Assert.That((int)config.LotsType, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestRawValuesPreserved()
    {
        var config = new CalculationConfig(
            HouseSystems.Regiomontanus,
            Ayanamshas.Krishnamurti,
            ObserverPositions.Heliocentric,
            ProjectionTypes.ObliqueLongitude,
            BlackMoonCorrectionTypes.Interpolated,
            LunarNodeTypes.TrueNode,
            LotsTypes.NoSect);
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)config.HouseSystem, Is.EqualTo(4));
            Assert.That((int)config.Ayanamsha, Is.EqualTo(6));
            Assert.That((int)config.ObserverPosition, Is.EqualTo(2));
            Assert.That((int)config.ProjectionType, Is.EqualTo(1));
            Assert.That((int)config.BlackMoonCorrectionType, Is.EqualTo(2));
            Assert.That((int)config.LunarNodeType, Is.EqualTo(1));
            Assert.That((int)config.LotsType, Is.EqualTo(1));
        }
    }
}
