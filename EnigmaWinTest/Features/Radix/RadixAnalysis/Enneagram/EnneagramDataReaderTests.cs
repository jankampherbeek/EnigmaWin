// EnneagramDataReaderTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram;

namespace EnigmaWintest.Features.Radix.RadixAnalysis.Enneagram;

[TestFixture]
public class EnneagramDataReaderTests
{
    // MARK: - ReadSigns

    [Test]
    public void ReadSigns_Updated_ReturnsNonEmptyList()
    {
        var data = EnneagramDataReader.ReadSigns(useUpdated: true);
        Assert.That(data, Is.Not.Empty);
    }

    [Test]
    public void ReadSigns_Original_ReturnsNonEmptyList()
    {
        var data = EnneagramDataReader.ReadSigns(useUpdated: false);
        Assert.That(data, Is.Not.Empty);
    }

    [Test]
    public void ReadSigns_AllEntriesHaveNineFactors()
    {
        var data = EnneagramDataReader.ReadSigns(useUpdated: true);
        foreach (var entry in data)
            Assert.That(entry.Factors.Length, Is.EqualTo(9),
                $"Entry point={entry.Point} index={entry.Index} should have 9 factors");
    }

    [Test]
    public void ReadSigns_UpdatedAndOriginalDiffer()
    {
        var original = EnneagramDataReader.ReadSigns(useUpdated: false);
        var updated  = EnneagramDataReader.ReadSigns(useUpdated: true);
        Assert.That(original.Count, Is.EqualTo(updated.Count),
            "Both sign tables should have the same number of entries");
        var allSame = true;
        for (var i = 0; i < original.Count; i++)
        {
            if (!original[i].Factors.SequenceEqual(updated[i].Factors)) { allSame = false; break; }
        }
        Assert.That(allSame, Is.False, "2010 and 2012 sign tables should differ in at least one factor");
    }

    [Test]
    public void ReadSigns_SunInAries_KeyExists()
    {
        // Sun point=0, Aries index=1
        var data = EnneagramDataReader.ReadSigns(useUpdated: false);
        Assert.That(data.Any(d => d.Point == 0 && d.Index == 1), Is.True,
            "Sun in Aries (point=0, index=1) must exist in original signs table");
    }

    // MARK: - ReadHouses

    [Test]
    public void ReadHouses_Updated_ReturnsNonEmptyList()
    {
        var data = EnneagramDataReader.ReadHouses(useUpdated: true);
        Assert.That(data, Is.Not.Empty);
    }

    [Test]
    public void ReadHouses_Original_ReturnsNonEmptyList()
    {
        var data = EnneagramDataReader.ReadHouses(useUpdated: false);
        Assert.That(data, Is.Not.Empty);
    }

    [Test]
    public void ReadHouses_AllEntriesHaveNineFactors()
    {
        var data = EnneagramDataReader.ReadHouses(useUpdated: true);
        foreach (var entry in data)
            Assert.That(entry.Factors.Length, Is.EqualTo(9),
                $"Entry point={entry.Point} index={entry.Index} should have 9 factors");
    }

    [Test]
    public void ReadHouses_UpdatedAndOriginalDiffer()
    {
        var original = EnneagramDataReader.ReadHouses(useUpdated: false);
        var updated  = EnneagramDataReader.ReadHouses(useUpdated: true);
        // The two versions differ in either row count or factor values
        var sameCount  = original.Count == updated.Count;
        var sameValues = sameCount && original.Zip(updated).All(p => p.First.Factors.SequenceEqual(p.Second.Factors));
        Assert.That(sameValues, Is.False, "2010 and 2012 house tables should differ");
    }
}
