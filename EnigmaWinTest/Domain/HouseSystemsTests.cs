using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for HouseSystems domain types.</summary>
[TestFixture]
public class HouseSystemsTests
{

    [Test]
    public void TestAllHouseSystemsCases()
    {
        // Test that all major cases exist
        var allCases = Enum.GetValues<HouseSystems>();
        Assert.That(allCases, Contains.Item(HouseSystems.NoHouses));
        Assert.That(allCases, Contains.Item(HouseSystems.Placidus));
        Assert.That(allCases, Contains.Item(HouseSystems.Koch));
        Assert.That(allCases, Contains.Item(HouseSystems.Porphyri));
        Assert.That(allCases, Contains.Item(HouseSystems.Regiomontanus));
        Assert.That(allCases, Contains.Item(HouseSystems.Campanus));
        Assert.That(allCases, Contains.Item(HouseSystems.Alcabitius));
        Assert.That(allCases, Contains.Item(HouseSystems.TopoCentric));
        Assert.That(allCases, Contains.Item(HouseSystems.Krusinski));
        Assert.That(allCases, Contains.Item(HouseSystems.Apc));
        Assert.That(allCases, Contains.Item(HouseSystems.Morin));
        Assert.That(allCases, Contains.Item(HouseSystems.WholeSign));
    }

