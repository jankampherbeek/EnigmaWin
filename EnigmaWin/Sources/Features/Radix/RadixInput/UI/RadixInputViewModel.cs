// RadixInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Location;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.Sources.Features.Shared.Validation;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class RadixInputViewModel : ObservableObject
{
    private readonly RadixInputModel _model;
    protected readonly INavigationService _navigationService;
    protected readonly IChartSession _chartSession;
    protected readonly IRosetta _rosetta;
    protected readonly IHoroscopeRepository _horoscopeRepository;
    protected readonly IConfigContext _configContext;

    private readonly LocationOrchestrator _locationOrchestrator = new();
    private CancellationTokenSource? _countrySearchCts;
    private CancellationTokenSource? _citySearchCts;
    private bool _suppressCountrySearch;
    private bool _suppressCitySearch;

    public IReadOnlyList<int> HourValues { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> DegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues { get; } = Enumerable.Range(1, 31).ToList();

    public IReadOnlyList<DisplayItem<RoddenRating>> RoddenRatingValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<LongitudeHemisphere>> LongitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<LatitudeHemisphere>> LatitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<CalendarStyle>> CalendarValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<YearCount>> YearCountValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<DSTOption>> DstValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<UTOffsetDirection>> OffsetDirectionValues { get; private set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCalculate))]
    private string _chartName = string.Empty;

    [ObservableProperty]
    private DisplayItem<RoddenRating> _roddenRating = null!;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _source = string.Empty;

    [ObservableProperty]
    private string _countryQuery = string.Empty;

    [ObservableProperty]
    private string _cityQuery = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<LocationCountry> _filteredCountries = [];

    [ObservableProperty]
    private IReadOnlyList<LocationCity> _filteredCities = [];

    [ObservableProperty]
    private LocationCountry? _selectedCountry;

    [ObservableProperty]
    private LocationCity? _selectedCity;

    [ObservableProperty]
    private bool _countryDropdownVisible;

    [ObservableProperty]
    private bool _cityDropdownVisible;

    [ObservableProperty]
    private string _locationSearchError = string.Empty;

    [ObservableProperty]
    private string _locationName = string.Empty;

    [ObservableProperty]
    private int _longitudeDegree;

    [ObservableProperty]
    private int _longitudeMinute;

    [ObservableProperty]
    private int _longitudeSecond;

    [ObservableProperty]
    private DisplayItem<LongitudeHemisphere> _longitudeDirection = null!;

    [ObservableProperty]
    private int _latitudeDegree;

    [ObservableProperty]
    private int _latitudeMinute;

    [ObservableProperty]
    private int _latitudeSecond;

    [ObservableProperty]
    private DisplayItem<LatitudeHemisphere> _latitudeDirection = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCalculate))]
    private string _year = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCalculate))]
    private int _month = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCalculate))]
    private int _day = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCalculate))]
    private DisplayItem<CalendarStyle> _calendar = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCalculate))]
    private DisplayItem<YearCount> _yearCount = null!;

    [ObservableProperty]
    private int _hour;

    [ObservableProperty]
    private int _minute;

    [ObservableProperty]
    private int _second;

    [ObservableProperty]
    private DisplayItem<DSTOption> _dst = null!;

    [ObservableProperty]
    private int _offsetHour;

    [ObservableProperty]
    private int _offsetMinute;

    [ObservableProperty]
    private int _offsetSecond;

    [ObservableProperty]
    private DisplayItem<UTOffsetDirection> _offsetDirection = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAboutSectionError))]
    private string _aboutSectionError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDateTimeSectionError))]
    private string _dateTimeSectionError = string.Empty;

    public bool HasAboutSectionError => !string.IsNullOrWhiteSpace(AboutSectionError);
    public bool HasDateTimeSectionError => !string.IsNullOrWhiteSpace(DateTimeSectionError);
    public bool CanCalculate => IsAboutSectionValid(out _) && IsDateTimeSectionValid(out _);

    public string LabelTitle            => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.title");
    public string LabelAboutChart       => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.aboutchart");
    public string LabelName             => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.name");
    public string LabelDescription      => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.description");
    public string LabelSource           => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.source");
    public string LabelRoddenRating     => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.roddenrating");
    public string LabelLocation         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.location");
    public string LabelCountry          => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.country");
    public string HintCountry           => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.country.placeholder");
    public string HintCity              => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.city.placeholder");
    public string LabelNameOfLocation   => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.nameoflocation");
    public string LabelLongitude        => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.longitude");
    public string LabelLatitude         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.latitude");
    public string LabelDateAndTime      => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.datetime");
    public string LabelDate             => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.date");
    public string LabelCalendarYearCount => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.calendaryearcount");
    public string LabelTimeDst          => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.timedst");
    public string LabelOffsetUt         => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.offsetut");
    public string LabelCalculate        => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.calculate");

    public string HintChartName     => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.chartname");
    public string HintDescription   => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.description");
    public string HintSource        => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.source");
    public string HintLocationName  => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.hint.Locationname");
    public string TooltipHelp       => _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.tooltip");

    public RadixInputViewModel(RadixInputModel model, INavigationService navigationService, IChartSession chartSession, IRosetta rosetta, IHoroscopeRepository horoscopeRepository, IConfigContext configContext)
    {
        _model = model;
        _navigationService = navigationService;
        _chartSession = chartSession;
        _rosetta = rosetta;
        _horoscopeRepository = horoscopeRepository;
        _configContext = configContext;

        InitializeEnumLists();
        SetDefaults();
    }

    partial void OnChartNameChanged(string value) => ValidateAbout();
    partial void OnYearChanged(string value) { ValidateDateTime(); RecalculateOffset(); }
    partial void OnMonthChanged(int value) { ValidateDateTime(); RecalculateOffset(); }
    partial void OnDayChanged(int value) { ValidateDateTime(); RecalculateOffset(); }
    partial void OnCalendarChanged(DisplayItem<CalendarStyle> value) => ValidateDateTime();
    partial void OnYearCountChanged(DisplayItem<YearCount> value) => ValidateDateTime();

    internal void ValidateAbout()
    {
        AboutSectionError = IsAboutSectionValid(out var msg) ? string.Empty : msg;
    }

    internal void ValidateDateTime()
    {
        DateTimeSectionError = IsDateTimeSectionValid(out var msg) ? string.Empty : msg;
    }

    partial void OnCountryQueryChanged(string value)
    {
        if (_suppressCountrySearch) return;
        _countrySearchCts?.Cancel();
        _countrySearchCts = new CancellationTokenSource();
        var token = _countrySearchCts.Token;
        var q = value.Trim();
        if (string.IsNullOrEmpty(q) || q == "*") { FilteredCountries = []; CountryDropdownVisible = false; return; }
        _ = Task.Run(async () =>
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            var results = _locationOrchestrator.Countries(_rosetta.GetLanguage(), q);
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredCountries = results;
                CountryDropdownVisible = results.Count > 0;
            });
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
        var countryCode = SelectedCountry.Code;
        _ = Task.Run(async () =>
        {
            await Task.Delay(400, token);
            if (token.IsCancellationRequested) return;
            var results = _locationOrchestrator.Cities(countryCode, q);
            Application.Current.Dispatcher.Invoke(() =>
            {
                FilteredCities = results;
                CityDropdownVisible = results.Count > 0;
            });
        }, token);
    }

    internal void SelectCountry(LocationCountry country)
    {
        _countrySearchCts?.Cancel();
        _citySearchCts?.Cancel();
        _suppressCountrySearch = true;
        _suppressCitySearch = true;
        SelectedCountry = country;
        CountryQuery = country.Name;
        FilteredCountries = [];
        CountryDropdownVisible = false;
        CityQuery = string.Empty;
        SelectedCity = null;
        FilteredCities = [];
        CityDropdownVisible = false;
        _suppressCountrySearch = false;
        _suppressCitySearch = false;
    }

    internal void SelectCity(LocationCity city)
    {
        _citySearchCts?.Cancel();
        _suppressCitySearch = true;
        SelectedCity = city;
        CityQuery = city.Name;
        LocationName = city.Name;
        FilteredCities = [];
        CityDropdownVisible = false;
        ApplyCity(city);
        _suppressCitySearch = false;
    }

    private void ApplyCity(LocationCity city)
    {
        var (latDeg, latMin, latSec, latNeg) = DecimalToDms(city.Latitude);
        LatitudeDegree    = latDeg;
        LatitudeMinute    = latMin;
        LatitudeSecond    = latSec;
        LatitudeDirection = LatitudeDirectionValues.First(v => v.Value == (latNeg ? LatitudeHemisphere.South : LatitudeHemisphere.North));

        var (lonDeg, lonMin, lonSec, lonNeg) = DecimalToDms(city.Longitude);
        LongitudeDegree    = lonDeg;
        LongitudeMinute    = lonMin;
        LongitudeSecond    = lonSec;
        LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == (lonNeg ? LongitudeHemisphere.West : LongitudeHemisphere.East));

        var refDate = new AstronomicalDate(DateTime.Now.Year, 1, 1);
        var refTime = new AstronomicalTime(12, 0, 0);
        using var tmpOrch = new LocationOrchestrator();
        var zone = tmpOrch.TimezoneInfo(city.TimezoneName, new AstronomicalDateTime(refDate, refTime), city.Longitude);
        ApplyZoneInfo(zone);
    }

    internal void RecalculateOffset()
    {
        if (SelectedCity is null) return;
        if (!TryGetCurrentAstronomicalDate(out var date)) return;
        var dt = new AstronomicalDateTime(date, new AstronomicalTime(Hour, Minute, Second));
        var zone = _locationOrchestrator.TimezoneInfo(SelectedCity.TimezoneName, dt, SelectedCity.Longitude);
        ApplyZoneInfo(zone);
    }

    private void ApplyZoneInfo(ZoneInfo zone)
    {
        // zone.OffsetSeconds is the total offset (standard + DST). Strip DST so that
        // ToUtJulianDay can apply the DST correction separately via the Dst flag.
        var dstSave = zone.DstUsed ? 3600 : 0;
        var stdSec = zone.OffsetSeconds - dstSave;
        var absSec = Math.Abs(stdSec);
        OffsetHour   = absSec / 3600;
        OffsetMinute = (absSec % 3600) / 60;
        OffsetSecond = 0;
        OffsetDirection = OffsetDirectionValues.First(v =>
            v.Value == (stdSec >= 0 ? UTOffsetDirection.Later : UTOffsetDirection.Earlier));
        Dst = DstValues.First(v => v.Value == (zone.DstUsed ? DSTOption.DST : DSTOption.NoDST));
    }

    private bool TryGetCurrentAstronomicalDate(out AstronomicalDate date)
    {
        if (!int.TryParse(Year, out var enteredYear) || !TryGetAstronomicalYear(enteredYear, out var astroYear))
        {
            date = default!;
            return false;
        }
        date = new AstronomicalDate(astroYear, Month, Day, Calendar.Value == CalendarStyle.Gregorian);
        return true;
    }

    private static (int deg, int min, int sec, bool negative) DecimalToDms(double value)
    {
        var negative = value < 0;
        var abs      = Math.Abs(value);
        var deg      = (int)abs;
        var minRaw   = (abs - deg) * 60.0;
        var min      = (int)minRaw;
        var sec      = (int)Math.Round((minRaw - min) * 60.0);
        if (sec == 60) { sec = 0; min++; }
        if (min == 60) { min = 0; deg++; }
        return (deg, min, sec, negative);
    }

    internal async Task CalculateAsync()
    {
        if (!CanCalculate || !int.TryParse(Year, out var enteredYear))
            return;

        var inputData = new RadixInputModel.InputData(
            ChartName: ChartName,
            Year: enteredYear,
            Month: Month,
            Day: Day,
            Calendar: Calendar.Value,
            YearCount: YearCount.Value,
            Hour: Hour,
            Minute: Minute,
            Second: Second,
            OffsetHour: OffsetHour,
            OffsetMinute: OffsetMinute,
            OffsetSecond: OffsetSecond,
            OffsetDirection: OffsetDirection.Value,
            Dst: Dst.Value,
            LongitudeDegree: LongitudeDegree,
            LongitudeMinute: LongitudeMinute,
            LongitudeSecond: LongitudeSecond,
            LongitudeDirection: LongitudeDirection.Value,
            LatitudeDegree: LatitudeDegree,
            LatitudeMinute: LatitudeMinute,
            LatitudeSecond: LatitudeSecond,
            LatitudeDirection: LatitudeDirection.Value
        );

        var (chart, request) = _model.CalculateWithRequest(inputData, _configContext.ActiveConfig.FactorConfig, _configContext.ActiveConfig.CalculationConfig);

        var horoscope = new Horoscope
        {
            Name         = ChartName,
            Category     = "Radix",
            Notes        = string.IsNullOrWhiteSpace(Description) ? null : Description,
            Source       = string.IsNullOrWhiteSpace(Source) ? null : Source,
            RoddenRating = RoddenRating.Value,
            PlaceName    = string.IsNullOrWhiteSpace(LocationName) ? null : LocationName,
            Latitude     = request.Latitude,
            Longitude    = request.Longitude
        };

        var dateTime = new HoroscopeDateTime
        {
            HoroscopeId        = horoscope.Id,
            JulianDate         = request.JulianDay,
            TimeZoneIdentifier = BuildOffsetString(OffsetDirection.Value, OffsetHour, OffsetMinute, OffsetSecond),
            TimeIsUnknown      = false,
            IsPreferred        = true,
            OriginalInput      = BuildOriginalInput(enteredYear, Month, Day, Hour, Minute, Second,
                                     OffsetDirection.Value, OffsetHour, OffsetMinute, OffsetSecond)
        };

        await _horoscopeRepository.AddAsync(horoscope);
        await _horoscopeRepository.AddDateTimeAsync(horoscope.Id, dateTime);

        _chartSession.Add(ChartName, chart);
        _navigationService.NavigateMain(AppRoutes.RadixChart);
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    private static string BuildOffsetString(UTOffsetDirection direction, int hours, int minutes, int seconds)
    {
        var sign = direction == UTOffsetDirection.Later ? "+" : "-";
        return $"{sign}{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    private static string BuildOriginalInput(int year, int month, int day, int hour, int minute, int second,
        UTOffsetDirection direction, int offsetH, int offsetM, int offsetS)
    {
        var offset = BuildOffsetString(direction, offsetH, offsetM, offsetS);
        return $"{year:D4}-{month:D2}-{day:D2} {hour:D2}:{minute:D2}:{second:D2} {offset}";
    }

    internal void Clear()
    {
        ChartName = string.Empty;
        Description = string.Empty;
        Source = string.Empty;
        CountryQuery = string.Empty;
        CityQuery = string.Empty;
        FilteredCountries = [];
        FilteredCities = [];
        SelectedCountry = null;
        SelectedCity = null;
        CountryDropdownVisible = false;
        CityDropdownVisible = false;
        LocationName = string.Empty;
        LongitudeDegree = 0;
        LongitudeMinute = 0;
        LongitudeSecond = 0;
        LatitudeDegree = 0;
        LatitudeMinute = 0;
        LatitudeSecond = 0;
        Year = string.Empty;
        Month = 1;
        Day = 1;
        Hour = 0;
        Minute = 0;
        Second = 0;
        OffsetHour = 0;
        OffsetMinute = 0;
        OffsetSecond = 0;
        AboutSectionError = string.Empty;
        DateTimeSectionError = string.Empty;
        SetDefaults();
    }

    private void InitializeEnumLists()
    {
        RoddenRatingValues       = ToDisplayList<Domain.RoddenRating>(v => EnumKeySelector.Key(v));
        LongitudeDirectionValues = ToDisplayList<LongitudeHemisphere>(v => EnumKeySelector.Key(v));
        LatitudeDirectionValues  = ToDisplayList<LatitudeHemisphere>(v => EnumKeySelector.Key(v));
        CalendarValues           = ToDisplayList<CalendarStyle>(v => EnumKeySelector.Key(v));
        YearCountValues          = ToDisplayList<Domain.YearCount>(v => EnumKeySelector.Key(v));
        DstValues                = ToDisplayList<DSTOption>(v => EnumKeySelector.Key(v));
        OffsetDirectionValues    = ToDisplayList<UTOffsetDirection>(v => EnumKeySelector.Key(v));
    }

    private void SetDefaults()
    {
        RoddenRating       = RoddenRatingValues.First(v => v.Value == Domain.RoddenRating.AA);
        LongitudeDirection = LongitudeDirectionValues.First(v => v.Value == LongitudeHemisphere.East);
        LatitudeDirection  = LatitudeDirectionValues.First(v => v.Value == LatitudeHemisphere.North);
        Calendar           = CalendarValues.First(v => v.Value == CalendarStyle.Gregorian);
        YearCount          = YearCountValues.First(v => v.Value == Domain.YearCount.CE);
        Dst                = DstValues.First(v => v.Value == DSTOption.NoDST);
        OffsetDirection    = OffsetDirectionValues.First(v => v.Value == UTOffsetDirection.Later);
    }

    private IReadOnlyList<DisplayItem<T>> ToDisplayList<T>(Func<T, string> keySelector)
        where T : struct, Enum
    {
        return Enum.GetValues<T>()
            .Select(v => new DisplayItem<T>(v, _rosetta.GetText(RbFile.Localizable, keySelector(v))))
            .ToList();
    }

    private bool IsAboutSectionValid(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(ChartName))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.nameempty");
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private bool IsDateTimeSectionValid(out string errorMessage)
    {
        if (!int.TryParse(Year, out var enteredYear))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.yearnotanumber");
            return false;
        }

        if (!TryGetAstronomicalYear(enteredYear, out var astronomicalYear))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.invalidyear");
            return false;
        }

        var date = new AstronomicalDate(
            Year: astronomicalYear,
            Month: Month,
            Day: Day,
            Gregorian: Calendar.Value == CalendarStyle.Gregorian
        );

        if (!IsValidByCalendarRules(date))
        {
            errorMessage = _rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.validation.dateinvalid");
            return false;
        }

        try
        {
            AstronomicalDateValidation.ValidateDate(date);
        }
        catch
        {
            // Fall back to deterministic calendar validation already performed above.
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool IsValidByCalendarRules(AstronomicalDate date)
    {
        if (date.Month is < 1 or > 12) return false;
        if (date.Day < 1) return false;

        var maxDay = date.Month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11 => 30,
            2 => IsLeapYear(date.Year, date.Gregorian) ? 29 : 28,
            _ => 0
        };

        return date.Day <= maxDay;
    }

    private static bool IsLeapYear(int year, bool gregorian) =>
        gregorian ? year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) : year % 4 == 0;

    private bool TryGetAstronomicalYear(int enteredYear, out int astronomicalYear)
    {
        switch (YearCount.Value)
        {
            case Domain.YearCount.Astronomical:
                astronomicalYear = enteredYear;
                return true;
            case Domain.YearCount.CE:
                if (enteredYear > 0) { astronomicalYear = enteredYear; return true; }
                astronomicalYear = 0;
                return false;
            case Domain.YearCount.BCE:
                if (enteredYear > 0) { astronomicalYear = 1 - enteredYear; return true; }
                astronomicalYear = 0;
                return false;
            default:
                astronomicalYear = enteredYear;
                return true;
        }
    }
}
