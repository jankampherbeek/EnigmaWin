// EventEditViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Location;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.Events.UI;

public partial class EventEditViewModel : ObservableObject
{
    private readonly EventsOrchestrator _orchestrator;
    private readonly INavigationService _navigationService;
    private readonly IRosetta _rosetta;
    private readonly ChartEvent _originalEvent;

    private readonly LocationOrchestrator _locationOrchestrator = new();
    private CancellationTokenSource? _countrySearchCts;
    private CancellationTokenSource? _citySearchCts;
    private bool _suppressCountrySearch;
    private bool _suppressCitySearch;

    // ── Event fields ──────────────────────────────────────────────────────────
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanSave))] private string _eventTitle = string.Empty;
    [ObservableProperty] private string _eventDescription = string.Empty;

    // ── Location fields ───────────────────────────────────────────────────────
    [ObservableProperty] private string _countryQuery = string.Empty;
    [ObservableProperty] private string _cityQuery = string.Empty;
    [ObservableProperty] private IReadOnlyList<LocationCountry> _filteredCountries = [];
    [ObservableProperty] private IReadOnlyList<LocationCity> _filteredCities = [];
    [ObservableProperty] private LocationCountry? _selectedCountry;
    [ObservableProperty] private LocationCity? _selectedCity;
    [ObservableProperty] private bool _countryDropdownVisible;
    [ObservableProperty] private bool _cityDropdownVisible;
    [ObservableProperty] private string _locationName = string.Empty;
    [ObservableProperty] private int _latitudeDegree;
    [ObservableProperty] private int _latitudeMinute;
    [ObservableProperty] private int _latitudeSecond;
    [ObservableProperty] private DisplayItem<LatitudeHemisphere> _latitudeDirection = null!;
    [ObservableProperty] private int _longitudeDegree;
    [ObservableProperty] private int _longitudeMinute;
    [ObservableProperty] private int _longitudeSecond;
    [ObservableProperty] private DisplayItem<LongitudeHemisphere> _longitudeDirection = null!;

    // ── Date/Time fields ──────────────────────────────────────────────────────
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanSave))] private string _year = string.Empty;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanSave))] private int _month = 1;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanSave))] private int _day = 1;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanSave))] private DisplayItem<CalendarStyle> _calendar = null!;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanSave))] private DisplayItem<YearCount> _yearCount = null!;
    [ObservableProperty] private int _hour;
    [ObservableProperty] private int _minute;
    [ObservableProperty] private int _second;
    [ObservableProperty] private int _offsetHour;
    [ObservableProperty] private int _offsetMinute;
    [ObservableProperty] private DisplayItem<DSTOption> _dst = null!;
    [ObservableProperty] private DisplayItem<UTOffsetDirection> _offsetDirection = null!;

    // ── Validation / error ────────────────────────────────────────────────────
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasError))] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _titleSubmitted;
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    // ── Dropdown data ─────────────────────────────────────────────────────────
    public IReadOnlyList<int> HourValues { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues { get; } = Enumerable.Range(1, 31).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 91).ToList();
    public IReadOnlyList<int> LongitudeDegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<DisplayItem<LatitudeHemisphere>> LatitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<LongitudeHemisphere>> LongitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<CalendarStyle>> CalendarValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<YearCount>> YearCountValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<DSTOption>> DstValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<UTOffsetDirection>> OffsetDirectionValues { get; private set; } = [];

    public bool CanSave => !string.IsNullOrWhiteSpace(EventTitle) && IsDateValid();

    // ── Labels ────────────────────────────────────────────────────────────────
    public string LabelTitle             => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.title");
    public string LabelEventTitle        => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.eventtitle");
    public string LabelEventDescription  => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.eventdescription");
    public string LabelLocation          => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.location");
    public string LabelDateTime          => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.datetime");
    public string LabelSave              => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.save");
    public string LabelCancel            => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.cancel");
    public string LabelValidationTitle   => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.validation.titleempty");
    public string LabelValidationYear    => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.validation.invalidyear");
    public string LabelCountry           => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.country");
    public string LabelNameOfLocation    => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.nameoflocation");
    public string LabelLongitude         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.longitude");
    public string LabelLatitude          => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.latitude");
    public string LabelDate              => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.date");
    public string LabelCalendarYearCount => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.calendaryearcount");
    public string LabelTimeDst           => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.timedst");
    public string LabelOffsetUt          => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.offsetut");
    public string LabelHelp              => _rosetta.GetText(RbFile.EventEdit, "view.eventeditscreen.help");

    public EventEditViewModel(
        EventsOrchestrator orchestrator,
        INavigationService navigationService,
        IRosetta rosetta,
        ChartEvent chartEvent)
    {
        _orchestrator      = orchestrator;
        _navigationService = navigationService;
        _rosetta           = rosetta;
        _originalEvent     = chartEvent;

        InitEnumLists();
        PopulateFromEvent(chartEvent);
    }

    partial void OnCountryQueryChanged(string value)
    {
        if (_suppressCountrySearch) return;
        _countrySearchCts?.Cancel();
        _countrySearchCts = new CancellationTokenSource();
        var token = _countrySearchCts.Token;
        var q = value.Trim();
        if (string.IsNullOrEmpty(q)) { FilteredCountries = []; CountryDropdownVisible = false; return; }
        _ = Task.Run(async () =>
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            var results = _locationOrchestrator.Countries(_rosetta.GetLanguage(), q);
            Application.Current.Dispatcher.Invoke(() => { FilteredCountries = results; CountryDropdownVisible = results.Count > 0; });
        }, token);
    }

    partial void OnCityQueryChanged(string value)
    {
        if (_suppressCitySearch) return;
        _citySearchCts?.Cancel();
        _citySearchCts = new CancellationTokenSource();
        var token = _citySearchCts.Token;
        var q = value.Trim();
        if (SelectedCountry is null || string.IsNullOrEmpty(q)) { FilteredCities = []; CityDropdownVisible = false; return; }
        var code = SelectedCountry.Code;
        _ = Task.Run(async () =>
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            var results = _locationOrchestrator.Cities(code, q);
            Application.Current.Dispatcher.Invoke(() => { FilteredCities = results; CityDropdownVisible = results.Count > 0; });
        }, token);
    }

    partial void OnYearChanged(string value)  { OnPropertyChanged(nameof(CanSave)); RecalculateOffset(); }
    partial void OnMonthChanged(int value)    { OnPropertyChanged(nameof(CanSave)); RecalculateOffset(); }
    partial void OnDayChanged(int value)      { OnPropertyChanged(nameof(CanSave)); RecalculateOffset(); }

    public void SelectCountry(LocationCountry country)
    {
        _suppressCountrySearch = _suppressCitySearch = true;
        SelectedCountry = country; CountryQuery = country.Name;
        FilteredCountries = []; CountryDropdownVisible = false;
        CityQuery = string.Empty; SelectedCity = null;
        FilteredCities = []; CityDropdownVisible = false;
        _suppressCountrySearch = _suppressCitySearch = false;
    }

    public void SelectCity(LocationCity city)
    {
        _suppressCitySearch = true;
        SelectedCity = city; CityQuery = city.Name; LocationName = city.Name;
        FilteredCities = []; CityDropdownVisible = false;
        ApplyCity(city);
        _suppressCitySearch = false;
    }

    private void ApplyCity(LocationCity city)
    {
        var (latDeg, latMin, latSec, latNeg) = DecimalToDms(city.Latitude);
        LatitudeDegree    = latDeg; LatitudeMinute = latMin; LatitudeSecond = latSec;
        LatitudeDirection = LatitudeDirectionValues.First(v => v.Value == (latNeg ? LatitudeHemisphere.South : LatitudeHemisphere.North));
        var (lonDeg, lonMin, lonSec, lonNeg) = DecimalToDms(city.Longitude);
        LongitudeDegree    = lonDeg; LongitudeMinute = lonMin; LongitudeSecond = lonSec;
        LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == (lonNeg ? LongitudeHemisphere.West : LongitudeHemisphere.East));
        var refDate = new AstronomicalDate(DateTime.Now.Year, 1, 1);
        using var tmpOrch = new LocationOrchestrator();
        var zone = tmpOrch.TimezoneInfo(city.TimezoneName, new AstronomicalDateTime(refDate, new AstronomicalTime(12, 0, 0)), city.Longitude);
        ApplyZone(zone);
    }

    internal void RecalculateOffset()
    {
        if (SelectedCity is null) return;
        if (!TryGetAstronomicalDate(out var date)) return;
        var zone = _locationOrchestrator.TimezoneInfo(SelectedCity.TimezoneName,
            new AstronomicalDateTime(date, new AstronomicalTime(Hour, Minute, Second)), SelectedCity.Longitude);
        ApplyZone(zone);
    }

    private void ApplyZone(ZoneInfo zone)
    {
        var totalSec = Math.Abs(zone.OffsetSeconds);
        OffsetHour   = totalSec / 3600;
        OffsetMinute = (totalSec % 3600) / 60;
        OffsetDirection = OffsetDirectionValues.First(v => v.Value == (zone.OffsetSeconds >= 0 ? UTOffsetDirection.Later : UTOffsetDirection.Earlier));
        Dst = DstValues.First(v => v.Value == (zone.DstUsed ? DSTOption.DST : DSTOption.NoDST));
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        TitleSubmitted = true;
        if (!CanSave) return;
        if (!TryGetAstronomicalDate(out var date)) return;

        var jd = ComputeJulianDay(date);
        var hasLocation = !string.IsNullOrWhiteSpace(LocationName);
        double? lat = hasLocation ? DmsToDecimal(LatitudeDegree, LatitudeMinute, LatitudeSecond, LatitudeDirection.Value == LatitudeHemisphere.South) : null;
        double? lon = hasLocation ? DmsToDecimal(LongitudeDegree, LongitudeMinute, LongitudeSecond, LongitudeDirection.Value == LongitudeHemisphere.West) : null;

        var values = new EventValues(
            Title: EventTitle.Trim(),
            JulianDate: jd,
            TimeZoneIdentifier: BuildOffsetString(),
            EventDescription: string.IsNullOrWhiteSpace(EventDescription) ? null : EventDescription,
            OriginalInput: BuildOriginalInput(date),
            PlaceName: hasLocation ? LocationName : null,
            Latitude: lat,
            Longitude: lon);

        try
        {
            await _orchestrator.UpdateValuesAsync(_originalEvent, values);
            _navigationService.GoBackDetail();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void Cancel() => _navigationService.GoBackDetail();

    private void PopulateFromEvent(ChartEvent e)
    {
        EventTitle       = e.Title;
        EventDescription = e.EventDescription ?? string.Empty;
        LocationName     = e.PlaceName ?? string.Empty;

        // Reconstruct local date/time from stored UTC offset string
        var (dir, offH, offM) = ParseTz(e.TimeZoneIdentifier);
        var offsetFraction = (double)(offH * 3600 + offM * 60) / 86_400.0;
        var localJd = dir == UTOffsetDirection.Later
            ? e.JulianDate + offsetFraction
            : e.JulianDate - offsetFraction;
        var localDt = SEWrapper.DateFromJulianDay(localJd, gregorian: true);

        var astroY = localDt.Date.Year;
        YearCount = astroY > 0
            ? YearCountValues.First(v => v.Value == Domain.YearCount.CE)
            : YearCountValues.First(v => v.Value == Domain.YearCount.BCE);
        Year   = astroY > 0 ? astroY.ToString() : (1 - astroY).ToString();
        Month  = localDt.Date.Month;
        Day    = localDt.Date.Day;
        Hour   = localDt.Time.Hour;
        Minute = localDt.Time.Minute;
        Second = localDt.Time.Second;
        Calendar = CalendarValues.First(v => v.Value == CalendarStyle.Gregorian);
        OffsetHour = offH; OffsetMinute = offM;
        OffsetDirection = OffsetDirectionValues.First(v => v.Value == dir);
        Dst = DstValues.First(v => v.Value == DSTOption.NoDST);

        if (e.Latitude.HasValue)
        {
            var (latDeg, latMin, latSec, latNeg) = DecimalToDms(e.Latitude.Value);
            LatitudeDegree = latDeg; LatitudeMinute = latMin; LatitudeSecond = latSec;
            LatitudeDirection = LatitudeDirectionValues.First(v => v.Value == (latNeg ? LatitudeHemisphere.South : LatitudeHemisphere.North));
        }
        else LatitudeDirection = LatitudeDirectionValues.First(v => v.Value == LatitudeHemisphere.North);

        if (e.Longitude.HasValue)
        {
            var (lonDeg, lonMin, lonSec, lonNeg) = DecimalToDms(e.Longitude.Value);
            LongitudeDegree = lonDeg; LongitudeMinute = lonMin; LongitudeSecond = lonSec;
            LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == (lonNeg ? LongitudeHemisphere.West : LongitudeHemisphere.East));
        }
        else LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == LongitudeHemisphere.East);
    }

    private bool IsDateValid()
    {
        if (!int.TryParse(Year, out var y)) return false;
        if (!TryConvertToAstronomicalYear(y, out var astroY)) return false;
        var date = new AstronomicalDate(astroY, Month, Day, Calendar?.Value == CalendarStyle.Gregorian);
        return IsValidCalendarDate(date);
    }

    private bool TryGetAstronomicalDate(out AstronomicalDate date)
    {
        if (!int.TryParse(Year, out var y) || !TryConvertToAstronomicalYear(y, out var astroY))
        { date = default!; return false; }
        date = new AstronomicalDate(astroY, Month, Day, Calendar?.Value == CalendarStyle.Gregorian);
        return true;
    }

    private bool TryConvertToAstronomicalYear(int entered, out int astroYear)
    {
        switch (YearCount?.Value)
        {
            case Domain.YearCount.Astronomical: astroYear = entered; return true;
            case Domain.YearCount.CE: if (entered > 0) { astroYear = entered; return true; } break;
            case Domain.YearCount.BCE: if (entered > 0) { astroYear = 1 - entered; return true; } break;
        }
        astroYear = 0; return false;
    }

    private static bool IsValidCalendarDate(AstronomicalDate d)
    {
        if (d.Month is < 1 or > 12 || d.Day < 1) return false;
        var max = d.Month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11 => 30,
            2 => (d.Gregorian ? (d.Year % 4 == 0 && (d.Year % 100 != 0 || d.Year % 400 == 0)) : d.Year % 4 == 0) ? 29 : 28,
            _ => 0
        };
        return d.Day <= max;
    }

    private double ComputeJulianDay(AstronomicalDate date)
    {
        var localJd = SEWrapper.JulianDay(date, new AstronomicalTime(Hour, Minute, Second));
        var offsetSec = (double)(OffsetHour * 3600 + OffsetMinute * 60);
        var sign = OffsetDirection?.Value == UTOffsetDirection.Earlier ? 1.0 : -1.0;
        var dst  = Dst?.Value == DSTOption.DST ? -3600.0 : 0.0;
        return localJd + (sign * offsetSec + dst) / 86_400.0;
    }

    private string BuildOffsetString()
    {
        var sign = OffsetDirection?.Value == UTOffsetDirection.Later ? "+" : "-";
        return $"{sign}{OffsetHour:D2}:{OffsetMinute:D2}";
    }

    private string BuildOriginalInput(AstronomicalDate date)
    {
        var yStr = YearCount?.Value == Domain.YearCount.BCE ? $"{Year} BCE" :
                   YearCount?.Value == Domain.YearCount.CE  ? $"{Year} CE"  : Year;
        var cal  = Calendar?.Value == CalendarStyle.Julian ? "Jul." : "Greg.";
        var dst  = Dst?.Value == DSTOption.DST ? " DST" : string.Empty;
        return $"{yStr} {date.Month:D2}-{date.Day:D2} {Hour:D2}:{Minute:D2}:{Second:D2} (UT{BuildOffsetString()}{dst}) {cal}";
    }

    private static double DmsToDecimal(int deg, int min, int sec, bool negative)
    {
        var v = deg + min / 60.0 + sec / 3600.0;
        return negative ? -v : v;
    }

    private static (int deg, int min, int sec, bool negative) DecimalToDms(double value)
    {
        var neg = value < 0;
        var abs = Math.Abs(value);
        var deg = (int)abs;
        var mf  = (abs - deg) * 60.0;
        var min = (int)mf;
        var sec = (int)Math.Round((mf - min) * 60.0);
        if (sec == 60) { sec = 0; min++; }
        if (min == 60) { min = 0; deg++; }
        return (deg, min, sec, neg);
    }

    private static (UTOffsetDirection dir, int hours, int minutes) ParseTz(string tzId)
    {
        if (string.IsNullOrEmpty(tzId)) return (UTOffsetDirection.Later, 0, 0);
        var dir = tzId.StartsWith('+') ? UTOffsetDirection.Later : UTOffsetDirection.Earlier;
        var tail = tzId[1..];
        var parts = tail.Split(':');
        var h = parts.Length > 0 && int.TryParse(parts[0], out var ph) ? ph : 0;
        var m = parts.Length > 1 && int.TryParse(parts[1], out var pm) ? pm : 0;
        return (dir, h, m);
    }

    private void InitEnumLists()
    {
        LatitudeDirectionValues  = ToDisplayList<LatitudeHemisphere>(v => EnumKeySelector.Key(v));
        LongitudeDirectionValues = ToDisplayList<LongitudeHemisphere>(v => EnumKeySelector.Key(v));
        CalendarValues           = ToDisplayList<CalendarStyle>(v => EnumKeySelector.Key(v));
        YearCountValues          = ToDisplayList<Domain.YearCount>(v => EnumKeySelector.Key(v));
        DstValues                = ToDisplayList<DSTOption>(v => EnumKeySelector.Key(v));
        OffsetDirectionValues    = ToDisplayList<UTOffsetDirection>(v => EnumKeySelector.Key(v));
    }

    private IReadOnlyList<DisplayItem<T>> ToDisplayList<T>(Func<T, string> keySelector)
        where T : struct, Enum
        => Enum.GetValues<T>()
               .Select(v => new DisplayItem<T>(v, _rosetta.GetText(RbFile.Localizable, keySelector(v))))
               .ToList();
}
