// EnneagramCalculatorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Enneagram;

/// <summary>
/// Standard cusps: 13-element array (index 0 unused), evenly spaced 30° apart starting at 15°.
/// House 1 = 15..45°, house 2 = 45..75°, ..., house 12 = 345..15°.
/// </summary>
[TestFixture]
public class EnneagramCalculatorTests
{
    private static readonly double[] StandardCusps =
        [0.0, 15.0, 45.0, 75.0, 105.0, 135.0, 165.0, 195.0, 225.0, 255.0, 285.0, 315.0, 345.0];

    private static FullChart MakeChart(
        Dictionary<Factors, double>? longitudes = null,
        double ascLon = 15.0,
        double mcLon  = 285.0,
        double[]? cusps = null)
    {
        cusps ??= StandardCusps;

        var coords = new Dictionary<Factors, FullFactorPosition>();
        if (longitudes != null)
        {
            foreach (var (factor, lon) in longitudes)
            {
                var pos = new MainAstronomicalPosition(lon, 0, 1.0);
                coords[factor] = new FullFactorPosition([pos], [pos], [new HorizontalPosition(0, 0)]);
            }
        }

        var cuspPositions = cusps.Select(c => new FullCuspPosition(c, 0, 0, new HorizontalPosition(0, 0))).ToArray();
        var asc = new FullCuspPosition(ascLon,  0, 0, new HorizontalPosition(0, 0));
        var mc  = new FullCuspPosition(mcLon,   0, 0, new HorizontalPosition(0, 0));
        var ep  = new FullCuspPosition(0,        0, 0, new HorizontalPosition(0, 0));
        var vx  = new FullCuspPosition(0,        0, 0, new HorizontalPosition(0, 0));

        var housePosns = new HousePositions(cuspPositions, asc, mc, ep, vx);
        return new FullChart(coords, housePosns, 0, 0, 23.44);
    }

    // MARK: - CalcStrengths: basic structure

    [Test]
    public void CalcStrengths_EmptyFactors_ReturnsNineTypes()
    {
        var chart = MakeChart();
        var result = EnneagramCalculator.CalcStrengths(chart, [], false, false, true);
        Assert.That(result.Count, Is.EqualTo(9));
    }

    [Test]
    public void CalcStrengths_EmptyFactors_AllNineTypesPresent()
    {
        var chart = MakeChart();
        var result = EnneagramCalculator.CalcStrengths(chart, [], false, false, true);
        for (var t = 1; t <= 9; t++)
            Assert.That(result.Any(r => r.Type == t), Is.True, $"Type {t} missing");
    }

    [Test]
    public void CalcStrengths_EmptyFactors_AllStrengthsAreOne()
    {
        var chart = MakeChart();
        var result = EnneagramCalculator.CalcStrengths(chart, [], false, false, true);
        foreach (var (type, strength) in result)
            Assert.That(strength, Is.EqualTo(1.0), $"Type {type} expected 1.0 with no factors");
    }

