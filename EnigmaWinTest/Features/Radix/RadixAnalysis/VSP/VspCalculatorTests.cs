// VspCalculatorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Linq;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.VSP;

/// <summary>
/// Tests for VspCalculator.Calculate().
/// All tests require the Swiss Ephemeris native DLL; tests are skipped gracefully
/// when the DLL is unavailable (CI without ephemeris data).
/// </summary>
[TestFixture]
public class VspCalculatorTests
{
    // JD for 2022-01-01 00:00 UT
    private const double Jd2022Jan1 = 2_459_580.5;

    private bool _seAvailable;

    [SetUp]
    public void SetUp()
    {
        try
        {
            var ephePath = Environment.GetEnvironmentVariable("EPHE_PATH") ?? Environment.CurrentDirectory;
            SEWrapper.SeInitializer(ephePath);
            _seAvailable = true;
        }
        catch (Exception)
        {
            _seAvailable = false;
        }
    }

    [TearDown]
    public void TearDown()
    {
        try { SEWrapper.CloseEphemeris(); }
        catch (Exception) { /* ignore */ }
    }

    private void RequireSe()
    {
        if (!_seAvailable)
            Assert.Ignore("Swiss Ephemeris DLL not available — skipping SE-backed test.");
    }

    // MARK: - Happy flow: exactly 5 positions with sequential IDs

    [Test]
    public void Calculate_Returns5Positions()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        Assert.That(result.Count, Is.EqualTo(5));
    }

    [Test]
    public void Calculate_SequenceIds_Are1Through5()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        var ids = result.Select(p => p.SequenceId).ToList();
        Assert.That(ids, Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));
    }

    // MARK: - Chronological order

    [Test]
    public void Calculate_JDs_AreInStrictAscendingOrder()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        for (var i = 1; i < result.Count; i++)
            Assert.That(result[i].Jd, Is.GreaterThan(result[i - 1].Jd),
                $"JD at index {i} ({result[i].Jd}) is not greater than index {i - 1} ({result[i - 1].Jd})");
    }

    // MARK: - Longitudes in valid range

    [Test]
    public void Calculate_AllLongitudes_AreInRange0To360()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        foreach (var pos in result)
            Assert.That(pos.Longitude, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0),
                $"Longitude {pos.Longitude} out of range for sequenceId {pos.SequenceId}");
    }

    // MARK: - Both phenomena present

    [Test]
    public void Calculate_ContainsBothInferiorAndSuperiorConjunctions()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        Assert.That(result.Any(p => p.Phenomenon == VenusPhenomena.InferiorConjunction),
            "No inferior conjunction found");
        Assert.That(result.Any(p => p.Phenomenon == VenusPhenomena.SuperiorConjunction),
            "No superior conjunction found");
    }

    // MARK: - Known longitude: Jan 2022 inferior conjunction near 18° Capricorn (~288°)

    [Test]
    public void Calculate_Jan2022_ContainsPositionNear288Degrees()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        var nearCapricorn = result.Any(p => Math.Abs(p.Longitude - 288.0) < 5.0);
        var longitudes = string.Join(", ", result.Select(p => $"{p.Longitude:F1}°"));
        Assert.That(nearCapricorn, Is.True,
            $"Expected a VSP position near 288° (18° Capricorn), got: {longitudes}");
    }

    // MARK: - Consistent results for same input

    [Test]
    public void Calculate_SameInput_ProducesIdenticalResults()
    {
        RequireSe();
        var first  = VspCalculator.Calculate(Jd2022Jan1);
        var second = VspCalculator.Calculate(Jd2022Jan1);

        Assert.That(first.Count, Is.EqualTo(second.Count));
        for (var i = 0; i < first.Count; i++)
        {
            Assert.That(first[i].SequenceId,  Is.EqualTo(second[i].SequenceId),  $"sequenceId mismatch at {i}");
            Assert.That(first[i].Phenomenon,  Is.EqualTo(second[i].Phenomenon),  $"phenomenon mismatch at {i}");
            Assert.That(Math.Abs(first[i].Jd        - second[i].Jd),        Is.LessThan(1e-8), $"JD mismatch at {i}");
            Assert.That(Math.Abs(first[i].Longitude - second[i].Longitude), Is.LessThan(1e-8), $"longitude mismatch at {i}");
        }
    }

    // MARK: - Various birth dates return 5 valid positions

    [Test]
    [TestCase(2_440_000.5, "1968-05-01")]
    [TestCase(2_451_545.0, "2000-01-01 (J2000)")]
    [TestCase(2_459_580.5, "2022-01-01")]
    [TestCase(2_480_000.5, "2050-01-22")]
    public void Calculate_VariousDates_Return5ValidPositions(double birthJd, string label)
    {
        RequireSe();
        var result = VspCalculator.Calculate(birthJd);

        Assert.That(result.Count, Is.EqualTo(5), $"{label}: expected 5 positions");
        foreach (var pos in result)
        {
            Assert.That(pos.Longitude, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0),
                $"{label}: longitude {pos.Longitude} out of range");
            Assert.That(pos.Jd, Is.GreaterThan(0.0),
                $"{label}: non-positive JD {pos.Jd}");
        }
    }

    // MARK: - Head (seq 3) is the last conjunction before birth

    [Test]
    public void Calculate_HeadPosition_IsLastConjunctionBeforeBirth()
    {
        RequireSe();
        var result = VspCalculator.Calculate(Jd2022Jan1);
        var head = result.FirstOrDefault(p => p.SequenceId == 3);

        Assert.That(head, Is.Not.Null, "No position with sequenceId 3 found");
        Assert.That(head!.Jd, Is.LessThan(Jd2022Jan1), "Head (seq 3) should be before birth JD");

        // The position after Head (seq 4) should be after birth
        var afterHead = result.FirstOrDefault(p => p.SequenceId == 4);
        Assert.That(afterHead, Is.Not.Null);
        Assert.That(afterHead!.Jd, Is.GreaterThan(Jd2022Jan1), "Position 4 should be after birth JD");
    }
}
