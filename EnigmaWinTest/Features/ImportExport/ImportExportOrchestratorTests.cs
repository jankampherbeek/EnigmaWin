// ImportExportOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Text;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ImportExport.Aaf;
using EnigmaWin.Sources.Features.ImportExport.Enigma;
using EnigmaWin.Sources.Features.ImportExport.Qck;
using EnigmaWin.Sources.Features.ImportExport.Shared;

namespace EnigmaWintest.Features.ImportExport;

/// <summary>Import/export round trips for the QCK, AAF'97 and Enigma JSON orchestrators, using in-memory repositories.</summary>
[TestFixture]
public class ImportExportOrchestratorTests
{
    private const string QckFixture =
        "JK                     JAN 29, 195308:37:30 AM    -01:00006E54'00 52N13'00 Enschede,Netherlands     ";

    private const string AafFixture = """
        #: Enigma AAF export
        #A93:Kampherbeek,Jan,*,29.1.1953,08:37:30,Enschede,NL
        #B93:*,52n13,6e54,1he,0
        #ZNAM:CET
        """;

    // ── QCK ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task TestQckImport()
    {
        var repo = new InMemoryHoroscopeRepository();
        var result = await new QckImportExportOrchestrator(repo).ImportAsync(Encoding.ASCII.GetBytes(QckFixture));

        Assert.That(result.ChartsImported, Is.EqualTo(1));
        Assert.That(result.FatalMessages, Is.Empty);
        var chart = (await repo.FetchAllAsync()).Single();
        Assert.That(chart.Name, Is.EqualTo("JK"));
        Assert.That(chart.PlaceName, Is.EqualTo("Enschede,Netherlands"));
        Assert.That(chart.DateTimes.Single().TimeZoneIdentifier, Is.EqualTo("UTC"));
        Assert.That(chart.DateTimes.Single().IsPreferred, Is.True);
    }