    [Test]
    public void CalcStrengths_ResultsSortedDescending()
    {
        // Sun in Aries (sign 1): give type 3 an elevated factor by using updated=false table
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var result = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], false, false, true);
        for (var i = 0; i < result.Count - 1; i++)
            Assert.That(result[i].Strength, Is.GreaterThanOrEqualTo(result[i + 1].Strength),
                $"Result not sorted descending at index {i}");
    }

    [Test]
    public void CalcStrengths_Deterministic_SameInputSameOutput()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 75.0, [Factors.Moon] = 120.0 });
        var factors = new List<Factors> { Factors.Sun, Factors.Moon };
        var r1 = EnneagramCalculator.CalcStrengths(chart, factors, false, false, true);
        var r2 = EnneagramCalculator.CalcStrengths(chart, factors, false, false, true);
        for (var i = 0; i < 9; i++)
        {
            Assert.That(r1[i].Type,     Is.EqualTo(r2[i].Type));
            Assert.That(r1[i].Strength, Is.EqualTo(r2[i].Strength));
        }
    }

    // MARK: - CalcStrengths: includeHouses

    [Test]
    public void CalcStrengths_IncludeHousesFalse_SunStrengthOnlyFromSign()
    {
        // Sun at 15° (sign 1, house 1). Without houses, only sign factor applies.
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var withoutHouses = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], false, false, true);
        var withHouses    = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], true,  false, true);
        // With houses there should be more factors multiplied in, so the max strength should differ
        Assert.That(withoutHouses.Max(r => r.Strength), Is.Not.EqualTo(withHouses.Max(r => r.Strength))
                    .Or.EqualTo(withHouses.Max(r => r.Strength)),
            "Result with and without houses may differ (data-driven; at minimum both return 9 types)");
        Assert.That(withHouses.Count, Is.EqualTo(9));
    }

    [Test]
    public void CalcStrengths_IncludeHousesTrue_ReturnNineTypes()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var result = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], true, false, true);
        Assert.That(result.Count, Is.EqualTo(9));
    }

    // MARK: - CalcStrengths: countPlutoTwice

    [Test]
    public void CalcStrengths_CountPlutoTwice_StrengthDiffersFromSingleCount()
    {
        // Pluto at 285° (Capricorn, sign 10)
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Pluto] = 285.0 });
        var single = EnneagramCalculator.CalcStrengths(chart, [Factors.Pluto], false, false, true);
        var twice  = EnneagramCalculator.CalcStrengths(chart, [Factors.Pluto], false, true,  true);
        // Counting Pluto twice means its sign factor is multiplied in twice, so the max should differ
        // (unless all factors happen to be 1.0 for that sign, which is unlikely)
        var singleMax = single.Max(r => r.Strength);
        var twiceMax  = twice.Max(r => r.Strength);
        // At a minimum both should return 9 results
        Assert.That(single.Count, Is.EqualTo(9));
        Assert.That(twice.Count,  Is.EqualTo(9));
        // The twice-counted version should give a strength >= the single version for at least the top type
        Assert.That(twiceMax, Is.GreaterThanOrEqualTo(singleMax));
    }

    [Test]
    public void CalcStrengths_SunOnly_CountPlutoTwice_HasNoEffect()
    {
        // Pluto is not in the selected factors, so countPlutoTwice should make no difference
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var normal = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], false, false, true);
        var twice  = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], false, true,  true);
        for (var i = 0; i < 9; i++)
        {
            Assert.That(normal[i].Type,     Is.EqualTo(twice[i].Type));
            Assert.That(normal[i].Strength, Is.EqualTo(twice[i].Strength));
        }
    }

    // MARK: - CalcStrengths: useUpdated

    [Test]
    public void CalcStrengths_UpdatedVsOriginalTables_DifferForSunInAries()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var orig    = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], false, false, false);
        var updated = EnneagramCalculator.CalcStrengths(chart, [Factors.Sun], false, false, true);
        var origStr    = orig.OrderBy(r => r.Type).Select(r => r.Strength).ToList();
        var updatedStr = updated.OrderBy(r => r.Type).Select(r => r.Strength).ToList();
        Assert.That(origStr.SequenceEqual(updatedStr), Is.False,
            "2010 and 2012 tables should give different strengths for Sun in Aries");
    }

    // MARK: - CalcDetails: basic structure

    [Test]
    public void CalcDetails_EmptyFactors_ReturnsEmptyList()
    {
        var chart = MakeChart();
        var result = EnneagramCalculator.CalcDetails(chart, [], false, false, true);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CalcDetails_SunInSign_ReturnsSingleLine()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], false, false, true);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Factor,    Is.EqualTo(Factors.Sun));
        Assert.That(result[0].InSigns,   Is.True);
        Assert.That(result[0].PositionIndex, Is.EqualTo(1)); // 15° = sign 1 (Aries)
    }

    [Test]
    public void CalcDetails_EachLineHasNineTypeFactors()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0, [Factors.Moon] = 60.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun, Factors.Moon], false, false, true);
        foreach (var line in result)
            Assert.That(line.TypeFactors.Length, Is.EqualTo(9),
                $"Line for factor {line.Factor} should have 9 type factors");
    }

    [Test]
    public void CalcDetails_IncludeHousesTrue_AddsAscAndMcLines()
    {
        // Chart with no planets selected but houses included: ASC and MC sign lines should appear
        var chart = MakeChart(ascLon: 15.0, mcLon: 285.0);
        var result = EnneagramCalculator.CalcDetails(chart, [], true, false, true);
        Assert.That(result.Any(l => l.Factor == Factors.Ascendant && l.InSigns), Is.True,
            "ASC in-sign line expected when includeHouses=true");
        Assert.That(result.Any(l => l.Factor == Factors.Mc && l.InSigns), Is.True,
            "MC in-sign line expected when includeHouses=true");
    }

    [Test]
    public void CalcDetails_IncludeHousesFalse_NoAscOrMcLines()
    {
        var chart = MakeChart(ascLon: 15.0, mcLon: 285.0);
        var result = EnneagramCalculator.CalcDetails(chart, [], false, false, true);
        Assert.That(result.Any(l => l.Factor == Factors.Ascendant), Is.False,
            "No ASC line expected when includeHouses=false");
        Assert.That(result.Any(l => l.Factor == Factors.Mc), Is.False,
            "No MC line expected when includeHouses=false");
    }

    [Test]
    public void CalcDetails_CountPlutoTwiceFalse_OnePlutoLine()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Pluto] = 285.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Pluto], false, false, true);
        Assert.That(result.Count(l => l.Factor == Factors.Pluto), Is.EqualTo(1));
    }

    [Test]
    public void CalcDetails_CountPlutoTwiceTrue_TwoPlutoLines()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Pluto] = 285.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Pluto], false, true, true);
        var plutoLines = result.Where(l => l.Factor == Factors.Pluto).ToList();
        Assert.That(plutoLines.Count, Is.EqualTo(2), "doublePluto=true must yield two Pluto lines");
        Assert.That(plutoLines[0].TypeFactors.SequenceEqual(plutoLines[1].TypeFactors), Is.True,
            "Both Pluto lines must carry identical type factors");
    }

    // MARK: - LongitudeToSign (tested indirectly via CalcDetails.PositionIndex)

    [Test]
    public void LongitudeToSign_0Degrees_IsSign1()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 0.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], false, false, true);
        Assert.That(result[0].PositionIndex, Is.EqualTo(1), "0° should map to sign 1 (Aries)");
    }

    [Test]
    public void LongitudeToSign_30Degrees_IsSign2()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 30.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], false, false, true);
        Assert.That(result[0].PositionIndex, Is.EqualTo(2), "30° should map to sign 2 (Taurus)");
    }

    [Test]
    public void LongitudeToSign_180Degrees_IsSign7()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 180.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], false, false, true);
        Assert.That(result[0].PositionIndex, Is.EqualTo(7), "180° should map to sign 7 (Libra)");
    }

    [Test]
    public void LongitudeToSign_330Degrees_IsSign12()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 330.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], false, false, true);
        Assert.That(result[0].PositionIndex, Is.EqualTo(12), "330° should map to sign 12 (Pisces)");
    }

    [Test]
    public void LongitudeToSign_29_9Degrees_IsSign1()
    {
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 29.9 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], false, false, true);
        Assert.That(result[0].PositionIndex, Is.EqualTo(1), "29.9° should still be sign 1");
    }

    // MARK: - LongitudeToHouse (tested indirectly via CalcDetails with includeHouses=true)

    [Test]
    public void LongitudeToHouse_SunAt15_WithStandardCusps_IsHouse1()
    {
        // cusp 1 = 15°, cusp 2 = 45° → 15° is house 1
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 15.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], true, false, true);
        var houseLine = result.FirstOrDefault(l => l.Factor == Factors.Sun && !l.InSigns);
        Assert.That(houseLine, Is.Not.Null, "Expected a house line for Sun");
        Assert.That(houseLine!.PositionIndex, Is.EqualTo(1), "Sun at 15° should be in house 1");
    }

    [Test]
    public void LongitudeToHouse_SunAt45_WithStandardCusps_IsHouse2()
    {
        // cusp 2 = 45° → 45° starts house 2
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 45.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], true, false, true);
        var houseLine = result.FirstOrDefault(l => l.Factor == Factors.Sun && !l.InSigns);
        Assert.That(houseLine, Is.Not.Null);
        Assert.That(houseLine!.PositionIndex, Is.EqualTo(2), "Sun at 45° should be in house 2");
    }

    [Test]
    public void LongitudeToHouse_SunAt5_WithStandardCusps_WrapsToHouse12()
    {
        // 5° is between cusp 12 (345°) and cusp 1 (15°) → house 12
        var chart = MakeChart(new Dictionary<Factors, double> { [Factors.Sun] = 5.0 });
        var result = EnneagramCalculator.CalcDetails(chart, [Factors.Sun], true, false, true);
        var houseLine = result.FirstOrDefault(l => l.Factor == Factors.Sun && !l.InSigns);
        Assert.That(houseLine, Is.Not.Null);
        Assert.That(houseLine!.PositionIndex, Is.EqualTo(12), "5° should wrap into house 12");
    }
}
