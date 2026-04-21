// LocationOrchestratorTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Location;

namespace EnigmaWintest.Features.Location;

// ── Country tests ──────────────────────────────────────────────────────────────

[TestFixture]
public class LocationCountryTests
{
    private LocationOrchestrator _orch = null!;

    [SetUp]
    public void SetUp() => _orch = new LocationOrchestrator();

    [TearDown]
    public void TearDown() => _orch.Dispose();

    [Test]
    public void AllCountries_English_Returns252Countries()
    {
        var countries = _orch.AllCountries("en");
        Assert.That(countries.Count, Is.EqualTo(252), $"Expected 252 countries, got {countries.Count}");
    }

    [Test]
    public void AllCountries_English_IsSortedAlphabetically()
    {
        var countries = _orch.AllCountries("en");
        var names = countries.Select(c => c.Name).ToList();
        var sorted = names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.That(names, Is.EqualTo(sorted), "Countries are not in alphabetical order");
    }

    [Test]
    public void AllCountries_English_NetherlandsPresentWithCorrectName()
    {
        var countries = _orch.AllCountries("en");
        var nl = countries.FirstOrDefault(c => c.Code == "NL");
        Assert.That(nl, Is.Not.Null, "NL not found in country list");
        Assert.That(nl!.Name, Is.EqualTo("The Netherlands"),
            $"Expected 'The Netherlands', got '{nl.Name}'");
    }

    [Test]
    public void AllCountries_Dutch_NetherlandsHasDutchName()
    {
        var countries = _orch.AllCountries("nl");
        var nl = countries.FirstOrDefault(c => c.Code == "NL");
        Assert.That(nl, Is.Not.Null, "NL not found in Dutch country list");
        Assert.That(nl!.Name, Is.EqualTo("Nederland"),
            $"Expected 'Nederland', got '{nl.Name}'");
    }

    [Test]
    public void Countries_PatternGer_MatchesGermany()
    {
        var results = _orch.Countries("en", "Ger");
        Assert.That(results.Any(c => c.Code == "DE"), Is.True,
            "Expected Germany in results for 'Ger'");
    }
}

// ── City tests ─────────────────────────────────────────────────────────────────

[TestFixture]
public class LocationCityTests
{
    private LocationOrchestrator _orch = null!;

    [SetUp]
    public void SetUp() => _orch = new LocationOrchestrator();

    [TearDown]
    public void TearDown() => _orch.Dispose();

    [Test]
    public void Cities_NL_NoPattern_Returns7039Cities()
    {
        var cities = _orch.Cities("NL");
        Assert.That(cities.Count, Is.EqualTo(7039), $"Expected 7039 NL cities, got {cities.Count}");
    }

    [Test]
    public void Cities_NL_EnschedeHasAmsterdamTimezone()
    {
        var cities = _orch.Cities("NL");
        var enschede = cities.FirstOrDefault(c => c.Name.StartsWith("Enschede", StringComparison.OrdinalIgnoreCase));
        Assert.That(enschede, Is.Not.Null, "Enschede not found in NL cities");
        Assert.That(enschede!.TimezoneName, Is.EqualTo("Europe/Amsterdam"),
            $"Expected Europe/Amsterdam, got '{enschede.TimezoneName}'");
    }

    [Test]
    public void Cities_NL_WildcardAPrefix_Returns50CitiesAllStartingWithA()
    {
        // Pattern search is capped at 50 results; there are more than 50 NL cities starting with A.
        var cities = _orch.Cities("NL", "A*");
        Assert.That(cities.Count, Is.EqualTo(50), $"Expected 50 (limit cap) NL cities starting with A, got {cities.Count}");
        foreach (var city in cities)
            Assert.That(city.Name.ToUpperInvariant(), Does.StartWith("A"),
                $"City '{city.Name}' does not start with A");
    }