    [Test]
    public async Task TestQckImportContinuesAfterFatalRecord()
    {
        var content = string.Join("\n", QckFixture, new string('X', 90), QckFixture);
        var result = await new QckImportExportOrchestrator(new InMemoryHoroscopeRepository())
            .ImportAsync(Encoding.ASCII.GetBytes(content));
        Assert.That(result.ChartsImported, Is.EqualTo(2));
        Assert.That(result.FatalMessages.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task TestQckExportImportRoundTrip()
    {
        var source = new InMemoryHoroscopeRepository();
        var orchestrator = new QckImportExportOrchestrator(source);
        await orchestrator.ImportAsync(Encoding.ASCII.GetBytes(QckFixture));

        var export = await orchestrator.ExportAsync();
        Assert.That(export.Messages, Is.Empty);
        Assert.That(export.ChartsExported, Is.EqualTo(1));

        var target = new InMemoryHoroscopeRepository();
        var reimport = await new QckImportExportOrchestrator(target).ImportAsync(export.Data);
        Assert.That(reimport.ChartsImported, Is.EqualTo(1));

        var original = (await source.FetchAllAsync()).Single();
        var copy = (await target.FetchAllAsync()).Single();
        Assert.That(copy.Name, Is.EqualTo("JK"));
        Assert.That(copy.PlaceName, Is.EqualTo("Enschede,Netherlands"));
        Assert.That(copy.DateTimes.Single().JulianDate, Is.EqualTo(original.DateTimes.Single().JulianDate).Within(0.0000001));
    }

    [Test]
    public async Task TestQckExportSkipsChartWithoutCoordinates()
    {
        var repo = new InMemoryHoroscopeRepository();
        var chart = new Horoscope { Name = "No Coordinates" };
        await repo.AddAsync(chart);
        await repo.AddDateTimeAsync(chart.Id, new HoroscopeDateTime { JulianDate = 2451545.0, IsPreferred = true });

        var export = await new QckImportExportOrchestrator(repo).ExportAsync();
        Assert.That(export.Data, Is.Empty);
        Assert.That(export.Messages.Any(m => m.Severity == ExchangeSeverity.Warning && m.Field == "coordinates"), Is.True);
    }

    [Test]
    public async Task TestQckExportIsWindows1252()
    {
        var repo = new InMemoryHoroscopeRepository();
        var chart = new Horoscope { Name = "Müller", PlaceName = "Montréal", Latitude = 45.5, Longitude = -73.6 };
        await repo.AddAsync(chart);
        await repo.AddDateTimeAsync(chart.Id, new HoroscopeDateTime { JulianDate = 2451545.0, IsPreferred = true });

        var export = await new QckImportExportOrchestrator(repo).ExportAsync();
        Assert.That(export.Data, Has.Length.EqualTo(102)); // 100 characters + CRLF, one byte per character
        Assert.That(LegacyTextEncoding.LegacyEncoding.GetString(export.Data), Does.StartWith("Müller"));
    }

    // ── AAF'97 ───────────────────────────────────────────────────────────────

    [Test]
    public async Task TestAafImport()
    {
        var repo = new InMemoryHoroscopeRepository();
        var result = await new AafImportExportOrchestrator(repo).ImportAsync(Encoding.UTF8.GetBytes(AafFixture));
        Assert.That(result.ChartsImported, Is.EqualTo(1));
        Assert.That(result.FatalMessages, Is.Empty);
        var chart = (await repo.FetchAllAsync()).Single();
        Assert.That(chart.Name, Is.EqualTo("Jan Kampherbeek"));
        Assert.That(chart.PlaceName, Is.EqualTo("Enschede,NL"));
    }

    [Test]
    public async Task TestAafExportImportRoundTripKeepsId()
    {
        var source = new InMemoryHoroscopeRepository();
        var orchestrator = new AafImportExportOrchestrator(source);
        await orchestrator.ImportAsync(Encoding.UTF8.GetBytes(AafFixture));

        var export = await orchestrator.ExportAsync();
        Assert.That(export.Messages.All(m => m.Field != "timezone"), Is.True);
        Assert.That(export.Messages.Any(m => m.Field == "place"), Is.True); // "Enschede,NL" has a comma

        var target = new InMemoryHoroscopeRepository();
        var reimport = await new AafImportExportOrchestrator(target).ImportAsync(export.Data);
        Assert.That(reimport.ChartsImported, Is.EqualTo(1));

        var original = (await source.FetchAllAsync()).Single();
        var copy = (await target.FetchAllAsync()).Single();
        Assert.That(copy.Name, Is.EqualTo(original.Name));
        Assert.That(copy.Id, Is.EqualTo(original.Id));
        Assert.That(copy.DateTimes.Single().JulianDate, Is.EqualTo(original.DateTimes.Single().JulianDate).Within(0.0000001));
    }

    [Test]
    public async Task TestAafReimportSameChartIsSkipped()
    {
        var repo = new InMemoryHoroscopeRepository();
        var orchestrator = new AafImportExportOrchestrator(repo);
        await orchestrator.ImportAsync(Encoding.UTF8.GetBytes(AafFixture));

        var export = await orchestrator.ExportAsync();
        var reimport = await orchestrator.ImportAsync(export.Data);
        Assert.That(reimport.ChartsImported, Is.EqualTo(0));
        Assert.That(reimport.ChartsSkipped, Is.EqualTo(1));
        Assert.That(await repo.FetchAllAsync(), Has.Exactly(1).Items);
    }

    [Test]
    public async Task TestAafExportIsUtf8()
    {
        var repo = new InMemoryHoroscopeRepository();
        var chart = new Horoscope { Name = "Müller, André", PlaceName = "Montréal", Latitude = 45.5, Longitude = -73.6 };
        await repo.AddAsync(chart);
        await repo.AddDateTimeAsync(chart.Id, new HoroscopeDateTime { JulianDate = 2451545.0, IsPreferred = true });

        var export = await new AafImportExportOrchestrator(repo).ExportAsync();
        var text = Encoding.UTF8.GetString(export.Data);
        Assert.That(text, Does.Contain("#A93:Müller,André"));
        Assert.That(text, Does.Contain("Montréal"));
    }

    // ── Enigma JSON ──────────────────────────────────────────────────────────

    private static async Task<(InMemoryHoroscopeRepository Horoscopes, InMemoryEventRepository Events, Horoscope Chart)> SeedAsync()
    {
        var horoscopes = new InMemoryHoroscopeRepository();
        var events = new InMemoryEventRepository();
        var chart = new Horoscope
        {
            Name = "Jan", Category = "natal", Notes = "Notes", Source = "Memory", RoddenRating = RoddenRating.AA,
            PlaceName = "Enschede", Latitude = 52.2166, Longitude = 6.9
        };
        await horoscopes.AddAsync(chart);
        await horoscopes.AddDateTimeAsync(chart.Id, new HoroscopeDateTime
        {
            JulianDate = 2434406.8, TimeZoneIdentifier = "Europe/Amsterdam", IsPreferred = true, Label = "Birth"
        });
        await horoscopes.AddDateTimeAsync(chart.Id, new HoroscopeDateTime { JulianDate = 2434406.9, Label = "Rectified" });
        await events.AddAsync(new ChartEvent { Title = "Move", JulianDate = 2440000.5, HoroscopeIds = [chart.Id] });
        return (horoscopes, events, chart);
    }

    [Test]
    public async Task TestEnigmaExportImportRoundTrip()
    {
        var (sourceCharts, sourceEvents, chart) = await SeedAsync();
        var export = await new EnigmaImportExportOrchestrator(sourceCharts, sourceEvents).ExportAsync();
        Assert.That((export.ChartsExported, export.EventsExported), Is.EqualTo((1, 1)));

        var targetCharts = new InMemoryHoroscopeRepository();
        var targetEvents = new InMemoryEventRepository();
        var result = await new EnigmaImportExportOrchestrator(targetCharts, targetEvents).ImportAsync(export.Data);
        Assert.That((result.ChartsImported, result.EventsImported), Is.EqualTo((1, 1)));
        Assert.That(result.FatalMessages, Is.Empty);

        var copy = (await targetCharts.FetchAllAsync()).Single();
        Assert.That((copy.Id, copy.Name, copy.Category, copy.Notes, copy.Source, copy.RoddenRating, copy.PlaceName, copy.Latitude, copy.Longitude),
            Is.EqualTo((chart.Id, chart.Name, chart.Category, chart.Notes, chart.Source, chart.RoddenRating, chart.PlaceName, chart.Latitude, chart.Longitude)));
        Assert.That(copy.DateTimes, Has.Count.EqualTo(2));
        Assert.That(copy.DateTimes.Single(dt => dt.IsPreferred).Label, Is.EqualTo("Birth"));
        Assert.That(copy.DateTimes.Single(dt => dt.IsPreferred).TimeZoneIdentifier, Is.EqualTo("Europe/Amsterdam"));

        var copiedEvent = (await targetEvents.FetchAllAsync()).Single();
        Assert.That(copiedEvent.HoroscopeIds, Is.EqualTo(new[] { chart.Id }));
    }

    [Test]
    public async Task TestEnigmaReimportSkipsExisting()
    {
        var (charts, events, _) = await SeedAsync();
        var orchestrator = new EnigmaImportExportOrchestrator(charts, events);
        var export = await orchestrator.ExportAsync();
        var result = await orchestrator.ImportAsync(export.Data);
        Assert.That((result.ChartsImported, result.EventsImported), Is.EqualTo((0, 0)));
        Assert.That((result.ChartsSkipped, result.EventsSkipped), Is.EqualTo((1, 1)));
    }

    [Test]
    public async Task TestEnigmaEventWithoutChartIsSkippedWithError()
    {
        var json = $$"""
            { "formatVersion": 1, "exportedAt": "2026-09-30T10:15:00Z", "charts": [],
              "events": [ { "id": "{{Guid.NewGuid().ToString().ToUpperInvariant()}}", "title": "Orphan",
                            "julianDate": 2451545.0, "timeZoneIdentifier": "UTC", "chartIds": [] } ] }
            """;
        var result = await new EnigmaImportExportOrchestrator(new InMemoryHoroscopeRepository(), new InMemoryEventRepository())
            .ImportAsync(Encoding.UTF8.GetBytes(json));
        Assert.That(result.EventsImported, Is.EqualTo(0));
        Assert.That(result.FatalMessages.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task TestEnigmaReadsAppleFile()
    {
        // Shape as written by the Apple version: upper-case ids, ISO 8601 date, country/location on events.
        const string json = """
            {
              "charts" : [ {
                "category" : "", "dateTimes" : [ { "id" : "3F2504E0-4F89-11D3-9A0C-0305E82C3301", "isPreferred" : true,
                  "julianDate" : 2434406.8, "timeIsUnknown" : false, "timeZoneIdentifier" : "UTC" } ],
                "id" : "6F9619FF-8B86-D011-B42D-00C04FC964FF", "latitude" : 52.2, "longitude" : 6.9,
                "name" : "Jan", "roddenRating" : "AA"
              } ],
              "events" : [ { "chartIds" : [ "6F9619FF-8B86-D011-B42D-00C04FC964FF" ], "country" : "NL",
                "id" : "7C9E6679-7425-40DE-944B-E07FC1F90AE7", "julianDate" : 2440000.5, "location" : "Enschede",
                "timeZoneIdentifier" : "UTC", "title" : "Move" } ],
              "exportedAt" : "2026-09-30T10:15:00Z",
              "formatVersion" : 1
            }
            """;
        var charts = new InMemoryHoroscopeRepository();
        var result = await new EnigmaImportExportOrchestrator(charts, new InMemoryEventRepository())
            .ImportAsync(Encoding.UTF8.GetBytes(json));
        Assert.That((result.ChartsImported, result.EventsImported), Is.EqualTo((1, 1)));
        Assert.That((await charts.FetchAllAsync()).Single().RoddenRating, Is.EqualTo(RoddenRating.AA));
    }

    [Test]
    public async Task TestEnigmaWritesAppleCompatibleIdsAndDate()
    {
        var (charts, events, chart) = await SeedAsync();
        var text = Encoding.UTF8.GetString((await new EnigmaImportExportOrchestrator(charts, events).ExportAsync()).Data);
        Assert.That(text, Does.Contain(chart.Id.ToString().ToUpperInvariant()));
        Assert.That(text, Does.Match("\"exportedAt\": \"\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}Z\""));
    }

    [Test]
    public async Task TestEnigmaInvalidFileIsFatal()
    {
        var result = await new EnigmaImportExportOrchestrator(new InMemoryHoroscopeRepository(), new InMemoryEventRepository())
            .ImportAsync(Encoding.UTF8.GetBytes("not json"));
        Assert.That(result.FatalMessages.Count(), Is.EqualTo(1));
    }
}
