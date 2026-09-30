// QckTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.ImportExport.Qck;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWintest.Features.ImportExport;

/// <summary>Tests for the QCK line parser, file parser, writer and mapper.</summary>
[TestFixture]
public class QckTests
{
    private const string Fixture =
        "JK                     JAN 29, 195308:37:30 AM    -01:00006E54'00 52N13'00 Enschede,Netherlands     ";

    /// <summary>
    /// Builds a raw 100-character QCK line with full control over every field, including combinations the
    /// writer would never produce (used for negative tests).
    /// </summary>
    private static string RawLine(
        string name = "JK", string month = "JAN", string day = " 29", string year = " 1953",
        string time = "08:37:30 AM ", string tz = "   ", string correction = "-01:00",
        string lonCore = "006E54'00", string latCore = "52N13'00", string place = "Enschede,Netherlands") =>
        name.PadRight(23) + month + day + "," + year + time + tz + correction
        + lonCore.PadRight(9) + " " + latCore.PadRight(8) + " " + place.PadRight(25);

    private static QckRecord SampleRecord(
        string name = "Test", int month = 1, int day = 29, int year = 1953,
        int hour = 8, int minute = 37, int second = 30, string tz = "", int correctionMinutes = -60,
        double longitude = 6.9, double latitude = 52.2166, string place = "Enschede,Netherlands") => new()
    {
        Name = name, Month = month, Day = day, Year = year,
        Hour = hour, Minute = minute, Second = second, IsPm = hour >= 12,
        TimeZoneAbbreviation = tz, QckCorrectionMinutes = correctionMinutes,
        Longitude = longitude, Latitude = latitude, Place = place
    };

    // ── Line parser ──────────────────────────────────────────────────────────

    [Test]
    public void TestRegressionFixture()
    {
        Assert.That(Fixture.Length, Is.EqualTo(100));
        var (record, messages) = QckLineParser.Parse(Fixture, 1);
        Assert.That(record, Is.Not.Null);
        Assert.That(messages, Is.Empty);
        Assert.That((record!.Year, record.Month, record.Day), Is.EqualTo((1953, 1, 29)));
        Assert.That((record.Hour, record.Minute, record.Second), Is.EqualTo((8, 37, 30)));
        Assert.That(record.EffectiveUtcOffsetMinutes, Is.EqualTo(60));
        Assert.That(record.Longitude, Is.EqualTo(6.0 + 54.0 / 60.0).Within(0.0001));
        Assert.That(record.Latitude, Is.EqualTo(52.0 + 13.0 / 60.0).Within(0.0001));
        Assert.That(record.Place, Is.EqualTo("Enschede,Netherlands"));
    }

    [Test]
    public void Test101CharacterRecord()
    {
        var (record, messages) = QckLineParser.Parse(Fixture + " ", 1);
        Assert.That(record?.Place, Is.EqualTo("Enschede,Netherlands"));
        Assert.That(messages, Is.Empty);
    }

    [Test]
    public void TestCetCorrection()
    {
        var (record, _) = QckLineParser.Parse(RawLine(tz: "CET", correction: "-01:00"), 1);
        Assert.That(record?.EffectiveUtcOffsetMinutes, Is.EqualTo(60));
    }

    [Test]
    public void TestPstCorrection()
    {
        var (record, _) = QckLineParser.Parse(RawLine(tz: "PST", correction: "+08:00"), 1);
        Assert.That(record?.EffectiveUtcOffsetMinutes, Is.EqualTo(-480));
    }

    [Test]
    public void TestDstZoneCode()
    {
        var (record, _) = QckLineParser.Parse(RawLine(tz: "EDT", correction: "+04:00"), 1);
        Assert.That(record!.IndicatesDaylightOrWarTime, Is.True);
        Assert.That(record.EffectiveUtcOffsetMinutes, Is.EqualTo(-240));
    }

    [Test]
    public void TestLmt()
    {
        var (record, messages) = QckLineParser.Parse(RawLine(tz: "LMT", correction: "-00:23", lonCore: "005E47'00"), 1);
        Assert.That(record!.IsLmt, Is.True);
        Assert.That(messages, Is.Empty);
    }

    [Test]
    public void TestLatitudeAndLongitudeDirections()
    {
        Assert.That(QckLineParser.Parse(RawLine(latCore: "52N13'00"), 1).Record!.Latitude, Is.GreaterThan(0));
        Assert.That(QckLineParser.Parse(RawLine(latCore: "33S52'00"), 1).Record!.Latitude, Is.LessThan(0));
        Assert.That(QckLineParser.Parse(RawLine(lonCore: "006E54'00"), 1).Record!.Longitude, Is.GreaterThan(0));
        Assert.That(QckLineParser.Parse(RawLine(lonCore: "073W52'00"), 1).Record!.Longitude, Is.LessThan(0));
    }