    [Test]
    public void Cities_NL_WildcardContainsDam_AllMatchAndNonEmpty()
    {
        var cities = _orch.Cities("NL", "*dam*");
        Assert.That(cities, Is.Not.Empty, "Expected at least one NL city containing 'dam'");
        foreach (var city in cities)
            Assert.That(city.Name.ToLowerInvariant(), Does.Contain("dam"),
                $"City '{city.Name}' does not contain 'dam'");
    }

    [Test]
    public void Cities_NL_WildcardContainsSter_AllMatchAndNonEmpty()
    {
        var cities = _orch.Cities("NL", "*ster*");
        Assert.That(cities, Is.Not.Empty, "Expected at least one NL city containing 'ster'");
        foreach (var city in cities)
            Assert.That(city.Name.ToLowerInvariant(), Does.Contain("ster"),
                $"City '{city.Name}' does not contain 'ster'");
    }

    [Test]
    public void Cities_NL_WildcardNoMatch_ReturnsEmpty()
    {
        var cities = _orch.Cities("NL", "ZZZZZ*");
        Assert.That(cities, Is.Empty, "Expected empty result for pattern 'ZZZZZ*'");
    }

    [Test]
    public void Cities_NL_AmsterdamHasValidCoordinates()
    {
        var cities = _orch.Cities("NL");
        var amsterdam = cities.FirstOrDefault(c => c.Name.StartsWith("Amsterdam", StringComparison.OrdinalIgnoreCase));
        Assert.That(amsterdam, Is.Not.Null, "Amsterdam not found in NL cities");
        Assert.That(amsterdam!.Latitude, Is.InRange(50.0, 54.0),
            $"Amsterdam latitude {amsterdam.Latitude} out of expected range");
        Assert.That(amsterdam.Longitude, Is.InRange(3.0, 8.0),
            $"Amsterdam longitude {amsterdam.Longitude} out of expected range");
    }

    [Test]
    public void Cities_UnknownCountryCode_ReturnsEmpty()
    {
        var cities = _orch.Cities("XX");
        Assert.That(cities, Is.Empty, "Expected empty result for unknown country code 'XX'");
    }
}

// ── DayDefinitionResolver tests ────────────────────────────────────────────────

[TestFixture]
public class DayDefinitionResolverTests
{
    [Test]
    public void ResolveIana_FixedDay_ReturnsThatDay()
    {
        var result = DayDefinitionResolver.ResolveIana("14", 2025, 3);
        Assert.That(result, Is.EqualTo(14), $"Expected 14, got {result}");
    }

    [Test]
    public void ResolveIana_6Gte1_Feb2025_Returns2_FirstSundayOnOrAfter1()
    {
        // Mon=0..Sun=6: Feb 2025 day 1 = Sat(5), day 2 = Sun(6) → first Sunday on or after 1 is 2
        var result = DayDefinitionResolver.ResolveIana("6>=1", 2025, 2);
        Assert.That(result, Is.EqualTo(2), $"Expected 2, got {result}");
    }

    [Test]
    public void ResolveIana_5Gte2_May2026_Returns2_FirstSaturdayOnOrAfter2()
    {
        // Mon=0..Sun=6: May 2026 day 1 = Fri(4), day 2 = Sat(5) → first Saturday on or after 2 is 2
        var result = DayDefinitionResolver.ResolveIana("5>=2", 2026, 5);
        Assert.That(result, Is.EqualTo(2), $"Expected 2, got {result}");
    }

    [Test]
    public void ResolveIana_5Gte3_May2026_Returns9_FirstSaturdayOnOrAfter3()
    {
        // May 2026: day 3 = Sun(6), next Saturday is day 9
        var result = DayDefinitionResolver.ResolveIana("5>=3", 2026, 5);
        Assert.That(result, Is.EqualTo(9), $"Expected 9, got {result}");
    }

    [Test]
    public void ResolveIana_Last2_Feb2025_Returns26_LastWednesday()
    {
        // Mon=0..Sun=6: Wed=2; last Wednesday in Feb 2025 = 26
        var result = DayDefinitionResolver.ResolveIana("last2", 2025, 2);
        Assert.That(result, Is.EqualTo(26), $"Expected 26, got {result}");
    }

