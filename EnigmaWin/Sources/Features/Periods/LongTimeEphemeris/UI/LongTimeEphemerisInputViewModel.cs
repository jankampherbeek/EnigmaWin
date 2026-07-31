// LongTimeEphemerisInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris.UI;

public sealed class LongTimeEphemerisInputViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly LongTimeEphemerisModel _model;
    private readonly IConfigContext _configContext;

    // ── Start date ────────────────────────────────────────────────────────────

    private string _startYearText = "2000";
    private int _startMonth = 1;
    private int _startDay = 1;
    private CalendarStyle _startCalendar = CalendarStyle.Gregorian;
    private YearCount _startYearCount = YearCount.CE;

    // ── End date ──────────────────────────────────────────────────────────────

    private string _endYearText = "2025";
    private int _endMonth = 12;
    private int _endDay = 31;
    private CalendarStyle _endCalendar = CalendarStyle.Gregorian;
    private YearCount _endYearCount = YearCount.CE;

    // ── Interval ──────────────────────────────────────────────────────────────

    private int _intervalDays = 1;
    private int _intervalHours;

    // ── Calculation parameters ───────────────────────────────────────────────

    private ObserverPositions _observerPosition = ObserverPositions.Geocentric;
    private Ayanamshas _ayanamsha = Ayanamshas.Tropical;
    private LongTimeEphemerisCoordinate _selectedCoordinate = LongTimeEphemerisCoordinate.Longitude;
    private LongTimeEphemerisDisplayFormat _displayFormat = LongTimeEphemerisDisplayFormat.Dms;

    private readonly ObservableCollection<LongTimeEphemerisFactorItem> _factorItems = [];

    // ── Progress state ────────────────────────────────────────────────────────

    private bool _isCalculating;
    private double _progressFraction;
    private CancellationTokenSource? _cts;

    public LongTimeEphemerisInputViewModel(IRosetta rosetta, LongTimeEphemerisModel model, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _model         = model;
        _configContext = configContext;

        CalculateCommand = new AsyncRelayCommand(CalculateAsync, () => CanCalculate);
        CancelCommand     = new RelayCommand(Cancel);

        RebuildFactorList();
        Validate();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IAsyncRelayCommand CalculateCommand { get; }
    public IRelayCommand CancelCommand { get; }

    // ── Bound collections ─────────────────────────────────────────────────────

    public static List<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public static List<int> DayValues   { get; } = Enumerable.Range(1, 31).ToList();
    public IEnumerable<CalendarStyle> AllCalendarStyles => Enum.GetValues<CalendarStyle>();
    public IEnumerable<YearCount> AllYearCounts => Enum.GetValues<YearCount>();
    public IEnumerable<ObserverPositions> AllObserverPositions =>
        Enum.GetValues<ObserverPositions>().Where(p => p != ObserverPositions.Topocentric);
    public IEnumerable<Ayanamshas> AllAyanamshas => Enum.GetValues<Ayanamshas>();
    public ObservableCollection<LongTimeEphemerisFactorItem> FactorItems => _factorItems;

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

    // ── Interval properties ──────────────────────────────────────────────────

    public int IntervalDays
    {
        get => _intervalDays;
        set { _intervalDays = value; OnPropertyChanged(); Validate(); }
    }
    public int IntervalHours
    {
        get => _intervalHours;
        set { _intervalHours = value; OnPropertyChanged(); Validate(); }
    }

    // ── Calculation parameter properties ─────────────────────────────────────

    public ObserverPositions ObserverPosition
    {
        get => _observerPosition;
        set
        {
            _observerPosition = value;
            OnPropertyChanged();
            RebuildFactorList();
        }
    }

    public Ayanamshas Ayanamsha
    {
        get => _ayanamsha;
        set { _ayanamsha = value; OnPropertyChanged(); }
    }

    public LongTimeEphemerisCoordinate SelectedCoordinate
    {
        get => _selectedCoordinate;
        set { _selectedCoordinate = value; OnPropertyChanged(); }
    }

    public IEnumerable<LongTimeEphemerisCoordinate> AllCoordinates => Enum.GetValues<LongTimeEphemerisCoordinate>();

    public LongTimeEphemerisDisplayFormat DisplayFormat
    {
        get => _displayFormat;
        set
        {
            _displayFormat = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsFormatDms));
            OnPropertyChanged(nameof(IsFormatDecimal));
        }
    }

    public bool IsFormatDms
    {
        get => _displayFormat == LongTimeEphemerisDisplayFormat.Dms;
        set { if (value) DisplayFormat = LongTimeEphemerisDisplayFormat.Dms; }
    }

    public bool IsFormatDecimal
    {
        get => _displayFormat == LongTimeEphemerisDisplayFormat.Decimal;
        set { if (value) DisplayFormat = LongTimeEphemerisDisplayFormat.Decimal; }
    }

    // ── Progress state ────────────────────────────────────────────────────────

    public bool IsCalculating
    {
        get => _isCalculating;
        private set { _isCalculating = value; OnPropertyChanged(); OnPropertyChanged(nameof(ShowCalculateButton)); }
    }
    public bool ShowCalculateButton => !_isCalculating;

    public double ProgressFraction
    {
        get => _progressFraction;
        private set { _progressFraction = value; OnPropertyChanged(); }
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private string? _startDateError;
    private string? _endDateError;
    private string? _dateOrderError;
    private string? _intervalError;

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

    public string? IntervalError
    {
        get => _intervalError;
        private set { _intervalError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasIntervalError)); }
    }
    public bool HasIntervalError => _intervalError is not null;

    public int? EstimatedRowCount
    {
        get
        {
            if (HasStartDateError || HasEndDateError || HasDateOrderError || HasIntervalError) return null;
            var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
            var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
            var interval = IntervalInDays;
            if (interval <= 0) return null;
            return (int)((jdEnd - jdStart) / interval) + 1;
        }
    }

    public bool RowCountExceedsLimit => EstimatedRowCount is { } count && count > LongTimeEphemerisModel.MaxRows;
    public bool HasEstimatedRowCount => EstimatedRowCount is not null;

    public string EstimatedRowsText => EstimatedRowCount is { } count
        ? $"{T("view.ltephemeris.estimatedrows")} {count:N0}"
        : string.Empty;

    public string TooManyRowsText => EstimatedRowCount is { } count
        ? string.Format(T("view.ltephemeris.toomanyrows"), count.ToString("N0"), LongTimeEphemerisModel.MaxRows.ToString("N0"))
        : string.Empty;

    private double IntervalInDays => _intervalDays + _intervalHours / 24.0;

    private bool CanCalculate =>
        !HasStartDateError && !HasEndDateError && !HasDateOrderError && !HasIntervalError &&
        _factorItems.Any(f => f.IsSelected) && !RowCountExceedsLimit;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle            => T("view.ltephemeris.title");
    public string LabelStartDate        => T("view.ltephemeris.startdate");
    public string LabelEndDate          => T("view.ltephemeris.enddate");
    public string LabelInterval         => T("view.ltephemeris.interval");
    public string LabelIntervalDays     => T("view.ltephemeris.interval.days");
    public string LabelIntervalHours    => T("view.ltephemeris.interval.hours");
    public string LabelObserverPosition => T("view.ltephemeris.observerposition");
    public string LabelAyanamsha        => T("view.ltephemeris.ayanamsha");
    public string LabelCoordinate       => T("view.ltephemeris.coordinate");
    public string LabelFormat           => T("view.ltephemeris.format");
    public string LabelFormatDms        => T("view.ltephemeris.format.dms");
    public string LabelFormatDecimal    => T("view.ltephemeris.format.decimal");
    public string LabelFactors          => T("view.ltephemeris.factors");
    public string LabelCalculate        => T("view.ltephemeris.calculate");
    public string LabelCancel           => T("view.ltephemeris.cancel");
    public string LabelCalculating      => T("view.ltephemeris.calculating");
    public string LabelYear             => T("view.ltephemeris.year");
    public string LabelMonth            => T("view.ltephemeris.month");
    public string LabelDay              => T("view.ltephemeris.day");
    public string LabelCalendar         => T("view.ltephemeris.calendar");
    public string LabelYearCount        => T("view.ltephemeris.yearcount");

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
    public string LocalizedName(ObserverPositions p) =>
        _rosetta.GetText(RbFile.Localizable, p.LocalizedName());
    public string LocalizedName(Ayanamshas a) =>
        _rosetta.GetText(RbFile.Localizable, a.LocalizedName());
    public string LocalizedName(LongTimeEphemerisCoordinate c) => c switch
    {
        LongTimeEphemerisCoordinate.Longitude        => T("view.ephemeris.longitude", RbFile.MonthlyEphemeris),
        LongTimeEphemerisCoordinate.Latitude         => T("view.ephemeris.latitude", RbFile.MonthlyEphemeris),
        LongTimeEphemerisCoordinate.RightAscension   => T("view.ephemeris.ra", RbFile.MonthlyEphemeris),
        LongTimeEphemerisCoordinate.Declination      => T("view.ephemeris.declination", RbFile.MonthlyEphemeris),
        LongTimeEphemerisCoordinate.Distance         => T("view.ephemeris.distance", RbFile.MonthlyEphemeris),
        LongTimeEphemerisCoordinate.SpeedLongitude   => T("view.ephemeris.speedlon", RbFile.MonthlyEphemeris),
        LongTimeEphemerisCoordinate.SpeedDeclination => T("view.ephemeris.speeddec", RbFile.MonthlyEphemeris),
        _                                             => string.Empty
    };

    // ── Notification for factor changes ──────────────────────────────────────

    public void OnFactorSelectionChanged() => CalculateCommand.NotifyCanExecuteChanged();

    // ── Calculate ─────────────────────────────────────────────────────────────

    private async Task CalculateAsync()
    {
        if (!CanCalculate) return;

        var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
        var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
        var factors = _factorItems.Where(f => f.IsSelected).Select(f => f.Factor).ToList();

        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsCalculating    = true;
        ProgressFraction = 0;
        _model.Rows       = [];

        var progress = new Progress<LongTimeEphemerisProgress>(p => ProgressFraction = p.Fraction);

        try
        {
            var rows = await LongTimeEphemerisOrchestrator.RunAsync(
                jdStart, jdEnd, IntervalInDays, factors, _selectedCoordinate,
                _observerPosition, _ayanamsha, progress, ct);

            _model.Factors             = factors;
            _model.SelectedCoordinate  = _selectedCoordinate;
            _model.DisplayFormat       = _displayFormat;
            _model.Rows                = rows;
        }
        catch (OperationCanceledException)
        {
            _model.Rows = [];
        }
        finally
        {
            IsCalculating = false;
        }
    }

    private void Cancel() => _cts?.Cancel();

    // ── Private ───────────────────────────────────────────────────────────────

    private void RebuildFactorList()
    {
        var configured = _configContext.ActiveConfig.FactorConfig.Settings
            .Where(s => s.IsUsed)
            .Select(s => s.Factor)
            .ToHashSet();

        var available = LongTimeEphemerisCalculator.AvailableFactors(_observerPosition).ToHashSet();
        var previouslySelected = _factorItems.Where(f => f.IsSelected).Select(f => f.Factor).ToHashSet();
        var isFirstBuild = _factorItems.Count == 0;

        _factorItems.Clear();
        foreach (var f in LongTimeEphemerisCalculator.AvailableFactors(_observerPosition))
        {
            var isSelected = isFirstBuild ? configured.Contains(f) : previouslySelected.Contains(f);
            _factorItems.Add(new LongTimeEphemerisFactorItem(
                f, _rosetta.GetText(RbFile.Localizable, f.LocalizedName()), isSelected, this));
        }

        OnPropertyChanged(nameof(FactorItems));
        CalculateCommand.NotifyCanExecuteChanged();
    }

    private void Validate()
    {
        StartDateError = ValidateDate(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar)
            ? null : T("view.ltephemeris.validation.invalidyear");
        EndDateError = ValidateDate(_endYearText, _endYearCount, _endMonth, _endDay, _endCalendar)
            ? null : T("view.ltephemeris.validation.invalidyear");

        if (StartDateError is null && EndDateError is null)
        {
            var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
            var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
            DateOrderError = jdEnd > jdStart ? null : T("view.ltephemeris.validation.enddateorder");
        }
        else
        {
            DateOrderError = null;
        }

        IntervalError = IntervalInDays > 0 ? null : T("view.ltephemeris.validation.intervalzero");

        OnPropertyChanged(nameof(EstimatedRowCount));
        OnPropertyChanged(nameof(EstimatedRowsText));
        OnPropertyChanged(nameof(HasEstimatedRowCount));
        OnPropertyChanged(nameof(RowCountExceedsLimit));
        OnPropertyChanged(nameof(TooManyRowsText));
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

    private string T(string key) => _rosetta.GetText(RbFile.LongTimeEphemeris, key);
    private string T(string key, RbFile file) => _rosetta.GetText(file, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class LongTimeEphemerisFactorItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private readonly LongTimeEphemerisInputViewModel _owner;

    public Factors Factor      { get; }
    public string  DisplayName { get; }

    public LongTimeEphemerisFactorItem(Factors factor, string displayName, bool isSelected, LongTimeEphemerisInputViewModel owner)
    {
        Factor      = factor;
        DisplayName = displayName;
        _isSelected = isSelected;
        _owner      = owner;
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            _owner.OnFactorSelectionChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
