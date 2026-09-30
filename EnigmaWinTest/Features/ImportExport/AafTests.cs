// AafTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.ImportExport.Aaf;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWintest.Features.ImportExport;

/// <summary>Tests for the AAF'97 field parsers, record parser, mapper and writer.</summary>
[TestFixture]
public class AafTests
{
    private static AafRecord MakeRecord(
        string lastName = "Kampherbeek", string firstName = "Jan", string type = "*",
        bool isGregorian = true, string place = "Enschede", string country = "NL",
        double latitude = 52.2166, double longitude = 6.9, int? greenwichOffsetSeconds = 3600,
        string timeType = "0", Guid? enigmaId = null) => new()
    {
        LastName = lastName, FirstName = firstName, Type = type,
        Day = 29, Month = 1, Year = 1953, IsGregorian = isGregorian,
        Hour = 8, Minute = 37, Second = 30,
        Place = place, Country = country,
        JulianDayRaw = "*", Latitude = latitude, Longitude = longitude,
        GreenwichOffsetSeconds = greenwichOffsetSeconds, TimeType = timeType,
        EnigmaId = enigmaId
    };

    // ── Field parsers ────────────────────────────────────────────────────────

    [TestCase("29.1.1953", 1953, true)]
    [TestCase("1.1.1500", 1500, false)]
    [TestCase("7.10.1582g", 1582, true)]
    [TestCase("7.10.1582j", 1582, false)]
    [TestCase("24.12.-6g", -6, true)]
    public void TestParseDate(string raw, int expectedYear, bool expectedGregorian)
    {
        var messages = new List<ExchangeMessage>();
        var result = AafFieldParsers.ParseDate(raw, 1, messages);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.Year, Is.EqualTo(expectedYear));
        Assert.That(result.Value.IsGregorian, Is.EqualTo(expectedGregorian));
        Assert.That(messages, Is.Empty);
    }

    [Test]
    public void TestParseDateInvalidDayIsFatal()
    {
        var messages = new List<ExchangeMessage>();
        Assert.That(AafFieldParsers.ParseDate("30.2.2000g", 1, messages), Is.Null);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Fatal), Is.True);
    }

    [TestCase("16:55", 16, 55, 0)]
    [TestCase("22:30:12", 22, 30, 12)]
    [TestCase("19", 19, 0, 0)]
    [TestCase("22h30:12", 22, 30, 12)]
    public void TestParseTime(string raw, int hour, int minute, int second)
    {
        var result = AafFieldParsers.ParseTime(raw, 1, []);
        Assert.That(result, Is.EqualTo(new AafFieldParsers.TimeResult(hour, minute, second)));
    }

    [Test]
    public void TestParseCoordinates()
    {
        var messages = new List<ExchangeMessage>();
        var south = AafFieldParsers.ParseCoordinate("15s53:03", 'n', 's', false, 1, "latitude", messages);
        Assert.That(south, Is.EqualTo(-(15.0 + 53.0 / 60.0 + 3.0 / 3600.0)).Within(0.0001));
        Assert.That(AafFieldParsers.ParseCoordinate("52n13", 'n', 's', false, 1, "latitude", messages), Is.GreaterThan(0));
        Assert.That(AafFieldParsers.ParseCoordinate("6e54", 'e', 'w', true, 1, "longitude", messages), Is.GreaterThan(0));
        Assert.That(AafFieldParsers.ParseCoordinate("74w54:45", 'e', 'w', true, 1, "longitude", messages), Is.LessThan(0));
        Assert.That(messages, Is.Empty);
    }

    [TestCase("1he", 3600)]
    [TestCase("5hw", -18000)]
    [TestCase("5he30", 5 * 3600 + 30 * 60)]
    [TestCase("0he32:06", 32 * 60 + 6)]
    public void TestParseGreenwichOffset(string raw, int expectedSeconds)
    {
        var (success, seconds) = AafFieldParsers.ParseGreenwichOffset(raw, 1, []);
        Assert.That(success, Is.True);
        Assert.That(seconds, Is.EqualTo(expectedSeconds));
    }

    [Test]
    public void TestParseGreenwichOffsetUnknown()
    {
        var (success, seconds) = AafFieldParsers.ParseGreenwichOffset("*", 1, []);
        Assert.That(success, Is.True);
        Assert.That(seconds, Is.Null);
    }

    // ── Record parser ────────────────────────────────────────────────────────

    [Test]
    public void TestParseBasicRecord()
    {
        const string content = """
            #: Enigma AAF export
            #A93:Kampherbeek,Jan,*,29.1.1953,08:37:30,Enschede,NL
            #B93:*,52n13,6e54,1he,0
            #ZNAM:CET
            #: AAF end
            """;
        var result = AafRecordParser.Parse(content);
        Assert.That(result.FatalMessages, Is.Empty);
        var record = result.Records.Single();
        Assert.That((record.LastName, record.FirstName), Is.EqualTo(("Kampherbeek", "Jan")));
        Assert.That((record.Day, record.Month, record.Year), Is.EqualTo((29, 1, 1953)));
        Assert.That((record.Hour, record.Minute, record.Second), Is.EqualTo((8, 37, 30)));
        Assert.That((record.Place, record.Country), Is.EqualTo(("Enschede", "NL")));
        Assert.That(record.GreenwichOffsetSeconds, Is.EqualTo(3600));
        Assert.That(record.TimeType, Is.EqualTo("0"));
        Assert.That(record.ZoneName, Is.EqualTo("CET"));
    }

    [Test]
    public void TestUnknownChunkIsWarningNotFatal()
    {
        const string content = "#A93:Doe,Jane,*,1.1.2000,12:00,City,US\n#B93:*,0n00,0e00,0he,0\n#LPOS:some data here";
        var result = AafRecordParser.Parse(content);
        Assert.That(result.Records, Has.Count.EqualTo(1));
        Assert.That(result.FatalMessages, Is.Empty);
        Assert.That(result.WarningMessages.Any(m => m.Args.Contains("LPOS")), Is.True);
    }

    [Test]
    public void TestFreeTextChunksAllowCommasAndUtf8()
    {
        const string content = """
            #A93:Müller,André,*,1.1.2000,12:00,Montréal,CA
            #B93:*,45n30,73w34,5hw,0
            #SRC:Birth certificate, county registry
            #VIA:Astro-Databank, via research
            #COM:Born near Málaga, moved to Osnabrück.
            """;
        var record = AafRecordParser.Parse(content).Records.Single();
        Assert.That((record.LastName, record.FirstName, record.Place), Is.EqualTo(("Müller", "André", "Montréal")));
        Assert.That(record.Source, Is.EqualTo("Birth certificate, county registry"));
        Assert.That(record.Via, Is.EqualTo("Astro-Databank, via research"));
        Assert.That(record.Comment, Is.EqualTo("Born near Málaga, moved to Osnabrück."));
    }

    [Test]
    public void TestFatalRecordDoesNotAbortFile()
    {
        const string content = """
            #A93:Doe,Jane,*,1.1.2000,12:00,City,US
            #B93:*,0n00,0e00,0he,0
            #A93:Broken,Record,*,not-a-date,12:00,City,US
            #B93:*,0n00,0e00,0he,0
            #A93:Roe,Richard,*,2.2.2001,13:00,Town,US
            #B93:*,1n00,1e00,1he,0
            """;
        var result = AafRecordParser.Parse(content);
        Assert.That(result.Records.Select(r => r.LastName), Is.EqualTo(new[] { "Doe", "Roe" }));
        Assert.That(result.FatalMessages.Count(), Is.EqualTo(1));
    }

    [TestCase("#A93:Doe,Jane,*,1.1.2000,12:00,City,US")]
    [TestCase("#A93:Doe,Jane,*,1.1.2000,12:00,City\n#B93:*,0n00,0e00,0he,0")]
    public void TestIncompleteRecordIsFatal(string content)
    {
        var result = AafRecordParser.Parse(content);
        Assert.That(result.Records, Is.Empty);
        Assert.That(result.FatalMessages.Count(), Is.EqualTo(1));
    }

    [Test]
    public void TestEnidParsedAbsentAndInvalid()
    {
        var id = Guid.NewGuid();
        const string record = "#A93:Doe,Jane,*,1.1.2000,12:00,City,US\n#B93:*,0n00,0e00,0he,0";
        Assert.That(AafRecordParser.Parse($"{record}\n#ENID:{id.ToString().ToUpperInvariant()}").Records.Single().EnigmaId,
            Is.EqualTo(id));
        Assert.That(AafRecordParser.Parse(record).Records.Single().EnigmaId, Is.Null);

        var invalid = AafRecordParser.Parse($"{record}\n#ENID:not-a-uuid");
        Assert.That(invalid.Records.Single().EnigmaId, Is.Null);
        Assert.That(invalid.WarningMessages.Any(m => m.Field == "enid"), Is.True);
    }

    // ── Mapper ───────────────────────────────────────────────────────────────

    private static double JulianDate(AafRecord record) => AafMapper.ToMappedChart(record, 1).Chart!.JulianDate;

    [Test]
    public void TestMapperBasicImport()
    {
        var (chart, messages) = AafMapper.ToMappedChart(MakeRecord(), 1);
        Assert.That(messages, Is.Empty);
        Assert.That(chart!.Name, Is.EqualTo("Jan Kampherbeek"));
        Assert.That(chart.PlaceName, Is.EqualTo("Enschede,NL"));
        var localJd = SEWrapper.JulianDay(new AstronomicalDate(1953, 1, 29), new AstronomicalTime(8, 37, 30));
        Assert.That(chart.JulianDate, Is.EqualTo(localJd - 3600.0 / 86400.0).Within(0.0000001));
    }

    [TestCase("1", 3600, 1.0)]
    [TestCase("h", 0, 0.5)]
    public void TestMapperDaylightTimeTypes(string timeType, int offset, double hoursEarlier)
    {
        var withDst = JulianDate(MakeRecord(greenwichOffsetSeconds: offset, timeType: timeType));
        var baseline = JulianDate(MakeRecord(greenwichOffsetSeconds: offset, timeType: "0"));
        Assert.That(baseline - withDst, Is.EqualTo(hoursEarlier / 24.0).Within(0.0000001));
    }

    [Test]
    public void TestMapperLmtUsesLongitude()
    {
        var (chart, messages) = AafMapper.ToMappedChart(MakeRecord(greenwichOffsetSeconds: null, timeType: "l"), 1);
        Assert.That(messages, Is.Empty);
        var localJd = SEWrapper.JulianDay(new AstronomicalDate(1953, 1, 29), new AstronomicalTime(8, 37, 30));
        Assert.That(chart!.JulianDate, Is.EqualTo(localJd - 6.9 / 15.0 * 3600.0 / 86400.0).Within(0.0000001));
    }

    [Test]
    public void TestMapperSpecialMeridianAndHalfHourOffset()
    {
        Assert.That(JulianDate(MakeRecord(greenwichOffsetSeconds: 7200, timeType: "m")),
            Is.EqualTo(JulianDate(MakeRecord(greenwichOffsetSeconds: 7200, timeType: "0"))).Within(0.0000001));
        var utc = JulianDate(MakeRecord(greenwichOffsetSeconds: 0));
        var india = JulianDate(MakeRecord(greenwichOffsetSeconds: 5 * 3600 + 30 * 60));
        Assert.That(utc - india, Is.EqualTo(5.5 / 24.0).Within(0.0000001));
    }

    [Test]
    public void TestMapperUnknownOffsetIsFatal()
    {
        var (chart, messages) = AafMapper.ToMappedChart(MakeRecord(greenwichOffsetSeconds: null, timeType: "0"), 1);
        Assert.That(chart, Is.Null);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Fatal), Is.True);
    }

    [Test]
    public void TestMapperTypeHandling()
    {
        var (female, messages) = AafMapper.ToMappedChart(MakeRecord(type: "f"), 1);
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "type"), Is.True);
        Assert.That(female!.Category, Is.Empty);
        Assert.That(AafMapper.ToMappedChart(MakeRecord(type: "e"), 1).Chart!.Category, Is.EqualTo("event"));
        Assert.That(AafMapper.ToMappedChart(MakeRecord(type: "l"), 1).Chart!.Category, Is.EqualTo("country"));
        Assert.That(AafMapper.ToMappedChart(MakeRecord(type: "o"), 1).Chart!.Category, Is.EqualTo("organisation"));
    }

    [Test]
    public void TestMapperUnknownNamesAndCountry()
    {
        var (chart, _) = AafMapper.ToMappedChart(MakeRecord(lastName: "*", firstName: "*", country: "*"), 1);
        Assert.That(chart!.Name, Is.Empty);
        Assert.That(chart.PlaceName, Is.EqualTo("Enschede"));
    }

    [Test]
    public void TestMapperSourceAndVia()
    {
        var record = MakeRecord() with { Source = "Birth certificate", Via = "Astro-Databank", Comment = "Some comment." };
        var (chart, _) = AafMapper.ToMappedChart(record, 1);
        Assert.That(chart!.Source, Is.EqualTo("Birth certificate"));
        Assert.That(chart.Notes, Is.EqualTo("Some comment.\nVia: Astro-Databank"));
    }

    [Test]
    public void TestMapperEnigmaIdRoundTrip()
    {
        var id = Guid.NewGuid();
        Assert.That(AafMapper.ToMappedChart(MakeRecord(enigmaId: id), 1).Chart!.Id, Is.EqualTo(id));
        Assert.That(AafMapper.ToMappedChart(MakeRecord(), 1).Chart!.Id, Is.Null);
    }

    [Test]
    public void TestMapperExport()
    {
        var id = Guid.NewGuid();
        var (record, messages) = AafMapper.ToRecord(id, "Jan Kampherbeek", "", null, null, "Staten Island, New York",
            0, 0, 2451545.0, "Europe/Amsterdam", 1);
        Assert.That((record.LastName, record.FirstName), Is.EqualTo(("Kampherbeek", "Jan")));
        Assert.That(record.Place, Is.EqualTo("Staten Island (New York)"));
        Assert.That(record.Country, Is.EqualTo("*"));
        Assert.That(record.EnigmaId, Is.EqualTo(id));
        Assert.That(messages.Any(m => m.Field == "timezone"), Is.True);
        Assert.That(messages.Any(m => m.Field == "place"), Is.True);
    }

    // ── Writer ───────────────────────────────────────────────────────────────

    [Test]
    public void TestWriterBasic()
    {
        var (text, messages) = AafWriter.Write(MakeRecord(), 1);
        Assert.That(messages, Is.Empty);
        Assert.That(text, Does.Contain("#A93:Kampherbeek,Jan,*,29.1.1953g,08:37:30,Enschede,NL"));
        Assert.That(text, Does.Contain("#B93:*,52n13,6e54,1he,0"));
        Assert.That(AafWriter.Write(MakeRecord(isGregorian: false), 1).Text, Does.Contain("29.1.1953j"));
    }

    [Test]
    public void TestWriterOptionalChunks()
    {
        var id = Guid.NewGuid();
        var record = MakeRecord(enigmaId: id) with { ZoneName = "CET", Source = "Birth certificate", Comment = "A comment." };
        var (text, _) = AafWriter.Write(record, 1);
        Assert.That(text, Does.Contain("#ZNAM:CET"));
        Assert.That(text, Does.Contain("#SRC:Birth certificate"));
        Assert.That(text, Does.Contain("#COM:A comment."));
        Assert.That(text, Does.Contain($"#ENID:{id.ToString().ToUpperInvariant()}"));
        Assert.That(text, Does.Not.Contain("#VIA:"));
        Assert.That(AafWriter.Write(MakeRecord(), 1).Text, Does.Not.Contain("#ENID"));
    }

    [Test]
    public void TestWriterSanitizesCommaInField()
    {
        var (text, messages) = AafWriter.Write(MakeRecord(place: "Staten Island, USA"), 1);
        Assert.That(text, Does.Contain("Staten Island  USA"));
        Assert.That(messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "place"), Is.True);
    }

    [Test]
    public void TestWriterParserRoundTrip()
    {
        var original = MakeRecord(enigmaId: Guid.NewGuid());
        var (text, writeMessages) = AafWriter.Write(original, 1);
        Assert.That(writeMessages, Is.Empty);
        var result = AafRecordParser.Parse(text);
        Assert.That(result.FatalMessages, Is.Empty);
        var reparsed = result.Records.Single();
        Assert.That((reparsed.LastName, reparsed.FirstName), Is.EqualTo((original.LastName, original.FirstName)));
        Assert.That((reparsed.Day, reparsed.Month, reparsed.Year, reparsed.IsGregorian),
            Is.EqualTo((original.Day, original.Month, original.Year, original.IsGregorian)));
        Assert.That((reparsed.Hour, reparsed.Minute, reparsed.Second), Is.EqualTo((original.Hour, original.Minute, original.Second)));
        Assert.That(reparsed.Latitude, Is.EqualTo(original.Latitude).Within(0.001));
        Assert.That(reparsed.Longitude, Is.EqualTo(original.Longitude).Within(0.001));
        Assert.That(reparsed.GreenwichOffsetSeconds, Is.EqualTo(original.GreenwichOffsetSeconds));
        Assert.That((reparsed.Place, reparsed.Country), Is.EqualTo((original.Place, original.Country)));
        Assert.That(reparsed.EnigmaId, Is.EqualTo(original.EnigmaId));
    }
}