    [Test]
    public void ResolveIana_6Gte23_Oct1981_Returns25_GbEireDstEdgeCase()
    {
        // Mon=0..Sun=6: Oct 1981 day 23 = Fri(4), day 24 = Sat(5), day 25 = Sun(6)
        var result = DayDefinitionResolver.ResolveIana("6>=23", 1981, 10);
        Assert.That(result, Is.EqualTo(25), $"Expected 25, got {result}");
    }

    [Test]
    public void ResolveIana_Last6_March2025_Returns30_LastSunday()
    {
        // Mon=0..Sun=6: last6 = last Sunday; March 2025 last Sunday = 30
        var result = DayDefinitionResolver.ResolveIana("last6", 2025, 3);
        Assert.That(result, Is.EqualTo(30), $"Expected 30 (last Sunday March 2025), got {result}");
    }
}

// ── Timezone resolution tests ──────────────────────────────────────────────────

[TestFixture]
public class TimezoneResolutionTests
{
    private LocationOrchestrator _orch = null!;
    private const double Delta = 1.0 / 3600.0;   // 1-second tolerance expressed as fractional hours

    [SetUp]
    public void SetUp()
    {
        try
        {
            var ephePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "se");
            if (!Directory.Exists(ephePath))
                ephePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "EnigmaWin", "se"));
            EnigmaWin.Sources.Features.AstronCalc.SEWrapper.SeInitializer(ephePath);
        }
        catch (Exception)
        {
            Assert.Ignore("SEWrapper not initialized — Swiss Ephemeris DLL or data may not be available.");
        }
        _orch = new LocationOrchestrator();
    }

    [TearDown]
    public void TearDown()
    {
        _orch.Dispose();
        try { EnigmaWin.Sources.Features.AstronCalc.SEWrapper.CloseEphemeris(); } catch { /* ignored */ }
    }

    private static AstronomicalDateTime Dt(int y, int mo, int d, int h, int mi, int s) =>
        new(new AstronomicalDate(y, mo, d), new AstronomicalTime(h, mi, s));

    private static double OffsetHours(int seconds) => seconds / 3600.0;

    // ── Standard time (no DST) ─────────────────────────────────────────────────

    [Test]
    public void TimezoneInfo_Amsterdam_March2025_StandardCet_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 3, 1, 12, 0, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST on March 1");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_April2025_SummerCest_Offset2()
    {
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 4, 6, 21, 20, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 2.0), Is.LessThan(Delta),
            $"Expected +2h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST active in April");
        Assert.That(zone.TzName, Is.EqualTo("CEST"), $"Expected CEST, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_Jan1953_StandardCet_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(1953, 1, 29, 8, 37, 30));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST in January 1953");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Paris_April1973_StandardCet_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Paris", Dt(1973, 4, 19, 8, 30, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Paris_August1953_StandardCet_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Paris", Dt(1953, 8, 2, 14, 0, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Rome_March1931_StandardCet_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Rome", Dt(1931, 3, 1, 6, 0, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Rome_August1968_SummerCest_Offset2()
    {
        var zone = _orch.TimezoneInfo("Europe/Rome", Dt(1968, 8, 22, 17, 10, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 2.0), Is.LessThan(Delta),
            $"Expected +2h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST active");
        Assert.That(zone.TzName, Is.EqualTo("CEST"), $"Expected CEST, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Oslo_May1947_StandardCet_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Oslo", Dt(1947, 5, 15, 23, 55, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Dublin_May1955_IrishSummerTime_Offset1()
    {
        var zone = _orch.TimezoneInfo("Europe/Dublin", Dt(1955, 5, 23, 19, 0, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST active");
    }

    [Test]
    public void TimezoneInfo_NewYork_Oct1993_Edt_OffsetMinus4()
    {
        var zone = _orch.TimezoneInfo("America/New_York", Dt(1993, 10, 13, 12, 50, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - (-4.0)), Is.LessThan(Delta),
            $"Expected -4h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST active");
        Assert.That(zone.TzName, Is.EqualTo("EDT"), $"Expected EDT, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_NewYork_May1949_Edt_OffsetMinus4()
    {
        var zone = _orch.TimezoneInfo("America/New_York", Dt(1949, 5, 15, 23, 39, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - (-4.0)), Is.LessThan(Delta),
            $"Expected -4h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST active");
        Assert.That(zone.TzName, Is.EqualTo("EDT"), $"Expected EDT, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_NewYork_Sep1997_Edt_OffsetMinus4()
    {
        var zone = _orch.TimezoneInfo("America/New_York", Dt(1997, 9, 12, 9, 0, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - (-4.0)), Is.LessThan(Delta),
            $"Expected -4h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST active");
        Assert.That(zone.TzName, Is.EqualTo("EDT"), $"Expected EDT, got '{zone.TzName}'");
    }

    // ── LMT (historical, pre-standard-time) ───────────────────────────────────

    [Test]
    public void TimezoneInfo_Brussels_June1864_Lmt()
    {
        var zone = _orch.TimezoneInfo("Europe/Brussels", Dt(1864, 6, 12, 7, 0, 0));
        Assert.That(zone.TzName, Is.EqualTo("LMT"), $"Expected LMT, got '{zone.TzName}'");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST for LMT");
    }

    [Test]
    public void TimezoneInfo_Rome_99Bce_Lmt()
    {
        var zone = _orch.TimezoneInfo("Europe/Rome", Dt(-99, 6, 29, 0, 0, 0));
        Assert.That(zone.TzName, Is.EqualTo("LMT"), $"Expected LMT for ancient date, got '{zone.TzName}'");
    }

    // ── DST boundary edge cases (Amsterdam 2025) ───────────────────────────────

    [Test]
    public void TimezoneInfo_Amsterdam_JustBeforeDstStart_Cet_NoDst()
    {
        // Amsterdam clocks spring forward at 02:00 on March 30 2025
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 3, 30, 1, 55, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST just before spring-forward");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_JustAfterDstStart_Cest_DstActive()
    {
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 3, 30, 3, 5, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 2.0), Is.LessThan(Delta),
            $"Expected +2h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST just after spring-forward");
        Assert.That(zone.TzName, Is.EqualTo("CEST"), $"Expected CEST, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_InvalidTimeDuringDstGap_IsInvalidTrue()
    {
        // 02:15 does not exist on March 30 2025 (clocks jump from 02:00 to 03:00)
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 3, 30, 2, 15, 0));
        Assert.That(zone.IsInvalid, Is.True, "Expected IsInvalid=true for time in DST gap");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_JustBeforeDstEnd_Cest_DstActive()
    {
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 10, 26, 1, 55, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 2.0), Is.LessThan(Delta),
            $"Expected +2h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.True, "Expected DST just before fall-back");
        Assert.That(zone.TzName, Is.EqualTo("CEST"), $"Expected CEST, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_JustAfterDstEnd_Cet_NoDst()
    {
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 10, 26, 3, 5, 0));
        Assert.That(Math.Abs(OffsetHours(zone.OffsetSeconds) - 1.0), Is.LessThan(Delta),
            $"Expected +1h, got {OffsetHours(zone.OffsetSeconds)}h");
        Assert.That(zone.DstUsed, Is.False, "Expected no DST just after fall-back");
        Assert.That(zone.TzName, Is.EqualTo("CET"), $"Expected CET, got '{zone.TzName}'");
    }

    [Test]
    public void TimezoneInfo_Amsterdam_AmbiguousTimeDuringDstOverlap_IsAmbiguousTrue()
    {
        // 02:15 occurs twice on October 26 2025 (clocks fall back from 03:00 to 02:00)
        var zone = _orch.TimezoneInfo("Europe/Amsterdam", Dt(2025, 10, 26, 2, 15, 0));
        Assert.That(zone.IsAmbiguous, Is.True, "Expected IsAmbiguous=true for time in DST overlap");
    }
}
