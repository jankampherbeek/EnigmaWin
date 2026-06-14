// PresentableVspPositionTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.VSP;

/// <summary>
/// Tests for PresentableVspPosition: name assignment, sign index, longitude formatting,
/// and phenomenon text. These tests require the Swiss Ephemeris DLL only for the
/// DateFromJulianDay call inside the constructor; they are skipped gracefully when
/// the DLL is unavailable.
/// </summary>
[TestFixture]
public class PresentableVspPositionTests
{
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

    private static VspPosition MakePosition(int seq, double longitude,
        VenusPhenomena phen = VenusPhenomena.InferiorConjunction,
        double jd = 2_459_580.5)
        => new(seq, jd, phen, longitude);

    // MARK: - Name assignment

    [Test]
    [TestCase(1, "Leg")]
    [TestCase(2, "Arm")]
    [TestCase(3, "Head")]
    [TestCase(4, "Arm")]
    [TestCase(5, "Leg")]
    public void Name_CorrectForEachSequenceId(int seq, string expected)
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(seq, 0.0));
        Assert.That(pos.Name, Is.EqualTo(expected));
    }

    // MARK: - Phenomenon text

    [Test]
    public void PhenomenonText_Inferior_ReturnsInferior()
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(1, 0.0, VenusPhenomena.InferiorConjunction));
        Assert.That(pos.PhenomenonText, Is.EqualTo("Inferior"));
    }

    [Test]
    public void PhenomenonText_Superior_ReturnsSuperior()
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(1, 0.0, VenusPhenomena.SuperiorConjunction));
        Assert.That(pos.PhenomenonText, Is.EqualTo("Superior"));
    }

    // MARK: - Longitude formatting (degrees within sign)

    [Test]
    public void LongitudeText_ExactSignBoundary_ShowsZeroDegrees()
    {
        RequireSe();
        // 0° = start of Aries → 0°00'00"
        var pos = new PresentableVspPosition(MakePosition(1, 0.0));
        Assert.That(pos.LongitudeText, Is.EqualTo("0°00'00\""));
    }

    [Test]
    public void LongitudeText_At15Degrees_Shows15DegreesTaurus()
    {
        RequireSe();
        // 45° = 15° Taurus → 15°00'00"
        var pos = new PresentableVspPosition(MakePosition(1, 45.0));
        Assert.That(pos.LongitudeText, Is.EqualTo("15°00'00\""));
    }

    [Test]
    public void LongitudeText_Degrees_Minutes_Seconds_FormattedCorrectly()
    {
        RequireSe();
        // 1.5° = 1°30'00" within sign (longitude 1.5 = 1°30' Aries)
        var pos = new PresentableVspPosition(MakePosition(1, 1.5));
        Assert.That(pos.LongitudeText, Is.EqualTo("1°30'00\""));
    }

    // MARK: - Sign glyph is non-empty

    [Test]
    [TestCase(0.0,   "Aries")]
    [TestCase(30.0,  "Taurus")]
    [TestCase(60.0,  "Gemini")]
    [TestCase(90.0,  "Cancer")]
    [TestCase(180.0, "Libra")]
    [TestCase(270.0, "Capricorn")]
    [TestCase(359.9, "Pisces")]
    public void SignGlyph_NonEmpty_ForAllSigns(double longitude, string signName)
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(1, longitude));
        Assert.That(pos.SignGlyph, Is.Not.Null.And.Not.Empty,
            $"SignGlyph should be non-empty for {signName} (longitude {longitude})");
    }

    // MARK: - IsEvenRow

    [Test]
    [TestCase(1, false)]
    [TestCase(2, true)]
    [TestCase(3, false)]
    [TestCase(4, true)]
    [TestCase(5, false)]
    public void IsEvenRow_CorrectForEachSequenceId(int seq, bool expected)
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(seq, 0.0));
        Assert.That(pos.IsEvenRow, Is.EqualTo(expected));
    }

    // MARK: - DateTimeText is non-empty and formatted

    [Test]
    public void DateTimeText_IsNonEmpty()
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(1, 0.0, VenusPhenomena.InferiorConjunction, 2_459_580.5));
        Assert.That(pos.DateTimeText, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void DateTimeText_ContainsExpectedYear()
    {
        RequireSe();
        // JD 2459580.5 = 2022-01-01
        var pos = new PresentableVspPosition(MakePosition(1, 0.0, VenusPhenomena.InferiorConjunction, 2_459_580.5));
        Assert.That(pos.DateTimeText, Does.Contain("2022"));
    }

    // MARK: - SequenceId and Longitude are preserved

    [Test]
    public void SequenceId_IsPreservedFromInput()
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(3, 45.0));
        Assert.That(pos.SequenceId, Is.EqualTo(3));
    }

    [Test]
    public void Longitude_IsPreservedFromInput()
    {
        RequireSe();
        var pos = new PresentableVspPosition(MakePosition(1, 123.456));
        Assert.That(pos.Longitude, Is.EqualTo(123.456).Within(1e-10));
    }
}
