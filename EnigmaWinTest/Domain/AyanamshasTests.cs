using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for Ayanamshas domain types.</summary>
[TestFixture]
public class AyanamshasTests
{

    [Test]
    public void TestAllAyanamshasCases()
    {
        // Test that all major cases exist
        var allCases = Enum.GetValues<Ayanamshas>();
        Assert.That(allCases, Contains.Item(Ayanamshas.Tropical));
        Assert.That(allCases, Contains.Item(Ayanamshas.Fagan));
        Assert.That(allCases, Contains.Item(Ayanamshas.Lahiri));
        Assert.That(allCases, Contains.Item(Ayanamshas.Raman));
        Assert.That(allCases, Contains.Item(Ayanamshas.Krishnamurti));
        Assert.That(allCases, Contains.Item(Ayanamshas.GalacticCtrOCap));
    }

    [Test]
    public void TestAllCasesCompleteness()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        Assert.That(allCases, Contains.Item(Ayanamshas.Tropical));
        Assert.That(allCases, Contains.Item(Ayanamshas.Fagan));
        Assert.That(allCases, Contains.Item(Ayanamshas.Lahiri));
        Assert.That(allCases, Contains.Item(Ayanamshas.GalacticCtrOCap));
        Assert.That(allCases, Has.Length.EqualTo(41)); // Total number of ayanamshas
    }

    // MARK: - SE ID Tests

    [Test]
    public void TestSeIdCommonAyanamshas()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Tropical.SeId(), Is.EqualTo(-1));
            Assert.That(Ayanamshas.Fagan.SeId(), Is.EqualTo(0));
            Assert.That(Ayanamshas.Lahiri.SeId(), Is.EqualTo(1));
            Assert.That(Ayanamshas.DeLuce.SeId(), Is.EqualTo(2));
            Assert.That(Ayanamshas.Raman.SeId(), Is.EqualTo(3));
            Assert.That(Ayanamshas.UshaShashi.SeId(), Is.EqualTo(4));
            Assert.That(Ayanamshas.Krishnamurti.SeId(), Is.EqualTo(5));
        }
    }

    [Test]
    public void TestSeIdDjwhalKhulAndYukteshwar()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.DjwhalKhul.SeId(), Is.EqualTo(6));
            Assert.That(Ayanamshas.Yukteshwar.SeId(), Is.EqualTo(7));
            Assert.That(Ayanamshas.Bhasin.SeId(), Is.EqualTo(8));
        }
    }

    [Test]
    public void TestSeIdKuglerVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Kugler1.SeId(), Is.EqualTo(9));
            Assert.That(Ayanamshas.Kugler2.SeId(), Is.EqualTo(10));
            Assert.That(Ayanamshas.Kugler3.SeId(), Is.EqualTo(11));
        }
    }

    [Test]
    public void TestSeIdHuberAndEtaPiscium()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Huber.SeId(), Is.EqualTo(12));
            Assert.That(Ayanamshas.EtaPiscium.SeId(), Is.EqualTo(13));
            Assert.That(Ayanamshas.Aldebaran15Tau.SeId(), Is.EqualTo(14));
        }
    }

    [Test]
    public void TestSeIdHipparchusAndSassanian()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Hipparchus.SeId(), Is.EqualTo(15));
            Assert.That(Ayanamshas.Sassanian.SeId(), Is.EqualTo(16));
            Assert.That(Ayanamshas.GalactCtr0Sag.SeId(), Is.EqualTo(17));
        }
    }

    [Test]
    public void TestSeIdJ2000AndJ1900()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.J2000.SeId(), Is.EqualTo(18));
            Assert.That(Ayanamshas.J1900.SeId(), Is.EqualTo(19));
            Assert.That(Ayanamshas.B1950.SeId(), Is.EqualTo(20));
        }
    }

    [Test]
    public void TestSeIdSuryaSiddhanta()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.SuryaSiddhanta.SeId(), Is.EqualTo(21));
            Assert.That(Ayanamshas.SuryaSiddhantaMeanSun.SeId(), Is.EqualTo(22));
        }
    }

    [Test]
    public void TestSeIdAryabhata()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Aryabhata.SeId(), Is.EqualTo(23));
            Assert.That(Ayanamshas.AryabhataMeanSun.SeId(), Is.EqualTo(24));
        }
    }

    [Test]
    public void TestSeIdSuryaSiddhantaVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.SsRevati.SeId(), Is.EqualTo(25));
            Assert.That(Ayanamshas.SsCitra.SeId(), Is.EqualTo(26));
        }
    }

    [Test]
    public void TestSeIdTrueVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.TrueCitra.SeId(), Is.EqualTo(27));
            Assert.That(Ayanamshas.TrueRevati.SeId(), Is.EqualTo(28));
            Assert.That(Ayanamshas.TruePushya.SeId(), Is.EqualTo(29));
        }
    }

    [Test]
    public void TestSeIdGalacticVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.GalacticCtrBrand.SeId(), Is.EqualTo(30));
            Assert.That(Ayanamshas.GalacticEqIau1958.SeId(), Is.EqualTo(31));
            Assert.That(Ayanamshas.GalacticEq.SeId(), Is.EqualTo(32));
            Assert.That(Ayanamshas.GalacticEqMidMula.SeId(), Is.EqualTo(33));
        }
    }

    [Test]
    public void TestSeIdSkydramAndTrueMula()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Skydram.SeId(), Is.EqualTo(34));
            Assert.That(Ayanamshas.TrueMula.SeId(), Is.EqualTo(35));
            Assert.That(Ayanamshas.Dhruva.SeId(), Is.EqualTo(36));
        }
    }

    [Test]
    public void TestSeIdAryabhata522AndBritton()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Aryabhata522.SeId(), Is.EqualTo(37));
            Assert.That(Ayanamshas.Britton.SeId(), Is.EqualTo(38));
            Assert.That(Ayanamshas.GalacticCtrOCap.SeId(), Is.EqualTo(39));
        }
    }

    [Test]
    public void TestSeIdTropicalIsNegativeOne()
    {
        // Tropical is special - it has SE ID of -1
        Assert.That(Ayanamshas.Tropical.SeId(), Is.EqualTo(-1));
    }

    [Test]
    public void TestSeIdAllAyanamshas()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var seId = ayanamsha.SeId();
            // Verify it's in the valid range for Swiss Ephemeris (-1 to 39)
            Assert.That(seId, Is.GreaterThanOrEqualTo(-1).And.LessThanOrEqualTo(39), 
                $"Ayanamsha {ayanamsha} SE ID should be between -1 and 39");
        }
    }

    // MARK: - LocalizedName Tests

    [Test]
    public void TestLocalizedNameCommon()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Tropical.LocalizedName(), Is.EqualTo("enum.ayanamsha.tropical"));
            Assert.That(Ayanamshas.Fagan.LocalizedName(), Is.EqualTo("enum.ayanamsha.fagan"));
            Assert.That(Ayanamshas.Lahiri.LocalizedName(), Is.EqualTo("enum.ayanamsha.lahiri"));
            Assert.That(Ayanamshas.DeLuce.LocalizedName(), Is.EqualTo("enum.ayanamsha.deluce"));
            Assert.That(Ayanamshas.Raman.LocalizedName(), Is.EqualTo("enum.ayanamsha.raman"));
        }
    }

    [Test]
    public void TestLocalizedNameUshaShashiAndKrishnamurti()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.UshaShashi.LocalizedName(), Is.EqualTo("enum.ayanamsha.ushashashi"));
            Assert.That(Ayanamshas.Krishnamurti.LocalizedName(), Is.EqualTo("enum.ayanamsha.krishnamurti"));
            Assert.That(Ayanamshas.DjwhalKhul.LocalizedName(), Is.EqualTo("enum.ayanamsha.djwhalkhul"));
        }
    }

    [Test]
    public void TestLocalizedNameYukteshwarAndBhasin()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Yukteshwar.LocalizedName(), Is.EqualTo("enum.ayanamsha.yukteshwar"));
            Assert.That(Ayanamshas.Bhasin.LocalizedName(), Is.EqualTo("enum.ayanamsha.bhasin"));
        }
    }

    [Test]
    public void TestLocalizedNameKuglerVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Kugler1.LocalizedName(), Is.EqualTo("enum.ayanamsha.kugler1"));
            Assert.That(Ayanamshas.Kugler2.LocalizedName(), Is.EqualTo("enum.ayanamsha.kugler2"));
            Assert.That(Ayanamshas.Kugler3.LocalizedName(), Is.EqualTo("enum.ayanamsha.kugler3"));
        }
    }

    [Test]
    public void TestLocalizedNameHuberAndEtaPiscium()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Huber.LocalizedName(), Is.EqualTo("enum.ayanamsha.huber"));
            Assert.That(Ayanamshas.EtaPiscium.LocalizedName(), Is.EqualTo("enum.ayanamsha.etapiscium"));
            Assert.That(Ayanamshas.Aldebaran15Tau.LocalizedName(), Is.EqualTo("enum.ayanamsha.aldebaran15tau"));
        }
    }

    [Test]
    public void TestLocalizedNameHipparchusAndSassanian()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Hipparchus.LocalizedName(), Is.EqualTo("enum.ayanamsha.hipparchus"));
            Assert.That(Ayanamshas.Sassanian.LocalizedName(), Is.EqualTo("enum.ayanamsha.sassanian"));
            Assert.That(Ayanamshas.GalactCtr0Sag.LocalizedName(), Is.EqualTo("enum.ayanamsha.galcent0sag"));
        }
    }

    [Test]
    public void TestLocalizedNameJ2000AndJ1900()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.J2000.LocalizedName(), Is.EqualTo("enum.ayanamsha.j2000"));
            Assert.That(Ayanamshas.J1900.LocalizedName(), Is.EqualTo("enum.ayanamsha.j1900"));
            Assert.That(Ayanamshas.B1950.LocalizedName(), Is.EqualTo("enum.ayanamsha.b1950"));
        }
    }

    [Test]
    public void TestLocalizedNameSuryaSiddhanta()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.SuryaSiddhanta.LocalizedName(), Is.EqualTo("enum.ayanamsha.suryasiddhanta"));
            Assert.That(Ayanamshas.SuryaSiddhantaMeanSun.LocalizedName(), Is.EqualTo("enum.ayanamsha.suryasiddhantameansun"));
        }
    }

    [Test]
    public void TestLocalizedNameAryabhata()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Aryabhata.LocalizedName(), Is.EqualTo("enum.ayanamsha.aryabhata"));
            Assert.That(Ayanamshas.AryabhataMeanSun.LocalizedName(), Is.EqualTo("enum.ayanamsha.aryabhatameansun"));
        }
    }

    [Test]
    public void TestLocalizedNameSuryaSiddhantaVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.SsRevati.LocalizedName(), Is.EqualTo("enum.ayanamsha.ssrevati"));
            Assert.That(Ayanamshas.SsCitra.LocalizedName(), Is.EqualTo("enum.ayanamsha.sscitra"));
        }
    }

    [Test]
    public void TestLocalizedNameTrueVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.TrueCitra.LocalizedName(), Is.EqualTo("enum.ayanamsha.truecitrapaksha"));
            Assert.That(Ayanamshas.TrueRevati.LocalizedName(), Is.EqualTo("enum.ayanamsha.truerevati"));
            Assert.That(Ayanamshas.TruePushya.LocalizedName(), Is.EqualTo("enum.ayanamsha.truepushya"));
        }
    }

    [Test]
    public void TestLocalizedNameGalacticVariants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.GalacticCtrBrand.LocalizedName(), Is.EqualTo("enum.ayanamsha.galcentbrand"));
            Assert.That(Ayanamshas.GalacticEqIau1958.LocalizedName(), Is.EqualTo("enum.ayanamsha.galcentiau1958"));
            Assert.That(Ayanamshas.GalacticEq.LocalizedName(), Is.EqualTo("enum.ayanamsha.galequator"));
            Assert.That(Ayanamshas.GalacticEqMidMula.LocalizedName(), Is.EqualTo("enum.ayanamsha.galequatormidmula"));
        }
    }

    [Test]
    public void TestLocalizedNameSkydramAndTrueMula()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Skydram.LocalizedName(), Is.EqualTo("enum.ayanamsha.skydram"));
            Assert.That(Ayanamshas.TrueMula.LocalizedName(), Is.EqualTo("enum.ayanamsha.truemula"));
            Assert.That(Ayanamshas.Dhruva.LocalizedName(), Is.EqualTo("enum.ayanamsha.dhruva"));
        }
    }

    [Test]
    public void TestLocalizedNameAryabhata522AndBritton()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Aryabhata522.LocalizedName(), Is.EqualTo("enum.ayanamsha.aryabhata522"));
            Assert.That(Ayanamshas.Britton.LocalizedName(), Is.EqualTo("enum.ayanamsha.britton"));
            Assert.That(Ayanamshas.GalacticCtrOCap.LocalizedName(), Is.EqualTo("enum.ayanamsha.galcent0cap"));
        }
    }

    [Test]
    public void TestLocalizedNameAllAyanamshas()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var name = ayanamsha.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Ayanamsha {ayanamsha} has empty localized name");
            Assert.That(name, Does.StartWith("enum.ayanamsha"), 
                $"Ayanamsha {ayanamsha} localized name does not start with 'enum.ayanamsha'");
        }
    }

    // MARK: - FromIndex Tests

    [Test]
    public void TestFromIndexValid()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var expectedAyanamsha = allCases[index];
            var ayanamsha = AyanamshasExtensions.FromIndex(index);
            Assert.That(ayanamsha, Is.EqualTo(expectedAyanamsha), 
                $"Index {index} should return {expectedAyanamsha}");
        }
    }

    [Test]
    public void TestFromIndexFirst()
    {
        var ayanamsha = AyanamshasExtensions.FromIndex(0);
        var allCases = Enum.GetValues<Ayanamshas>();
        Assert.That(ayanamsha, Is.EqualTo(allCases.First()));
        Assert.That(ayanamsha, Is.EqualTo(Ayanamshas.Tropical));
    }

    [Test]
    public void TestFromIndexLast()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        var lastIndex = allCases.Length - 1;
        var ayanamsha = AyanamshasExtensions.FromIndex(lastIndex);
        Assert.That(ayanamsha, Is.EqualTo(allCases.Last()));
        Assert.That(ayanamsha, Is.EqualTo(Ayanamshas.GalacticCtrOCap));
    }

    [Test]
    public void TestFromIndexNegative()
    {
        var ayanamsha = AyanamshasExtensions.FromIndex(-1);
        Assert.That(ayanamsha, Is.Null);
    }

    [Test]
    public void TestFromIndexTooLarge()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        var tooLargeIndex = allCases.Length;
        var ayanamsha = AyanamshasExtensions.FromIndex(tooLargeIndex);
        Assert.That(ayanamsha, Is.Null);
    }

    [Test]
    public void TestFromIndexBoundary()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        var boundaryIndex = allCases.Length;
        var ayanamsha = AyanamshasExtensions.FromIndex(boundaryIndex);
        Assert.That(ayanamsha, Is.Null);

        var validBoundaryIndex = allCases.Length - 1;
        var validAyanamsha = AyanamshasExtensions.FromIndex(validBoundaryIndex);
        Assert.That(validAyanamsha, Is.Not.Null);
        Assert.That(validAyanamsha, Is.EqualTo(Ayanamshas.GalacticCtrOCap));
    }

    [Test]
    public void TestFromIndexSpecific()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(AyanamshasExtensions.FromIndex(0), Is.EqualTo(Ayanamshas.Tropical));
            Assert.That(AyanamshasExtensions.FromIndex(1), Is.EqualTo(Ayanamshas.Fagan));
            Assert.That(AyanamshasExtensions.FromIndex(2), Is.EqualTo(Ayanamshas.Lahiri));
            Assert.That(AyanamshasExtensions.FromIndex(6), Is.EqualTo(Ayanamshas.Krishnamurti));
            Assert.That(AyanamshasExtensions.FromIndex(40), Is.EqualTo(Ayanamshas.GalacticCtrOCap));
        }
    }

    // MARK: - Raw Value Tests

    [Test]
    public void TestRawValuesSequential()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var ayanamsha = allCases[index];
            Assert.That((int)ayanamsha, Is.EqualTo(index), 
                $"Ayanamsha {ayanamsha} should have raw value {index}");
        }
    }

    [Test]
    public void TestRawValuesMatchExpected()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)Ayanamshas.Tropical, Is.EqualTo(0));
            Assert.That((int)Ayanamshas.Fagan, Is.EqualTo(1));
            Assert.That((int)Ayanamshas.Lahiri, Is.EqualTo(2));
            Assert.That((int)Ayanamshas.DeLuce, Is.EqualTo(3));
            Assert.That((int)Ayanamshas.Raman, Is.EqualTo(4));
            Assert.That((int)Ayanamshas.UshaShashi, Is.EqualTo(5));
            Assert.That((int)Ayanamshas.Krishnamurti, Is.EqualTo(6));
            Assert.That((int)Ayanamshas.DjwhalKhul, Is.EqualTo(7));
            Assert.That((int)Ayanamshas.Yukteshwar, Is.EqualTo(8));
            Assert.That((int)Ayanamshas.Bhasin, Is.EqualTo(9));
            Assert.That((int)Ayanamshas.Kugler1, Is.EqualTo(10));
            Assert.That((int)Ayanamshas.Kugler2, Is.EqualTo(11));
            Assert.That((int)Ayanamshas.Kugler3, Is.EqualTo(12));
            Assert.That((int)Ayanamshas.Huber, Is.EqualTo(13));
            Assert.That((int)Ayanamshas.EtaPiscium, Is.EqualTo(14));
            Assert.That((int)Ayanamshas.Aldebaran15Tau, Is.EqualTo(15));
            Assert.That((int)Ayanamshas.Hipparchus, Is.EqualTo(16));
            Assert.That((int)Ayanamshas.Sassanian, Is.EqualTo(17));
            Assert.That((int)Ayanamshas.GalactCtr0Sag, Is.EqualTo(18));
            Assert.That((int)Ayanamshas.J2000, Is.EqualTo(19));
            Assert.That((int)Ayanamshas.J1900, Is.EqualTo(20));
            Assert.That((int)Ayanamshas.B1950, Is.EqualTo(21));
            Assert.That((int)Ayanamshas.SuryaSiddhanta, Is.EqualTo(22));
            Assert.That((int)Ayanamshas.SuryaSiddhantaMeanSun, Is.EqualTo(23));
            Assert.That((int)Ayanamshas.Aryabhata, Is.EqualTo(24));
            Assert.That((int)Ayanamshas.AryabhataMeanSun, Is.EqualTo(25));
            Assert.That((int)Ayanamshas.SsRevati, Is.EqualTo(26));
            Assert.That((int)Ayanamshas.SsCitra, Is.EqualTo(27));
            Assert.That((int)Ayanamshas.TrueCitra, Is.EqualTo(28));
            Assert.That((int)Ayanamshas.TrueRevati, Is.EqualTo(29));
            Assert.That((int)Ayanamshas.TruePushya, Is.EqualTo(30));
            Assert.That((int)Ayanamshas.GalacticCtrBrand, Is.EqualTo(31));
            Assert.That((int)Ayanamshas.GalacticEqIau1958, Is.EqualTo(32));
            Assert.That((int)Ayanamshas.GalacticEq, Is.EqualTo(33));
            Assert.That((int)Ayanamshas.GalacticEqMidMula, Is.EqualTo(34));
            Assert.That((int)Ayanamshas.Skydram, Is.EqualTo(35));
            Assert.That((int)Ayanamshas.TrueMula, Is.EqualTo(36));
            Assert.That((int)Ayanamshas.Dhruva, Is.EqualTo(37));
            Assert.That((int)Ayanamshas.Aryabhata522, Is.EqualTo(38));
            Assert.That((int)Ayanamshas.Britton, Is.EqualTo(39));
            Assert.That((int)Ayanamshas.GalacticCtrOCap, Is.EqualTo(40));
        }
    }

    [Test]
    public void TestRawValuesUnique()
    {
        var rawValues = new HashSet<int>();
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var rawValue = (int)ayanamsha;
            Assert.That(rawValues, Does.Not.Contain(rawValue), 
                $"Duplicate raw value {rawValue} found for ayanamsha {ayanamsha}");
            rawValues.Add(rawValue);
        }
    }

    // MARK: - Comprehensive Tests

    [Test]
    public void TestAllAyanamshasHaveSeId()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var seId = ayanamsha.SeId();
            // Verify it's in the valid range for Swiss Ephemeris (-1 to 39)
            Assert.That(seId, Is.GreaterThanOrEqualTo(-1).And.LessThanOrEqualTo(39), 
                $"Ayanamsha {ayanamsha} SE ID should be between -1 and 39");
        }
    }

    [Test]
    public void TestAllAyanamshasHaveLocalizedName()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var name = ayanamsha.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Ayanamsha {ayanamsha} has empty localized name");
        }
    }

    [Test]
    public void TestCaseIterable()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        Assert.That(allCases, Has.Length.EqualTo(41));

        // Verify we can iterate
        var count = 0;
        foreach (var _ in allCases)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(41));
    }

    [Test]
    public void TestIntBacked()
    {
        // Test that we can create from raw value
        const Ayanamshas tropical = 0;
        Assert.That(tropical, Is.EqualTo(Ayanamshas.Tropical));

        const Ayanamshas fagan = (Ayanamshas)1;
        Assert.That(fagan, Is.EqualTo(Ayanamshas.Fagan));

        const Ayanamshas galacticCtrOCap = (Ayanamshas)40;
        Assert.That(galacticCtrOCap, Is.EqualTo(Ayanamshas.GalacticCtrOCap));
    }

    [Test]
    public void TestSeIdValidRange()
    {
        var allCases = Enum.GetValues<Ayanamshas>();
        foreach (var ayanamsha in allCases)
        {
            var seId = ayanamsha.SeId();
            // Verify it's in the valid range for Swiss Ephemeris (-1 to 39)
            Assert.That(seId, Is.GreaterThanOrEqualTo(-1).And.LessThanOrEqualTo(39), 
                $"Ayanamsha {ayanamsha} SE ID should be in valid range");
        }
    }

    [Test]
    public void TestPopularAyanamshas()
    {
        // Test the most commonly used ayanamshas
        var allCases = Enum.GetValues<Ayanamshas>();
        Assert.That(allCases, Contains.Item(Ayanamshas.Fagan));
        Assert.That(allCases, Contains.Item(Ayanamshas.Lahiri));
        Assert.That(allCases, Contains.Item(Ayanamshas.Raman));
        Assert.That(allCases, Contains.Item(Ayanamshas.Krishnamurti));
        Assert.That(allCases, Contains.Item(Ayanamshas.Tropical));
    }

    [Test]
    public void TestKuglerVariantsDistinct()
    {
        Assert.That(Ayanamshas.Kugler1, Is.Not.EqualTo(Ayanamshas.Kugler2));
        Assert.That(Ayanamshas.Kugler1, Is.Not.EqualTo(Ayanamshas.Kugler3));
        Assert.That(Ayanamshas.Kugler2, Is.Not.EqualTo(Ayanamshas.Kugler3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Kugler1.SeId(), Is.Not.EqualTo(Ayanamshas.Kugler2.SeId()));
            Assert.That(Ayanamshas.Kugler1.SeId(), Is.Not.EqualTo(Ayanamshas.Kugler3.SeId()));
            Assert.That(Ayanamshas.Kugler2.SeId(), Is.Not.EqualTo(Ayanamshas.Kugler3.SeId()));
        }
    }

    [Test]
    public void TestAryabhataVariantsDistinct()
    {
        Assert.That(Ayanamshas.Aryabhata, Is.Not.EqualTo(Ayanamshas.AryabhataMeanSun));
        Assert.That(Ayanamshas.Aryabhata, Is.Not.EqualTo(Ayanamshas.Aryabhata522));
        Assert.That(Ayanamshas.AryabhataMeanSun, Is.Not.EqualTo(Ayanamshas.Aryabhata522));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.Aryabhata.SeId(), Is.Not.EqualTo(Ayanamshas.AryabhataMeanSun.SeId()));
            Assert.That(Ayanamshas.Aryabhata.SeId(), Is.Not.EqualTo(Ayanamshas.Aryabhata522.SeId()));
            Assert.That(Ayanamshas.AryabhataMeanSun.SeId(), Is.Not.EqualTo(Ayanamshas.Aryabhata522.SeId()));
        }
    }

    [Test]
    public void TestSuryaSiddhantaVariantsDistinct()
    {
        Assert.That(Ayanamshas.SuryaSiddhanta, Is.Not.EqualTo(Ayanamshas.SuryaSiddhantaMeanSun));
        Assert.That(Ayanamshas.SuryaSiddhanta.SeId(), Is.Not.EqualTo(Ayanamshas.SuryaSiddhantaMeanSun.SeId()));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.SuryaSiddhanta.SeId(), Is.EqualTo(21));
            Assert.That(Ayanamshas.SuryaSiddhantaMeanSun.SeId(), Is.EqualTo(22));
        }
    }

    [Test]
    public void TestTrueVariantsDistinct()
    {
        Assert.That(Ayanamshas.TrueCitra, Is.Not.EqualTo(Ayanamshas.TrueRevati));
        Assert.That(Ayanamshas.TrueCitra, Is.Not.EqualTo(Ayanamshas.TruePushya));
        Assert.That(Ayanamshas.TrueRevati, Is.Not.EqualTo(Ayanamshas.TruePushya));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.TrueCitra.SeId(), Is.Not.EqualTo(Ayanamshas.TrueRevati.SeId()));
            Assert.That(Ayanamshas.TrueCitra.SeId(), Is.Not.EqualTo(Ayanamshas.TruePushya.SeId()));
            Assert.That(Ayanamshas.TrueRevati.SeId(), Is.Not.EqualTo(Ayanamshas.TruePushya.SeId()));
        }
    }

    [Test]
    public void TestGalacticVariantsDistinct()
    {
        Assert.That(Ayanamshas.GalactCtr0Sag, Is.Not.EqualTo(Ayanamshas.GalacticCtrBrand));
        Assert.That(Ayanamshas.GalactCtr0Sag, Is.Not.EqualTo(Ayanamshas.GalacticCtrOCap));
        Assert.That(Ayanamshas.GalacticCtrBrand, Is.Not.EqualTo(Ayanamshas.GalacticCtrOCap));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Ayanamshas.GalactCtr0Sag.SeId(), Is.Not.EqualTo(Ayanamshas.GalacticCtrBrand.SeId()));
            Assert.That(Ayanamshas.GalactCtr0Sag.SeId(), Is.Not.EqualTo(Ayanamshas.GalacticCtrOCap.SeId()));
            Assert.That(Ayanamshas.GalacticCtrBrand.SeId(), Is.Not.EqualTo(Ayanamshas.GalacticCtrOCap.SeId()));
        }
    }
}

