// WavesScreenViewModel.cs
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
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Periods.CyclesWaves.UI;

public sealed class WavesScreenViewModel : INotifyPropertyChanged
{
    private readonly IRosetta  _rosetta;
    private readonly WavesModel _model;

    // ── Date fields ───────────────────────────────────────────────────────────

    private string _startYearText = "2000";
    private int _startMonth = 1;
    private int _startDay   = 1;
    private CalendarStyle _startCalendar  = CalendarStyle.Gregorian;
    private YearCount     _startYearCount = YearCount.CE;

    private string _endYearText = "2100";
    private int _endMonth = 12;
    private int _endDay   = 31;
    private CalendarStyle _endCalendar  = CalendarStyle.Gregorian;
    private YearCount     _endYearCount = YearCount.CE;

    // ── Calculation parameters ────────────────────────────────────────────────

    private ObserverPositions _observerPosition   = ObserverPositions.Heliocentric;
    private Coordinates       _selectedCoordinate = Coordinates.Longitude;
    private bool _jupiterSelected;
    private bool _saturnSelected = true;
    private bool _uranusSelected;

    private string? _startDateError;
    private string? _endDateError;
    private string? _dateOrderError;

    private static readonly Dictionary<Factors, int> WaveIntervals = new()
    {
        [Factors.Jupiter] = 5,
        [Factors.Saturn]  = 5,
        [Factors.Uranus]  = 10
    };

    public WavesScreenViewModel(IRosetta rosetta, WavesModel model)
    {
        _rosetta = rosetta;
        _model   = model;
        CalculateCommand = new RelayCommand(Calculate, () => CanCalculate);
        Validate();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IRelayCommand CalculateCommand { get; }

    // ── Bound collections ─────────────────────────────────────────────────────

    public IEnumerable<CalendarStyle>    AllCalendarStyles    => Enum.GetValues<CalendarStyle>();
    public IEnumerable<YearCount>        AllYearCounts        => Enum.GetValues<YearCount>();
    public IEnumerable<ObserverPositions> AllObserverPositions =>
        Enum.GetValues<ObserverPositions>().Where(p => p != ObserverPositions.Topocentric);
    public IEnumerable<Coordinates> AvailableCoordinates =>
        _observerPosition == ObserverPositions.Heliocentric
            ? [Coordinates.Longitude, Coordinates.Latitude, Coordinates.Distance]
            : Enum.GetValues<Coordinates>();

    // ── Date list helpers ─────────────────────────────────────────────────────

    public static List<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public static List<int> DayValues   { get; } = Enumerable.Range(1, 31).ToList();

    // ── Date properties ───────────────────────────────────────────────────────

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

    // ── Calculation parameter properties ──────────────────────────────────────

    public ObserverPositions ObserverPosition
    {
        get => _observerPosition;
        set
        {
            _observerPosition = value;
            OnPropertyChanged();
            if (!AvailableCoordinates.Contains(_selectedCoordinate))
                SelectedCoordinate = Coordinates.Longitude;
            OnPropertyChanged(nameof(AvailableCoordinates));
            Validate();
        }
    }

    public Coordinates SelectedCoordinate
    {
        get => _selectedCoordinate;
        set { _selectedCoordinate = value; OnPropertyChanged(); }
    }

    public bool JupiterSelected
    {
        get => _jupiterSelected;
        set { _jupiterSelected = value; OnPropertyChanged(); Validate(); }
    }
    public bool SaturnSelected
    {
        get => _saturnSelected;
        set { _saturnSelected = value; OnPropertyChanged(); Validate(); }
    }
    public bool UranusSelected
    {
        get => _uranusSelected;
        set { _uranusSelected = value; OnPropertyChanged(); Validate(); }
    }

    // ── Validation feedback ───────────────────────────────────────────────────

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

    public string LabelTitle          => T("view.waves.title");
    public string LabelStartDate      => T("view.waves.startdate");
    public string LabelEndDate        => T("view.waves.enddate");
    public string LabelYear           => T("view.waves.year");
    public string LabelMonth          => T("view.waves.month");
    public string LabelDay            => T("view.waves.day");
    public string LabelCalendar       => T("view.waves.calendar");
    public string LabelYearCount      => T("view.waves.yearcount");
    public string LabelObserverPos    => T("view.waves.observerposition");
    public string LabelCoordinate     => T("view.waves.coordinate");
    public string LabelCycleType      => T("view.waves.cycletype");
    public string LabelCalculate      => T("view.waves.calculate");
    public string LabelJupiter        => _rosetta.GetText(RbFile.Localizable, Factors.Jupiter.LocalizedName());
    public string LabelSaturn         => _rosetta.GetText(RbFile.Localizable, Factors.Saturn.LocalizedName());
    public string LabelUranus         => _rosetta.GetText(RbFile.Localizable, Factors.Uranus.LocalizedName());

    public string LocalizedName(CalendarStyle c) =>
        _rosetta.GetText(RbFile.Localizable, c == CalendarStyle.Gregorian
            ? "enum.calendarstyle.gregorian" : "enum.calendarstyle.julian");
    public string LocalizedName(YearCount y) =>
        _rosetta.GetText(RbFile.Localizable, y switch {
            YearCount.CE           => "enum.yearcount.ce",
            YearCount.BCE          => "enum.yearcount.bce",
            YearCount.Astronomical => "enum.yearcount.astronomical",
            _                      => "enum.yearcount.ce"
        });
    public string LocalizedName(ObserverPositions o) =>
        _rosetta.GetText(RbFile.Localizable, o switch {
            ObserverPositions.Geocentric   => "enum.observerpos.geocentric",
            ObserverPositions.Heliocentric => "enum.observerpos.heliocentric",
            _                              => "enum.observerpos.geocentric"
        });
    public string LocalizedName(Coordinates coord) =>
        _rosetta.GetText(RbFile.Localizable, coord.LocalizedName());

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool CanCalculate =>
        !HasStartDateError && !HasEndDateError && !HasDateOrderError &&
        (_jupiterSelected || _saturnSelected || _uranusSelected);

    private void Validate()
    {
        StartDateError = ValidateDate(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar)
            ? null : T("view.waves.validation.invalidyear");
        EndDateError   = ValidateDate(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar)
            ? null : T("view.waves.validation.invalidyear");

        if (StartDateError is null && EndDateError is null)
        {
            var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
            var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
            DateOrderError = jdEnd > jdStart ? null : T("view.waves.validation.enddateorder");
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

    private void Calculate()
    {
        var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
        var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);

        var allResults     = new Dictionary<Factors, List<(double, double)>>();
        var selectedFactors = new List<Factors>();

        foreach (var (factor, selected) in new[]
        {
            (Factors.Jupiter, _jupiterSelected),
            (Factors.Saturn,  _saturnSelected),
            (Factors.Uranus,  _uranusSelected)
        })
        {
            if (!selected) continue;
            selectedFactors.Add(factor);
            var req = new WavesRequest(factor, WaveIntervals[factor], jdStart, jdEnd,
                _selectedCoordinate, _observerPosition);
            allResults[factor] = WavesCalculator.PerformCalculation(req);
        }

        _model.SelectedFactors = selectedFactors;
        _model.AllResults      = allResults;
    }

    private string T(string key) => _rosetta.GetText(RbFile.Localizable, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