    [Test]
    public void TestNameAndPlaceAtFullWidth()
    {
        var name = new string('A', 22) + "W";
        var place = new string('X', 24) + "Y";
        var (record, _) = QckLineParser.Parse(RawLine(name: name, place: place), 1);
        Assert.That(record!.Name, Is.EqualTo(name));
        Assert.That(record.Place, Is.EqualTo(place));
    }

    [Test]
    public void TestHistoricalDateWarns()
    {
        var (record, messages) = QckLineParser.Parse(RawLine(month: "MAR", day: "  1", year: "  800"), 1);
        Assert.That(record!.Year, Is.EqualTo(800));
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "date"), Is.True);
    }

    [Test]
    public void TestNegativeYear()
    {
        var (record, _) = QckLineParser.Parse(RawLine(month: "JUN", day: " 15", year: "-0100"), 1);
        Assert.That(record!.Year, Is.EqualTo(-100));
    }

    [Test]
    public void TestInvalidRecordLengthIsFatal()
    {
        var (record, messages) = QckLineParser.Parse(new string('X', 90), 1);
        Assert.That(record, Is.Null);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Fatal), Is.True);
    }

    [TestCase("200E54'00", null, "longitude")]
    [TestCase("006X54'00", null, "longitude")]
    [TestCase(null, "95N13'00", "latitude")]
    public void TestInvalidCoordinatesAreFatal(string? lonCore, string? latCore, string field)
    {
        var line = RawLine(lonCore: lonCore ?? "006E54'00", latCore: latCore ?? "52N13'00");
        var (record, messages) = QckLineParser.Parse(line, 1);
        Assert.That(record, Is.Null);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Fatal && m.Field == field), Is.True);
    }

    [Test]
    public void TestInvalidMonthIsFatal()
    {
        var (record, messages) = QckLineParser.Parse(RawLine(month: "XXX"), 1);
        Assert.That(record, Is.Null);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Fatal && m.Field == "month"), Is.True);
    }

    [Test]
    public void TestHistoricalNoonBugIsTolerated()
    {
        var (record, messages) = QckLineParser.Parse(RawLine(time: "00:00:00 PM "), 1);
        Assert.That((record!.Hour, record.Minute), Is.EqualTo((12, 0)));
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "time"), Is.True);
    }

    // ── File parser ──────────────────────────────────────────────────────────

    [Test]
    public void TestMixedLineEndings()
    {
        var result = QckFileParser.Parse(Fixture + "\r\n" + Fixture + "\n");
        Assert.That(result.Records, Has.Count.EqualTo(2));
        Assert.That(result.FatalMessages, Is.Empty);
    }

    [Test]
    public void TestOneBadRecordDoesNotAbortFile()
    {
        var result = QckFileParser.Parse(string.Join("\n", Fixture, new string('X', 90), Fixture));
        Assert.That(result.Records, Has.Count.EqualTo(2));
        Assert.That(result.FatalMessages.Count(), Is.EqualTo(1));
    }

    [Test]
    public void TestBlankLinesIgnored()
    {
        var result = QckFileParser.Parse(Fixture + "\n\n" + Fixture);
        Assert.That(result.Records, Has.Count.EqualTo(2));
    }

    // ── Writer ───────────────────────────────────────────────────────────────

    [Test]
    public void TestWriterProduces100Characters()
    {
        var (line, _) = QckWriter.Write(SampleRecord(), 1);
        Assert.That(line!.Length, Is.EqualTo(100));
    }

    [TestCase(0, "12:00:00 AM")]
    [TestCase(12, "12:00:00 PM")]
    public void TestWriterUsesNormal12HourNotation(int hour, string expected)
    {
        var (line, _) = QckWriter.Write(SampleRecord(hour: hour, minute: 0, second: 0), 1);
        Assert.That(line, Does.Contain(expected));
    }

    [Test]
    public void TestWriterTruncatesLongNameWithWarning()
    {
        var longName = new string('N', 30);
        var (line, messages) = QckWriter.Write(SampleRecord(name: longName), 1);
        Assert.That(line![..23], Is.EqualTo(longName[..23]));
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "name"), Is.True);
    }

    [Test]
    public void TestWriterTruncatesLongPlaceWithWarning()
    {
        const string longPlace = "Staten Island,United States";
        var (line, messages) = QckWriter.Write(SampleRecord(place: longPlace), 1);
        Assert.That(line![75..].Trim(), Is.EqualTo(longPlace[..25]));
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "place"), Is.True);
    }

    [Test]
    public void TestWriterExactWidthNoWarning()
    {
        var (_, messages) = QckWriter.Write(SampleRecord(name: new string('A', 23), place: new string('B', 25)), 1);
        Assert.That(messages, Is.Empty);
    }

    [Test]
    public void TestWriterYearOutOfRangeIsFatal()
    {
        var (line, messages) = QckWriter.Write(SampleRecord(year: 200_000), 1);
        Assert.That(line, Is.Null);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Fatal), Is.True);
    }

    [Test]
    public void TestWriterParserRoundTrip()
    {
        var original = SampleRecord(tz: "CET", correctionMinutes: -60);
        var (line, writeMessages) = QckWriter.Write(original, 1);
        Assert.That(writeMessages, Is.Empty);
        var (reparsed, parseMessages) = QckLineParser.Parse(line!, 1);
        Assert.That(parseMessages, Is.Empty);
        Assert.That(reparsed!.Name, Is.EqualTo(original.Name));
        Assert.That((reparsed.Year, reparsed.Month, reparsed.Day), Is.EqualTo((original.Year, original.Month, original.Day)));
        Assert.That((reparsed.Hour, reparsed.Minute, reparsed.Second), Is.EqualTo((original.Hour, original.Minute, original.Second)));
        Assert.That(reparsed.Longitude, Is.EqualTo(original.Longitude).Within(0.001));
        Assert.That(reparsed.Latitude, Is.EqualTo(original.Latitude).Within(0.001));
        Assert.That(reparsed.Place, Is.EqualTo(original.Place));
        Assert.That(reparsed.TimeZoneAbbreviation, Is.EqualTo(original.TimeZoneAbbreviation));
        Assert.That(reparsed.EffectiveUtcOffsetMinutes, Is.EqualTo(original.EffectiveUtcOffsetMinutes));
    }

    // ── Mapper ───────────────────────────────────────────────────────────────

    [Test]
    public void TestMapperComputesUtJulianDay()
    {
        var record = SampleRecord(name: "JK", place: "Enschede,Netherlands");
        var (chart, messages) = QckMapper.ToMappedChart(record, 1);
        Assert.That(messages, Is.Empty);
        var localJd = SEWrapper.JulianDay(new AstronomicalDate(1953, 1, 29), new AstronomicalTime(8, 37, 30));
        Assert.That(chart.JulianDate, Is.EqualTo(localJd - 60.0 / 1440.0).Within(0.0000001));
        Assert.That(chart.Name, Is.EqualTo("JK"));
        Assert.That(chart.PlaceName, Is.EqualTo("Enschede,Netherlands"));
    }

    [Test]
    public void TestMapperWarnsOnUnresolvedAbbreviation()
    {
        var (_, messages) = QckMapper.ToMappedChart(SampleRecord(tz: "CET"), 1);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "timezone"), Is.True);
    }

    [TestCase("Europe/Amsterdam", true)]
    [TestCase("UTC", false)]
    public void TestMapperExportTimezoneWarning(string timeZone, bool expectWarning)
    {
        var (_, messages) = QckMapper.ToRecord("N", null, 0, 0, 2451545.0, timeZone, 1);
        Assert.That(messages.Any(m => m.Field == "timezone"), Is.EqualTo(expectWarning));
    }

    [Test]
    public void TestMapperRoundTripPreservesInstant()
    {
        var (imported, _) = QckMapper.ToMappedChart(SampleRecord(), 1);
        var (exported, _) = QckMapper.ToRecord(imported.Name, imported.PlaceName, imported.Latitude,
            imported.Longitude, imported.JulianDate, "UTC", 1);
        var (reimported, _) = QckMapper.ToMappedChart(exported, 1);
        Assert.That(reimported.JulianDate, Is.EqualTo(imported.JulianDate).Within(0.0000001));
    }

    [Test]
    public void TestExportNeverWritesSixtySeconds()
    {
        // 12:00:59.9 UT rounds to 12:01:00, not to 12:00:60.
        var jd = SEWrapper.JulianDay(new AstronomicalDate(2000, 1, 1), new AstronomicalTime(12.0 + 59.9 / 3600.0));
        var (record, _) = QckMapper.ToRecord("N", null, 0, 0, jd, "UTC", 1);
        Assert.That((record.Hour, record.Minute, record.Second), Is.EqualTo((12, 1, 0)));
    }
}
