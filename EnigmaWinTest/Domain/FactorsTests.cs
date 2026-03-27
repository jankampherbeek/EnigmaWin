// FactorsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for Factors domain types.</summary>
[TestFixture]
public class FactorsTests
{

    [Test]
    public void TestAllCasesCompleteness()
    {
        var allCases = Enum.GetValues<Factors>();
        Assert.That(allCases, Contains.Item(Factors.Sun));
        Assert.That(allCases, Contains.Item(Factors.Moon));
        Assert.That(allCases, Contains.Item(Factors.Ascendant));
        Assert.That(allCases, Contains.Item(Factors.Mc));
        Assert.That(allCases, Contains.Item(Factors.FortunaSect));
        Assert.That(allCases, Is.Not.Empty);
    }

    [Test]
    public void TestCalculationTypeCommonSeForPlanets()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Sun.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Moon.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Mercury.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Venus.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Earth.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Mars.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Jupiter.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Saturn.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Uranus.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Neptune.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Pluto.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
        }
    }

    [Test]
    public void TestCalculationTypeCommonSeForNodes()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.NorthNodeMean.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.NorthNodeTrue.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.ApogeeMean.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.ApogeeCorrected.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.ApogeeInterpolated.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.PerigeeInterpolated.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
        }
    }

    [Test]
    public void TestCalculationTypeCommonSeForAsteroids()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Chiron.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Pholus.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Ceres.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Pallas.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Juno.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Vesta.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Hygieia.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Astraea.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
        }
    }

    [Test]
    public void TestCalculationTypeCommonSeForUranian()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.CupidoUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.HadesUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.ZeusUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.KronosUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.ApollonUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.AdmetosUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.VulcanusUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.PoseidonUra.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
        }
    }

    [Test]
    public void TestCalculationTypeCommonSeForPlanetoids()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Eris.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Nessus.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Huya.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Varuna.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Ixion.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Quaoar.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Haumea.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Orcus.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Makemake.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Sedna.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
            Assert.That(Factors.Isis.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe));
        }
    }

    [Test]
    public void TestCalculationTypeCommonElements()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.PersephoneRam.CalculationType(), Is.EqualTo(CalculationTypes.CommonElements));
            Assert.That(Factors.HermesRam.CalculationType(), Is.EqualTo(CalculationTypes.CommonElements));
            Assert.That(Factors.DemeterRam.CalculationType(), Is.EqualTo(CalculationTypes.CommonElements));
        }
    }

    [Test]
    public void TestCalculationTypeCommonFormulaLongitude()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.PersephoneCarteret.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaLongitude));
            Assert.That(Factors.VulcanusCarteret.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaLongitude));
        }
    }

    [Test]
    public void TestCalculationTypeCommonFormulaFull()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Priapus.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaFull));
            Assert.That(Factors.PriapusCorrected.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaFull));
            Assert.That(Factors.Dragon.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaFull));
            Assert.That(Factors.Beast.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaFull));
            Assert.That(Factors.SouthNodeMean.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaFull));
            Assert.That(Factors.SouthNodeTrue.CalculationType(), Is.EqualTo(CalculationTypes.CommonFormulaFull));
        }
    }

    [Test]
    public void TestCalculationTypeMundane()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Mc.CalculationType(), Is.EqualTo(CalculationTypes.Mundane));
            Assert.That(Factors.Ascendant.CalculationType(), Is.EqualTo(CalculationTypes.Mundane));
            Assert.That(Factors.EastPoint.CalculationType(), Is.EqualTo(CalculationTypes.Mundane));
            Assert.That(Factors.Vertex.CalculationType(), Is.EqualTo(CalculationTypes.Mundane));
        }
    }

    [Test]
    public void TestCalculationTypeZodiacFixed()
    {
        Assert.That(Factors.ZeroAries.CalculationType(), Is.EqualTo(CalculationTypes.ZodiacFixed));
    }

    [Test]
    public void TestCalculationTypeLots()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.FortunaSect.CalculationType(), Is.EqualTo(CalculationTypes.Lots));
            Assert.That(Factors.FortunaNoSect.CalculationType(), Is.EqualTo(CalculationTypes.Lots));
        }
    }

    [Test]
    public void TestCalculationTypeApsides()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.BlackSun.CalculationType(), Is.EqualTo(CalculationTypes.Apsides));
            Assert.That(Factors.Diamond.CalculationType(), Is.EqualTo(CalculationTypes.Apsides));
        }
    }

    [Test]
    public void TestSeIdMajorPlanets()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Sun.SeId(), Is.EqualTo(0));
            Assert.That(Factors.Moon.SeId(), Is.EqualTo(1));
            Assert.That(Factors.Mercury.SeId(), Is.EqualTo(2));
            Assert.That(Factors.Venus.SeId(), Is.EqualTo(3));
            Assert.That(Factors.Mars.SeId(), Is.EqualTo(4));
            Assert.That(Factors.Jupiter.SeId(), Is.EqualTo(5));
            Assert.That(Factors.Saturn.SeId(), Is.EqualTo(6));
            Assert.That(Factors.Uranus.SeId(), Is.EqualTo(7));
            Assert.That(Factors.Neptune.SeId(), Is.EqualTo(8));
            Assert.That(Factors.Pluto.SeId(), Is.EqualTo(9));
        }
    }

    [Test]
    public void TestSeIdNodesAndApogees()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.NorthNodeMean.SeId(), Is.EqualTo(10));
            Assert.That(Factors.NorthNodeTrue.SeId(), Is.EqualTo(11));
            Assert.That(Factors.ApogeeMean.SeId(), Is.EqualTo(12));
            Assert.That(Factors.ApogeeCorrected.SeId(), Is.EqualTo(13));
            Assert.That(Factors.Earth.SeId(), Is.EqualTo(14));
            Assert.That(Factors.ApogeeInterpolated.SeId(), Is.EqualTo(21));
            Assert.That(Factors.PerigeeInterpolated.SeId(), Is.EqualTo(22));
        }
    }

    [Test]
    public void TestSeIdAsteroids()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Chiron.SeId(), Is.EqualTo(15));
            Assert.That(Factors.Pholus.SeId(), Is.EqualTo(16));
            Assert.That(Factors.Ceres.SeId(), Is.EqualTo(17));
            Assert.That(Factors.Pallas.SeId(), Is.EqualTo(18));
            Assert.That(Factors.Juno.SeId(), Is.EqualTo(19));
            Assert.That(Factors.Vesta.SeId(), Is.EqualTo(20));
            Assert.That(Factors.Hygieia.SeId(), Is.EqualTo(10010));
            Assert.That(Factors.Astraea.SeId(), Is.EqualTo(10005));
        }
    }

    [Test]
    public void TestSeIdUranian()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.CupidoUra.SeId(), Is.EqualTo(40));
            Assert.That(Factors.HadesUra.SeId(), Is.EqualTo(41));
            Assert.That(Factors.ZeusUra.SeId(), Is.EqualTo(42));
            Assert.That(Factors.KronosUra.SeId(), Is.EqualTo(43));
            Assert.That(Factors.ApollonUra.SeId(), Is.EqualTo(44));
            Assert.That(Factors.AdmetosUra.SeId(), Is.EqualTo(45));
            Assert.That(Factors.VulcanusUra.SeId(), Is.EqualTo(46));
            Assert.That(Factors.PoseidonUra.SeId(), Is.EqualTo(47));
            Assert.That(Factors.Isis.SeId(), Is.EqualTo(48));
        }
    }

    [Test]
    public void TestSeIdPlanetoids()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Eris.SeId(), Is.EqualTo(1009001));
            Assert.That(Factors.Nessus.SeId(), Is.EqualTo(17066));
            Assert.That(Factors.Huya.SeId(), Is.EqualTo(48628));
            Assert.That(Factors.Varuna.SeId(), Is.EqualTo(30000));
            Assert.That(Factors.Ixion.SeId(), Is.EqualTo(38978));
            Assert.That(Factors.Quaoar.SeId(), Is.EqualTo(60000));
            Assert.That(Factors.Haumea.SeId(), Is.EqualTo(146108));
            Assert.That(Factors.Orcus.SeId(), Is.EqualTo(100482));
            Assert.That(Factors.Makemake.SeId(), Is.EqualTo(146472));
            Assert.That(Factors.Sedna.SeId(), Is.EqualTo(100377));
        }
    }

    [Test]
    public void TestSeIdRam()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.PersephoneRam.SeId(), Is.EqualTo(300));
            Assert.That(Factors.HermesRam.SeId(), Is.EqualTo(301));
            Assert.That(Factors.DemeterRam.SeId(), Is.EqualTo(302));
        }
    }

    [Test]
    public void TestSeIdCarteret()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.PersephoneCarteret.SeId(), Is.EqualTo(400));
            Assert.That(Factors.VulcanusCarteret.SeId(), Is.EqualTo(401));
        }
    }

    [Test]
    public void TestSeIdFormula()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Priapus.SeId(), Is.EqualTo(501));
            Assert.That(Factors.PriapusCorrected.SeId(), Is.EqualTo(502));
            Assert.That(Factors.Dragon.SeId(), Is.EqualTo(503));
            Assert.That(Factors.Beast.SeId(), Is.EqualTo(504));
            Assert.That(Factors.SouthNodeMean.SeId(), Is.EqualTo(505));
            Assert.That(Factors.SouthNodeTrue.SeId(), Is.EqualTo(506));
        }
    }

    [Test]
    public void TestSeIdApsides()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.BlackSun.SeId(), Is.EqualTo(601));
            Assert.That(Factors.Diamond.SeId(), Is.EqualTo(602));
        }
    }

    [Test]
    public void TestSeIdMundane()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Mc.SeId(), Is.EqualTo(700));
            Assert.That(Factors.Ascendant.SeId(), Is.EqualTo(701));
            Assert.That(Factors.EastPoint.SeId(), Is.EqualTo(702));
            Assert.That(Factors.Vertex.SeId(), Is.EqualTo(703));
        }
    }

    [Test]
    public void TestSeIdFixedAndLots()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.ZeroAries.SeId(), Is.EqualTo(800));
            Assert.That(Factors.FortunaSect.SeId(), Is.EqualTo(900));
            Assert.That(Factors.FortunaNoSect.SeId(), Is.EqualTo(901));
        }
    }

    // MARK: - LocalizedName Tests

    [Test]
    public void TestLocalizedNameMajorPlanets()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Sun.LocalizedName(), Is.EqualTo("enum.factor.sun"));
            Assert.That(Factors.Moon.LocalizedName(), Is.EqualTo("enum.factor.moon"));
            Assert.That(Factors.Mercury.LocalizedName(), Is.EqualTo("enum.factor.mercury"));
            Assert.That(Factors.Venus.LocalizedName(), Is.EqualTo("enum.factor.venus"));
            Assert.That(Factors.Earth.LocalizedName(), Is.EqualTo("enum.factor.earth"));
            Assert.That(Factors.Mars.LocalizedName(), Is.EqualTo("enum.factor.mars"));
            Assert.That(Factors.Jupiter.LocalizedName(), Is.EqualTo("enum.factor.jupiter"));
            Assert.That(Factors.Saturn.LocalizedName(), Is.EqualTo("enum.factor.saturn"));
            Assert.That(Factors.Uranus.LocalizedName(), Is.EqualTo("enum.factor.uranus"));
            Assert.That(Factors.Neptune.LocalizedName(), Is.EqualTo("enum.factor.neptune"));
            Assert.That(Factors.Pluto.LocalizedName(), Is.EqualTo("enum.factor.pluto"));
        }
    }

    [Test]
    public void TestLocalizedNameNodes()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.NorthNodeMean.LocalizedName(), Is.EqualTo("enum.factor.northnode"));
            Assert.That(Factors.NorthNodeTrue.LocalizedName(), Is.EqualTo("enum.factor.truenode"));
            Assert.That(Factors.SouthNodeMean.LocalizedName(), Is.EqualTo("enum.factor.southnodemean"));
            Assert.That(Factors.SouthNodeTrue.LocalizedName(), Is.EqualTo("enum.factor.southnodetrue"));
        }
    }

    [Test]
    public void TestLocalizedNameAsteroids()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Chiron.LocalizedName(), Is.EqualTo("enum.factor.chiron"));
            Assert.That(Factors.Ceres.LocalizedName(), Is.EqualTo("enum.factor.ceres"));
            Assert.That(Factors.Pallas.LocalizedName(), Is.EqualTo("enum.factor.pallas"));
            Assert.That(Factors.Juno.LocalizedName(), Is.EqualTo("enum.factor.juno"));
            Assert.That(Factors.Vesta.LocalizedName(), Is.EqualTo("enum.factor.vesta"));
        }
    }

    [Test]
    public void TestLocalizedNameMundane()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.Ascendant.LocalizedName(), Is.EqualTo("enum.factor.ascendant"));
            Assert.That(Factors.Mc.LocalizedName(), Is.EqualTo("enum.factor.mc"));
            Assert.That(Factors.EastPoint.LocalizedName(), Is.EqualTo("enum.factor.eastpoint"));
            Assert.That(Factors.Vertex.LocalizedName(), Is.EqualTo("enum.factor.vertex"));
        }
    }

    [Test]
    public void TestLocalizedNameLots()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Factors.FortunaSect.LocalizedName(), Is.EqualTo("enum.factor.fortunasect"));
            Assert.That(Factors.FortunaNoSect.LocalizedName(), Is.EqualTo("enum.factor.fortunanosect"));
        }
    }

    [Test]
    public void TestLocalizedNameAllFactors()
    {
        var allCases = Enum.GetValues<Factors>();
        foreach (var factor in allCases)
        {
            var name = factor.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Factor {factor} has empty localized name");
            Assert.That(name, Does.StartWith("enum.factor."), $"Factor {factor} localized name does not start with 'enum.factor.'");
        }
    }

    // MARK: - FromIndex Tests

    [Test]
    public void TestFromIndexValid()
    {
        var allCases = Enum.GetValues<Factors>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var expectedFactor = allCases[index];
            var factor = FactorsExtensions.FromIndex(index);
            Assert.That(factor, Is.EqualTo(expectedFactor), $"Index {index} should return {expectedFactor}");
        }
    }

    [Test]
    public void TestFromIndexFirst()
    {
        var factor = FactorsExtensions.FromIndex(0);
        var allCases = Enum.GetValues<Factors>();
        Assert.That(factor, Is.EqualTo(allCases.First()));
    }

    [Test]
    public void TestFromIndexLast()
    {
        var allCases = Enum.GetValues<Factors>();
        var lastIndex = allCases.Length - 1;
        var factor = FactorsExtensions.FromIndex(lastIndex);
        Assert.That(factor, Is.EqualTo(allCases.Last()));
    }

    [Test]
    public void TestFromIndexNegative()
    {
        var factor = FactorsExtensions.FromIndex(-1);
        Assert.That(factor, Is.Null);
    }

    [Test]
    public void TestFromIndexTooLarge()
    {
        var allCases = Enum.GetValues<Factors>();
        var tooLargeIndex = allCases.Length;
        var factor = FactorsExtensions.FromIndex(tooLargeIndex);
        Assert.That(factor, Is.Null);
    }

    [Test]
    public void TestFromIndexBoundary()
    {
        var allCases = Enum.GetValues<Factors>();
        var boundaryIndex = allCases.Length;
        var factor = FactorsExtensions.FromIndex(boundaryIndex);
        Assert.That(factor, Is.Null);

        var validBoundaryIndex = allCases.Length - 1;
        var validFactor = FactorsExtensions.FromIndex(validBoundaryIndex);
        Assert.That(validFactor, Is.Not.Null);
    }

    [Test]
    public void TestRawValuesUnique()
    {
        var rawValues = new HashSet<int>();
        var allCases = Enum.GetValues<Factors>();
        foreach (var factor in allCases)
        {
            var rawValue = (int)factor;
            Assert.That(rawValues, Does.Not.Contain(rawValue), $"Duplicate raw value {rawValue} found for factor {factor}");
            rawValues.Add(rawValue);
        }
    }

    [Test]
    public void TestRawValuesMatchExpected()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)Factors.Sun, Is.EqualTo(0));
            Assert.That((int)Factors.Moon, Is.EqualTo(1));
            Assert.That((int)Factors.Mercury, Is.EqualTo(2));
            Assert.That((int)Factors.Venus, Is.EqualTo(3));
            Assert.That((int)Factors.Earth, Is.EqualTo(4));
            Assert.That((int)Factors.Mars, Is.EqualTo(5));
            Assert.That((int)Factors.Jupiter, Is.EqualTo(6));
            Assert.That((int)Factors.Saturn, Is.EqualTo(7));
            Assert.That((int)Factors.Uranus, Is.EqualTo(8));
            Assert.That((int)Factors.Neptune, Is.EqualTo(9));
            Assert.That((int)Factors.Pluto, Is.EqualTo(10));
            Assert.That((int)Factors.Ascendant, Is.EqualTo(1001));
            Assert.That((int)Factors.Mc, Is.EqualTo(1002));
            Assert.That((int)Factors.ZeroAries, Is.EqualTo(3001));
            Assert.That((int)Factors.FortunaSect, Is.EqualTo(4001));
        }
    }

    // MARK: - Comprehensive Tests

    [Test]
    public void TestAllFactorsHaveValidCalculationType()
    {
        var allCases = Enum.GetValues<Factors>();
        foreach (var factor in allCases)
        {
            var calculationType = factor.CalculationType();
            // Verify that calculation type is not Unknown (which indicates unhandled case)
            Assert.That(calculationType, Is.Not.EqualTo(CalculationTypes.Unknown), $"Factor {factor} has Unknown calculation type");
        }
    }

    [Test]
    public void TestAllFactorsHaveSeId()
    {
        var allCases = Enum.GetValues<Factors>();
        foreach (var factor in allCases)
        {
            var seId = factor.SeId();
            // SE ID can be 0 for some factors, but should be defined (non-negative)
            Assert.That(seId, Is.GreaterThanOrEqualTo(0), $"Factor {factor} has negative SE ID");
        }
    }

    [Test]
    public void TestCommonSeFactorsHaveValidSeIds()
    {
        var commonSeFactors = new[]
        {
            Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus, Factors.Earth, Factors.Mars,
            Factors.Jupiter, Factors.Saturn, Factors.Uranus, Factors.Neptune, Factors.Pluto,
            Factors.NorthNodeMean, Factors.NorthNodeTrue, Factors.ApogeeMean, Factors.ApogeeCorrected,
            Factors.Chiron, Factors.Pholus, Factors.Ceres, Factors.Pallas, Factors.Juno, Factors.Vesta,
            Factors.ApogeeInterpolated, Factors.PerigeeInterpolated, Factors.CupidoUra, Factors.HadesUra,
            Factors.ZeusUra, Factors.KronosUra, Factors.ApollonUra, Factors.AdmetosUra,
            Factors.VulcanusUra, Factors.PoseidonUra, Factors.Isis, Factors.Eris, Factors.Nessus,
            Factors.Huya, Factors.Varuna, Factors.Ixion, Factors.Quaoar, Factors.Haumea, Factors.Orcus,
            Factors.Makemake, Factors.Sedna, Factors.Hygieia, Factors.Astraea
        };

        foreach (var factor in commonSeFactors)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(factor.CalculationType(), Is.EqualTo(CalculationTypes.CommonSe), $"Factor {factor} should have CommonSe calculation type");
                Assert.That(factor.SeId(), Is.GreaterThan(-1), $"Factor {factor} should have valid SE ID");
            }
        }
    }

    [Test]
    public void TestCalculationTypeAndSeIdConsistency()
    {
        // Factors with CommonSe should generally have non-zero SE IDs
        var allCases = Enum.GetValues<Factors>();
        var commonSeFactors = allCases.Where(f => f.CalculationType() == CalculationTypes.CommonSe);

        foreach (var factor in commonSeFactors)
        {
            // Most CommonSe factors should have meaningful SE IDs
            // Some might have 0 as default, but major ones should not
            if (factor is not (Factors.Sun or Factors.Moon or Factors.Mercury or Factors.Venus or Factors.Mars
                or Factors.Jupiter or Factors.Saturn or Factors.Uranus or Factors.Neptune or Factors.Pluto)) continue;
            var seId = factor.SeId();
            Assert.That(seId, Is.GreaterThanOrEqualTo(0).And.LessThanOrEqualTo(9), $"Major planet {factor} should have SE ID between 0 and 9");
        }
    }

    [Test]
    public void TestEnumIsIterable()
    {
        var allCases = Enum.GetValues<Factors>();
        Assert.That(allCases, Is.Not.Empty);

        // Verify we can iterate
        var count = 0;
        foreach (var _ in allCases)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(allCases.Length));
    }

    [Test]
    public void TestEnumIsIntBacked()
    {
        // Test that we can create from raw value
        const Factors sun = 0;
        Assert.That(sun, Is.EqualTo(Factors.Sun));

        const Factors moon = (Factors)1;
        Assert.That(moon, Is.EqualTo(Factors.Moon));
    }
}

