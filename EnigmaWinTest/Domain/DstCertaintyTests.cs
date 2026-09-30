// DstCertaintyTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for the check on DST that cannot be determined reliably.</summary>
[TestFixture]
public class DstCertaintyTests
{
    [TestCase("US", 1966, true)]
    [TestCase("US", 1900, true)]
    [TestCase("US", -500, true)]
    [TestCase("US", 1967, false)]
    [TestCase("US", 2000, false)]
    [TestCase("NL", 1950, false)]
    [TestCase("CA", 1950, false)]
    [TestCase(null, 1950, false)]
    public void TestIsUncertain(string? countryCode, int year, bool expected)
    {
        Assert.That(DstCertainty.IsUncertain(countryCode, year), Is.EqualTo(expected));
    }
}
