// EclipsesInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Periods.Eclipses.API;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Periods.Eclipses.UI;

public sealed class EclipsesInputViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly EclipsesModel _model;

    // ── Start date ────────────────────────────────────────────────────────────

    private string _startYearText = "2024";
    private int _startMonth = 1;
    private int _startDay = 1;
    private CalendarStyle _startCalendar = CalendarStyle.Gregorian;
    private YearCount _startYearCount = YearCount.CE;

    // ── End date ──────────────────────────────────────────────────────────────

    private string _endYearText = "2026";
    private int _endMonth = 12;
    private int _endDay = 31;
    private CalendarStyle _endCalendar = CalendarStyle.Gregorian;
    private YearCount _endYearCount = YearCount.CE;

    // ── Eclipse type ──────────────────────────────────────────────────────────

    private EclipseSearchType _eclipseSearchType = EclipseSearchType.All;

    // ── Location (optional) ──────────────────────────────────────────────────

    private int _latitudeDegrees;
    private int _latitudeMinutes;
    private int _latitudeSeconds;
    private int _longitudeDegrees;
    private int _longitudeMinutes;
    private int _longitudeSeconds;
    private LatitudeHemisphere _latHemi = LatitudeHemisphere.North;
    private LongitudeHemisphere _lonHemi = LongitudeHemisphere.East;

    private string? _startDateError;
    private string? _endDateError;
    private string? _dateOrderError;

    public EclipsesInputViewModel(IRosetta rosetta, EclipsesModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        CalculateCommand = new RelayCommand(Calculate, () => CanCalculate);
        Validate();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IRelayCommand CalculateCommand { get; }

    // ── Bound collections ─────────────────────────────────────────────────────

    public static List<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public static List<int> DayValues   { get; } = Enumerable.Range(1, 31).ToList();
    public IEnumerable<CalendarStyle> AllCalendarStyles => Enum.GetValues<CalendarStyle>();
    public IEnumerable<YearCount> AllYearCounts => Enum.GetValues<YearCount>();
    public IEnumerable<EclipseSearchType> AllEclipseSearchTypes => Enum.GetValues<EclipseSearchType>();
    public IEnumerable<LatitudeHemisphere> AllLatitudeHemispheres => Enum.GetValues<LatitudeHemisphere>();
    public IEnumerable<LongitudeHemisphere> AllLongitudeHemispheres => Enum.GetValues<LongitudeHemisphere>();

    // ── Start date properties ────────────────────────────────────────────────

    public string StartYearText
    {
        get => _startYearText;
        set { _startYearText = value; OnPropertyChanged(); Validate(); }
    }
    public int StartMonth
    {
        get => _startMonth;
        set { _startMonth = value; OnPropertyChanged(); Validate(); }
    }
    public int StartDay
    {
        get => _startDay;
        set { _startDay = value; OnPropertyChanged(); Validate(); }
    }
    public CalendarStyle StartCalendar
    {
        get => _startCalendar;
        set { _startCalendar = value; OnPropertyChanged(); Validate(); }
    }
    public YearCount StartYearCount
    {
        get => _startYearCount;
        set { _startYearCount = value; OnPropertyChanged(); Validate(); }
    }

    // ── End date properties ──────────────────────────────────────────────────

    public string EndYearText
    {
        get => _endYearText;
        set { _endYearText = value; OnPropertyChanged(); Validate(); }
    }
    public int EndMonth
    {
        get => _endMonth;
        set { _endMonth = value; OnPropertyChanged(); Validate(); }
    }
    public int EndDay
    {
        get => _endDay;
        set { _endDay = value; OnPropertyChanged(); Validate(); }
    }
    public CalendarStyle EndCalendar
    {
        get => _endCalendar;
        set { _endCalendar = value; OnPropertyChanged(); Validate(); }
    }
    public YearCount EndYearCount
    {
        get => _endYearCount;
        set { _endYearCount = value; OnPropertyChanged(); Validate(); }
    }

    // ── Eclipse type ──────────────────────────────────────────────────────────

    public EclipseSearchType EclipseSearchType
    {
        get => _eclipseSearchType;
        set { _eclipseSearchType = value; OnPropertyChanged(); }
    }

    // ── Location properties ──────────────────────────────────────────────────

    public int LatitudeDegrees
    {
        get => _latitudeDegrees;
        set { _latitudeDegrees = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public int LatitudeMinutes
    {
        get => _latitudeMinutes;
        set { _latitudeMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public int LatitudeSeconds
    {
        get => _latitudeSeconds;
        set { _latitudeSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public int LongitudeDegrees
    {
        get => _longitudeDegrees;
        set { _longitudeDegrees = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public int LongitudeMinutes
    {
        get => _longitudeMinutes;
        set { _longitudeMinutes = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public int LongitudeSeconds
    {
        get => _longitudeSeconds;
        set { _longitudeSeconds = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public LatitudeHemisphere LatHemi
    {
        get => _latHemi;
        set { _latHemi = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }
    public LongitudeHemisphere LonHemi
    {
        get => _lonHemi;
        set { _lonHemi = value; OnPropertyChanged(); OnPropertyChanged(nameof(CoordinatesText)); }
    }

    public bool HasCoordinates =>
        _latitudeDegrees != 0 || _latitudeMinutes != 0 || _latitudeSeconds != 0 ||
        _longitudeDegrees != 0 || _longitudeMinutes != 0 || _longitudeSeconds != 0;

    public string CoordinatesText => HasCoordinates
        ? $"{CoordText(DecimalLat, isLat: true)}  {CoordText(DecimalLon, isLat: false)}"
        : string.Empty;

    private double DecimalLat
    {
        get
        {
            var v = _latitudeDegrees + _latitudeMinutes / 60.0 + _latitudeSeconds / 3600.0;
            return _latHemi == LatitudeHemisphere.South ? -v : v;
        }
    }

    private double DecimalLon
    {
        get
        {
            var v = _longitudeDegrees + _longitudeMinutes / 60.0 + _longitudeSeconds / 3600.0;
            return _lonHemi == LongitudeHemisphere.West ? -v : v;
        }
    }

    private static string CoordText(double value, bool isLat)
    {
        var abs = Math.Abs(value);
        var deg = (int)abs;
        var minFrac = (abs - deg) * 60.0;
        var min = (int)minFrac;
        var sec = (int)((minFrac - min) * 60.0);
        var hemi = isLat ? (value >= 0 ? "N" : "S") : (value >= 0 ? "E" : "W");
        return $"{deg}° {min:00}' {sec:00}\" {hemi}";
    }

    // ── Validation ────────────────────────────────────────────────────────────

    public string? StartDateError
    {
        get => _startDateError;
        private set { _startDateError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStartDateError)); }
    }
    public bool HasStartDateError => _startDateError is not null;

    public string? EndDateError
    {
        get => _endDateError;
        private set { _endDateError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasEndDateError)); }
    }
    public bool HasEndDateError => _endDateError is not null;

    public string? DateOrderError
    {
        get => _dateOrderError;
        private set { _dateOrderError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDateOrderError)); }
    }
    public bool HasDateOrderError => _dateOrderError is not null;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle        => T("view.eclipses.title");
    public string LabelStartDate    => T("view.eclipses.startdate");
    public string LabelEndDate      => T("view.eclipses.enddate");
    public string LabelEclipseType  => T("view.eclipses.eclipsetype");
    public string LabelSolar        => T("view.eclipses.solar");
    public string LabelLunar        => T("view.eclipses.lunar");
    public string LabelAll          => T("view.eclipses.all");
    public string LabelLocation     => T("view.eclipses.location");
    public string LabelCoordinates  => T("view.eclipses.coordinates");
    public string LabelCalculate    => T("view.eclipses.calculate");
    public string LabelYear         => T("view.eclipses.year");
    public string LabelMonth        => T("view.eclipses.month");
    public string LabelDay          => T("view.eclipses.day");
    public string LabelCalendar     => T("view.eclipses.calendar");
    public string LabelYearCount    => T("view.eclipses.yearcount");

    public string LocalizedName(CalendarStyle c) =>
        _rosetta.GetText(RbFile.Localizable, c == CalendarStyle.Gregorian
            ? "enum.calendarstyle.gregorian" : "enum.calendarstyle.julian");
    public string LocalizedName(YearCount y) =>
        _rosetta.GetText(RbFile.Localizable, y switch
        {
            YearCount.CE           => "enum.yearcount.ce",
            YearCount.BCE          => "enum.yearcount.bce",
            YearCount.Astronomical => "enum.yearcount.astronomical",
            _                      => "enum.yearcount.ce"
        });
    public string LocalizedName(EclipseSearchType t) => t switch
    {
        EclipseSearchType.Solar => LabelSolar,
        EclipseSearchType.Lunar => LabelLunar,
        _                       => LabelAll
    };

    // ── Calculate ─────────────────────────────────────────────────────────────

    private bool CanCalculate =>
        !HasStartDateError && !HasEndDateError && !HasDateOrderError;

    private void Calculate()
    {
        if (!CanCalculate) return;

        var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
        var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);

        double? geoLon = HasCoordinates ? DecimalLon : null;
        double? geoLat = HasCoordinates ? DecimalLat : null;

        var events = EclipsesOrchestrator.FindEclipses(jdStart, jdEnd, _eclipseSearchType, geoLon, geoLat);

        _model.HasLocation = HasCoordinates;
        _model.Results     = events;
    }

    // ── Validation helpers ────────────────────────────────────────────────────

    private void Validate()
    {
        StartDateError = ValidateDate(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar)
            ? null : T("view.eclipses.validation.invalidyear");
        EndDateError = ValidateDate(_endYearText, _endYearCount, _endMonth, _endDay, _endCalendar)
            ? null : T("view.eclipses.validation.invalidyear");

        if (StartDateError is null && EndDateError is null)
        {
            var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
            var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
            DateOrderError = jdEnd > jdStart ? null : T("view.eclipses.validation.enddateorder");
        }
        else
        {
            DateOrderError = null;
        }

        CalculateCommand.NotifyCanExecuteChanged();
    }

    private static bool ValidateDate(string yearText, YearCount yc, int month, int day, CalendarStyle cal)
    {
        if (!int.TryParse(yearText, out var y)) return false;
        if (ToAstrYear(y, yc) is not { } astrYear) return false;
        if (month is < 1 or > 12) return false;
        if (day < 1) return false;
        var gregorian = cal == CalendarStyle.Gregorian;
        var maxDay = month switch
        {
            1 or 3 or 5 or 7 or 8 or 10 or 12 => 31,
            4 or 6 or 9 or 11                  => 30,
            2 => IsLeapYear(astrYear, gregorian) ? 29 : 28,
            _  => 0
        };
        return day <= maxDay;
    }

    private static bool IsLeapYear(int year, bool gregorian) =>
        gregorian ? year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) : year % 4 == 0;

    private static double ToJd(string yearText, YearCount yc, int month, int day, CalendarStyle cal)
    {
        int.TryParse(yearText, out var y);
        var astrYear = ToAstrYear(y, yc) ?? 1;
        var date = new AstronomicalDate(astrYear, month, day, cal == CalendarStyle.Gregorian);
        return SEWrapper.JulianDay(date, new AstronomicalTime(0, 0, 0));
    }

    private static int? ToAstrYear(int y, YearCount yc) => yc switch
    {
        YearCount.Astronomical => y,
        YearCount.CE           => y > 0 ? y : null,
        YearCount.BCE          => y > 0 ? 1 - y : null,
        _                      => null
    };

    private string T(string key) => _rosetta.GetText(RbFile.Eclipses, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