    [Test]
    public void TestAllCasesCompleteness()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        Assert.That(allCases, Contains.Item(HouseSystems.NoHouses));
        Assert.That(allCases, Contains.Item(HouseSystems.Placidus));
        Assert.That(allCases, Contains.Item(HouseSystems.Koch));
        Assert.That(allCases, Contains.Item(HouseSystems.WholeSign));
        Assert.That(allCases, Contains.Item(HouseSystems.Sripati));
        Assert.That(allCases, Has.Length.EqualTo(25)); // Total number of house systems
    }

    // MARK: - SE ID Tests

    [Test]
    public void TestSeIdCommonHouseSystems()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.NoHouses.SeId(), Is.EqualTo('W'));
            Assert.That(HouseSystems.Placidus.SeId(), Is.EqualTo('P'));
            Assert.That(HouseSystems.Koch.SeId(), Is.EqualTo('K'));
            Assert.That(HouseSystems.Porphyri.SeId(), Is.EqualTo('O'));
            Assert.That(HouseSystems.Regiomontanus.SeId(), Is.EqualTo('R'));
            Assert.That(HouseSystems.Campanus.SeId(), Is.EqualTo('C'));
            Assert.That(HouseSystems.Alcabitius.SeId(), Is.EqualTo('B'));
            Assert.That(HouseSystems.TopoCentric.SeId(), Is.EqualTo('T'));
            Assert.That(HouseSystems.Krusinski.SeId(), Is.EqualTo('U'));
            Assert.That(HouseSystems.Apc.SeId(), Is.EqualTo('Y'));
            Assert.That(HouseSystems.Morin.SeId(), Is.EqualTo('M'));
        }
    }

    [Test]
    public void TestSeIdWholeSignAndEqual()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.WholeSign.SeId(), Is.EqualTo('W'));
            Assert.That(HouseSystems.EqualAsc.SeId(), Is.EqualTo('A'));
            Assert.That(HouseSystems.EqualMc.SeId(), Is.EqualTo('D'));
            Assert.That(HouseSystems.EqualAries.SeId(), Is.EqualTo('N'));
        }
    }

    [Test]
    public void TestSeIdSpecialized()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.Vehlow.SeId(), Is.EqualTo('V'));
            Assert.That(HouseSystems.Axial.SeId(), Is.EqualTo('X'));
            Assert.That(HouseSystems.Horizon.SeId(), Is.EqualTo('H'));
            Assert.That(HouseSystems.Carter.SeId(), Is.EqualTo('F'));
            Assert.That(HouseSystems.Gauquelin.SeId(), Is.EqualTo('G'));
        }
    }

    [Test]
    public void TestSeIdSunshineAndPullen()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.SunShine.SeId(), Is.EqualTo('i'));
            Assert.That(HouseSystems.SunShineTreindl.SeId(), Is.EqualTo('I'));
            Assert.That(HouseSystems.PullenSd.SeId(), Is.EqualTo('L'));
            Assert.That(HouseSystems.PullenSr.SeId(), Is.EqualTo('Q'));
            Assert.That(HouseSystems.Sripati.SeId(), Is.EqualTo('S'));
        }
    }

    [Test]
    public void TestSeIdSingleCharacter()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var seId = houseSystem.SeId();
            // Verify it's a single character (char is always a single character in C#)
            Assert.That(char.IsAscii(seId), Is.True, $"House system {houseSystem} SE ID should be ASCII");
        }
    }

    [Test]
    public void TestSeIdNoHousesAndWholeSignShare()
    {
        // Both NoHouses and WholeSign use "W" - this is intentional
        Assert.That(HouseSystems.NoHouses.SeId(), Is.EqualTo(HouseSystems.WholeSign.SeId()));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.NoHouses.SeId(), Is.EqualTo('W'));
            Assert.That(HouseSystems.WholeSign.SeId(), Is.EqualTo('W'));
        }
    }

    // MARK: - LocalizedName Tests

    [Test]
    public void TestLocalizedNameCommon()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.NoHouses.LocalizedName(), Is.EqualTo("enum.housesystem.nohouses"));
            Assert.That(HouseSystems.Placidus.LocalizedName(), Is.EqualTo("enum.housesystem.placidus"));
            Assert.That(HouseSystems.Koch.LocalizedName(), Is.EqualTo("enum.housesystem.koch"));
            Assert.That(HouseSystems.Porphyri.LocalizedName(), Is.EqualTo("enum.housesystem.porphyri"));
            Assert.That(HouseSystems.Regiomontanus.LocalizedName(), Is.EqualTo("enum.housesystem.regiomontanus"));
            Assert.That(HouseSystems.Campanus.LocalizedName(), Is.EqualTo("enum.housesystem.campanus"));
            Assert.That(HouseSystems.Alcabitius.LocalizedName(), Is.EqualTo("enum.housesystem.alcabitius"));
        }
    }

    [Test]
    public void TestLocalizedNameTopocentricAndSpecialized()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.TopoCentric.LocalizedName(), Is.EqualTo("enum.housesystem.topocentric"));
            Assert.That(HouseSystems.Krusinski.LocalizedName(), Is.EqualTo("enum.housesystem.krusinski"));
            Assert.That(HouseSystems.Apc.LocalizedName(), Is.EqualTo("enum.housesystem.apc"));
            Assert.That(HouseSystems.Morin.LocalizedName(), Is.EqualTo("enum.housesystem.morin"));
        }
    }

    [Test]
    public void TestLocalizedNameWholeSignAndEqual()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.WholeSign.LocalizedName(), Is.EqualTo("enum.housesystem.whole_sign"));
            Assert.That(HouseSystems.EqualAsc.LocalizedName(), Is.EqualTo("enum.housesystem.equal_from_ascendant"));
            Assert.That(HouseSystems.EqualMc.LocalizedName(), Is.EqualTo("enum.housesystem.equal_from_mc"));
            Assert.That(HouseSystems.EqualAries.LocalizedName(), Is.EqualTo("enum.housesystem.equal_from_0_aries"));
        }
    }

    [Test]
    public void TestLocalizedNameVehlowAndAxial()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.Vehlow.LocalizedName(), Is.EqualTo("enum.housesystem.vehlow"));
            Assert.That(HouseSystems.Axial.LocalizedName(), Is.EqualTo("enum.housesystem.axial_rotation"));
            Assert.That(HouseSystems.Horizon.LocalizedName(), Is.EqualTo("enum.housesystem.horizon"));
        }
    }

    [Test]
    public void TestLocalizedNameCarterAndGauquelin()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.Carter.LocalizedName(), Is.EqualTo("enum.housesystem.carter"));
            Assert.That(HouseSystems.Gauquelin.LocalizedName(), Is.EqualTo("enum.housesystem.gauquelin"));
        }
    }

    [Test]
    public void TestLocalizedNameSunshine()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.SunShine.LocalizedName(), Is.EqualTo("enum.housesystem.sunshine"));
            Assert.That(HouseSystems.SunShineTreindl.LocalizedName(), Is.EqualTo("enum.housesystem.sunshine_treindl"));
        }
    }

    [Test]
    public void TestLocalizedNamePullen()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.PullenSd.LocalizedName(), Is.EqualTo("enum.housesystem.pullen_sin_diff"));
            Assert.That(HouseSystems.PullenSr.LocalizedName(), Is.EqualTo("enum.housesystem.pullen_sin_ratio"));
        }
    }

    [Test]
    public void TestLocalizedNameSripati()
    {
        Assert.That(HouseSystems.Sripati.LocalizedName(), Is.EqualTo("enum.housesys.sripati"));
    }

    [Test]
    public void TestLocalizedNameAllSystems()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var name = houseSystem.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"House system {houseSystem} has empty localized name");
            Assert.That(name, Does.StartWith("enum.house"), $"House system {houseSystem} localized name does not start with 'enum.house'");
        }
    }

    // MARK: - FromIndex Tests

    [Test]
    public void TestFromIndexValid()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var expectedSystem = allCases[index];
            var system = HouseSystemsExtensions.FromIndex(index);
            Assert.That(system, Is.EqualTo(expectedSystem), $"Index {index} should return {expectedSystem}");
        }
    }

    [Test]
    public void TestFromIndexFirst()
    {
        var system = HouseSystemsExtensions.FromIndex(0);
        var allCases = Enum.GetValues<HouseSystems>();
        Assert.That(system, Is.EqualTo(allCases.First()));
        Assert.That(system, Is.EqualTo(HouseSystems.NoHouses));
    }

    [Test]
    public void TestFromIndexLast()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        var lastIndex = allCases.Length - 1;
        var system = HouseSystemsExtensions.FromIndex(lastIndex);
        Assert.That(system, Is.EqualTo(allCases.Last()));
        Assert.That(system, Is.EqualTo(HouseSystems.Sripati));
    }

    [Test]
    public void TestFromIndexNegative()
    {
        var system = HouseSystemsExtensions.FromIndex(-1);
        Assert.That(system, Is.Null);
    }

    [Test]
    public void TestFromIndexTooLarge()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        var tooLargeIndex = allCases.Length;
        var system = HouseSystemsExtensions.FromIndex(tooLargeIndex);
        Assert.That(system, Is.Null);
    }

    [Test]
    public void TestFromIndexBoundary()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        var boundaryIndex = allCases.Length;
        var system = HouseSystemsExtensions.FromIndex(boundaryIndex);
        Assert.That(system, Is.Null);

        var validBoundaryIndex = allCases.Length - 1;
        var validSystem = HouseSystemsExtensions.FromIndex(validBoundaryIndex);
        Assert.That(validSystem, Is.Not.Null);
        Assert.That(validSystem, Is.EqualTo(HouseSystems.Sripati));
    }

    [Test]
    public void TestFromIndexSpecific()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystemsExtensions.FromIndex(0), Is.EqualTo(HouseSystems.NoHouses));
            Assert.That(HouseSystemsExtensions.FromIndex(1), Is.EqualTo(HouseSystems.Placidus));
            Assert.That(HouseSystemsExtensions.FromIndex(2), Is.EqualTo(HouseSystems.Koch));
            Assert.That(HouseSystemsExtensions.FromIndex(11), Is.EqualTo(HouseSystems.WholeSign));
            Assert.That(HouseSystemsExtensions.FromIndex(24), Is.EqualTo(HouseSystems.Sripati));
        }
    }

    // MARK: - Raw Value Tests

    [Test]
    public void TestRawValuesSequential()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var houseSystem = allCases[index];
            Assert.That((int)houseSystem, Is.EqualTo(index), $"House system {houseSystem} should have raw value {index}");
        }
    }

    [Test]
    public void TestRawValuesMatchExpected()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)HouseSystems.NoHouses, Is.EqualTo(0));
            Assert.That((int)HouseSystems.Placidus, Is.EqualTo(1));
            Assert.That((int)HouseSystems.Koch, Is.EqualTo(2));
            Assert.That((int)HouseSystems.Porphyri, Is.EqualTo(3));
            Assert.That((int)HouseSystems.Regiomontanus, Is.EqualTo(4));
            Assert.That((int)HouseSystems.Campanus, Is.EqualTo(5));
            Assert.That((int)HouseSystems.Alcabitius, Is.EqualTo(6));
            Assert.That((int)HouseSystems.TopoCentric, Is.EqualTo(7));
            Assert.That((int)HouseSystems.Krusinski, Is.EqualTo(8));
            Assert.That((int)HouseSystems.Apc, Is.EqualTo(9));
            Assert.That((int)HouseSystems.Morin, Is.EqualTo(10));
            Assert.That((int)HouseSystems.WholeSign, Is.EqualTo(11));
            Assert.That((int)HouseSystems.EqualAsc, Is.EqualTo(12));
            Assert.That((int)HouseSystems.EqualMc, Is.EqualTo(13));
            Assert.That((int)HouseSystems.EqualAries, Is.EqualTo(14));
            Assert.That((int)HouseSystems.Vehlow, Is.EqualTo(15));
            Assert.That((int)HouseSystems.Axial, Is.EqualTo(16));
            Assert.That((int)HouseSystems.Horizon, Is.EqualTo(17));
            Assert.That((int)HouseSystems.Carter, Is.EqualTo(18));
            Assert.That((int)HouseSystems.Gauquelin, Is.EqualTo(19));
            Assert.That((int)HouseSystems.SunShine, Is.EqualTo(20));
            Assert.That((int)HouseSystems.SunShineTreindl, Is.EqualTo(21));
            Assert.That((int)HouseSystems.PullenSd, Is.EqualTo(22));
            Assert.That((int)HouseSystems.PullenSr, Is.EqualTo(23));
            Assert.That((int)HouseSystems.Sripati, Is.EqualTo(24));
        }
    }

    [Test]
    public void TestRawValuesUnique()
    {
        var rawValues = new HashSet<int>();
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var rawValue = (int)houseSystem;
            Assert.That(rawValues, Does.Not.Contain(rawValue), $"Duplicate raw value {rawValue} found for house system {houseSystem}");
            rawValues.Add(rawValue);
        }
    }

    // MARK: - Comprehensive Tests

    [Test]
    public void TestAllSystemsHaveSeId()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var seId = houseSystem.SeId();
            Assert.That(char.IsAscii(seId), Is.True, $"House system {houseSystem} SE ID should be ASCII");
            // Verify it's a single character (char is always a single character in C#)
        }
    }

    [Test]
    public void TestAllSystemsHaveLocalizedName()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var name = houseSystem.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"House system {houseSystem} has empty localized name");
        }
    }

    [Test]
    public void TestCaseIterable()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        Assert.That(allCases, Has.Length.EqualTo(25));

        // Verify we can iterate
        var count = 0;
        foreach (var _ in allCases)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(25));
    }

    [Test]
    public void TestIntBacked()
    {
        // Test that we can create from raw value
        const HouseSystems noHouses = 0;
        Assert.That(noHouses, Is.EqualTo(HouseSystems.NoHouses));

        const HouseSystems placidus = (HouseSystems)1;
        Assert.That(placidus, Is.EqualTo(HouseSystems.Placidus));

        const HouseSystems sripati = (HouseSystems)24;
        Assert.That(sripati, Is.EqualTo(HouseSystems.Sripati));
    }

    [Test]
    public void TestSeIdValidAscii()
    {
        var allCases = Enum.GetValues<HouseSystems>();
        foreach (var houseSystem in allCases)
        {
            var seId = houseSystem.SeId();
            Assert.That(char.IsAscii(seId), Is.True, $"House system {houseSystem} SE ID should be ASCII");
            // Verify it's a printable ASCII character (32-126)
            var asciiValue = (int)seId;
            Assert.That(asciiValue, Is.GreaterThanOrEqualTo(32).And.LessThanOrEqualTo(126), 
                $"House system {houseSystem} SE ID should be in printable ASCII range");
        }
    }

    [Test]
    public void TestPopularHouseSystems()
    {
        // Test the most commonly used house systems
        var allCases = Enum.GetValues<HouseSystems>();
        Assert.That(allCases, Contains.Item(HouseSystems.Placidus));
        Assert.That(allCases, Contains.Item(HouseSystems.Koch));
        Assert.That(allCases, Contains.Item(HouseSystems.WholeSign));
        Assert.That(allCases, Contains.Item(HouseSystems.EqualAsc));
        Assert.That(allCases, Contains.Item(HouseSystems.Regiomontanus));
        Assert.That(allCases, Contains.Item(HouseSystems.Campanus));
    }

    [Test]
    public void TestEqualHouseSystemsDistinct()
    {
        Assert.That(HouseSystems.EqualAsc, Is.Not.EqualTo(HouseSystems.EqualMc));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.EqualAsc, Is.Not.EqualTo(HouseSystems.EqualAries));
            Assert.That(HouseSystems.EqualMc, Is.Not.EqualTo(HouseSystems.EqualAries));
        }

        Assert.That(HouseSystems.EqualAsc.SeId(), Is.Not.EqualTo(HouseSystems.EqualMc.SeId()));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.EqualAsc.SeId(), Is.Not.EqualTo(HouseSystems.EqualAries.SeId()));
            Assert.That(HouseSystems.EqualMc.SeId(), Is.Not.EqualTo(HouseSystems.EqualAries.SeId()));
        }
    }

    [Test]
    public void TestSunshineSystemsDistinct()
    {
        Assert.That(HouseSystems.SunShine, Is.Not.EqualTo(HouseSystems.SunShineTreindl));
        Assert.That(HouseSystems.SunShine.SeId(), Is.Not.EqualTo(HouseSystems.SunShineTreindl.SeId()));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.SunShine.SeId(), Is.EqualTo('i'));
            Assert.That(HouseSystems.SunShineTreindl.SeId(), Is.EqualTo('I'));
        }
    }

    [Test]
    public void TestPullenSystemsDistinct()
    {
        Assert.That(HouseSystems.PullenSd, Is.Not.EqualTo(HouseSystems.PullenSr));
        Assert.That(HouseSystems.PullenSd.SeId(), Is.Not.EqualTo(HouseSystems.PullenSr.SeId()));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(HouseSystems.PullenSd.SeId(), Is.EqualTo('L'));
            Assert.That(HouseSystems.PullenSr.SeId(), Is.EqualTo('Q'));
        }
    }
}

