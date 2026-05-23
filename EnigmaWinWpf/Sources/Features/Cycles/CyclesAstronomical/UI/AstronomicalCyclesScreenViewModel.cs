// AstronomicalCyclesScreenViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical.UI;

public sealed class AstronomicalCyclesScreenViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly AstronomicalCyclesModel _model;

    // ── Date fields ───────────────────────────────────────────────────────────

    private string _startYearText = "2024";
    private int _startMonth = 1;
    private int _startDay = 1;
    private CalendarStyle _startCalendar = CalendarStyle.Gregorian;
    private YearCount _startYearCount = YearCount.CE;

    private string _endYearText = "2025";
    private int _endMonth = 12;
    private int _endDay = 31;
    private CalendarStyle _endCalendar = CalendarStyle.Gregorian;
    private YearCount _endYearCount = YearCount.CE;

    // ── Calculation parameters ────────────────────────────────────────────────

    private ObserverPositions _observerPosition = ObserverPositions.Geocentric;
    private Coordinates _selectedCoordinate = Coordinates.Longitude;
    private Ayanamshas _ayanamsha = Ayanamshas.Tropical;
    private bool _isPairsMode;

    // ── Factor selection ──────────────────────────────────────────────────────

    private readonly ObservableCollection<FactorItem> _factorItems = [];
    private FactorChoiceItem? _pendingChoiceA;
    private FactorChoiceItem? _pendingChoiceB;
    private readonly ObservableCollection<FactorPairItem> _factorPairs = [];

    private string? _periodWarning;
    private string? _startDateError;
    private string? _endDateError;
    private string? _dateOrderError;

    private const int MaxSingleFactors = 12;
    private const int MaxPairs = 6;

    private static readonly HashSet<Factors> HelioExcluded =
    [
        Factors.Sun, Factors.Moon, Factors.NorthNodeMean, Factors.NorthNodeTrue,
        Factors.ApogeeMean, Factors.ApogeeCorrected, Factors.ApogeeInterpolated,
        Factors.PerigeeInterpolated, Factors.Priapus, Factors.PriapusCorrected,
        Factors.Dragon, Factors.Beast, Factors.SouthNodeMean, Factors.SouthNodeTrue,
        Factors.BlackSun, Factors.Diamond
    ];

    public AstronomicalCyclesScreenViewModel(IRosetta rosetta, AstronomicalCyclesModel model)
    {
        _rosetta = rosetta;
        _model   = model;

        CalculateCommand = new RelayCommand(Calculate, () => CanCalculate);
        AddPairCommand   = new RelayCommand(AddPair,   () => CanAddPair);

        RebuildFactorList();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public IRelayCommand CalculateCommand { get; }
    public IRelayCommand AddPairCommand   { get; }

    // ── Bound collections ─────────────────────────────────────────────────────

    public ObservableCollection<FactorItem>     FactorItems  => _factorItems;
    public ObservableCollection<FactorPairItem> FactorPairs  => _factorPairs;
    public IEnumerable<CalendarStyle>  AllCalendarStyles  => Enum.GetValues<CalendarStyle>();
    public IEnumerable<YearCount>      AllYearCounts      => Enum.GetValues<YearCount>();
    public IEnumerable<ObserverPositions> AllObserverPositions =>
        Enum.GetValues<ObserverPositions>().Where(p => p != ObserverPositions.Topocentric);
    public IEnumerable<Coordinates>    AvailableCoordinates =>
        _observerPosition == ObserverPositions.Heliocentric
            ? [Coordinates.Longitude, Coordinates.Latitude, Coordinates.Distance]
            : Enum.GetValues<Coordinates>();
    public IEnumerable<Ayanamshas>     AllAyanamshas      => Enum.GetValues<Ayanamshas>();
    public IEnumerable<FactorChoiceItem> AllFactorChoices =>
        _factorItems.Select(f => new FactorChoiceItem(f.Factor, f.DisplayName)).Prepend(new FactorChoiceItem(null, "—"));

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
            RebuildFactorList();
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

    public Ayanamshas Ayanamsha
    {
        get => _ayanamsha;
        set { _ayanamsha = value; OnPropertyChanged(); }
    }

    public bool IsPairsMode
    {
        get => _isPairsMode;
        set
        {
            _isPairsMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSingleMode));
            Validate();
        }
    }
    public bool IsSingleMode => !_isPairsMode;

    // ── Factor-pair pending selection ─────────────────────────────────────────

    public FactorChoiceItem? PendingChoiceA
    {
        get => _pendingChoiceA;
        set { _pendingChoiceA = value; OnPropertyChanged(); AddPairCommand.NotifyCanExecuteChanged(); }
    }

    public FactorChoiceItem? PendingChoiceB
    {
        get => _pendingChoiceB;
        set { _pendingChoiceB = value; OnPropertyChanged(); AddPairCommand.NotifyCanExecuteChanged(); }
    }

    // ── Validation feedback ───────────────────────────────────────────────────

    public string? PeriodWarning
    {
        get => _periodWarning;
        private set { _periodWarning = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPeriodWarning)); }
    }
    public bool HasPeriodWarning => _periodWarning is not null;

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

    public string LabelTitle           => T("view.astrocycles.title");
    public string LabelStartDate       => T("view.astrocycles.startdate");
    public string LabelEndDate         => T("view.astrocycles.enddate");
    public string LabelYear            => T("view.astrocycles.year");
    public string LabelMonth           => T("view.astrocycles.month");
    public string LabelDay             => T("view.astrocycles.day");
    public string LabelCalendar        => T("view.astrocycles.calendar");
    public string LabelYearCount       => T("view.astrocycles.yearcount");
    public string LabelCoordinate      => T("view.astrocycles.coordinate");
    public string LabelObserverPos     => T("view.astrocycles.observerposition");
    public string LabelAyanamsha       => T("view.astrocycles.ayanamsha");
    public string LabelSingleFactors   => T("view.astrocycles.singlefactors");
    public string LabelPairsOfFactors  => T("view.astrocycles.pairsoffactors");
    public string LabelFactorA         => T("view.astrocycles.factora");
    public string LabelFactorB         => T("view.astrocycles.factorb");
    public string LabelAddPair         => T("view.astrocycles.addpair");
    public string LabelAddedPairs      => T("view.astrocycles.addedpairs");
    public string LabelCalculate       => T("view.astrocycles.calculate");

    public string SelectedCountLabel =>
        string.Format(T("view.astrocycles.selectedcount"),
            _factorItems.Count(f => f.IsSelected), MaxSingleFactors);

    public string LocalizedName(Factors f) =>
        _rosetta.GetText(RbFile.Localizable, f.LocalizedName());
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
    public string LocalizedName(Ayanamshas a) =>
        _rosetta.GetText(RbFile.Localizable, a.LocalizedName());

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool CanCalculate
    {
        get
        {
            if (StartAstrYear() is null || EndAstrYear() is null) return false;
            if (HasStartDateError || HasEndDateError || HasDateOrderError) return false;
            return _isPairsMode ? _factorPairs.Count > 0 : _factorItems.Any(f => f.IsSelected);
        }
    }

    private bool CanAddPair
    {
        get
        {
            if (_pendingChoiceA?.Factor is null || _pendingChoiceB?.Factor is null) return false;
            if (_pendingChoiceA.Factor == _pendingChoiceB.Factor) return false;
            if (_factorPairs.Count >= MaxPairs) return false;
            var f1 = _pendingChoiceA.Factor.Value;
            var f2 = _pendingChoiceB.Factor.Value;
            return !_factorPairs.Any(p => p.Factor1 == f1 && p.Factor2 == f2);
        }
    }

    private void RebuildFactorList()
    {
        var excluded = _observerPosition == ObserverPositions.Heliocentric
            ? HelioExcluded : new HashSet<Factors> { Factors.Earth };

        _factorItems.Clear();
        foreach (var f in Enum.GetValues<Factors>())
        {
            var ct = f.CalculationType();
            if (ct is CalculationTypes.Mundane or CalculationTypes.Lots
                    or CalculationTypes.ZodiacFixed or CalculationTypes.Unknown) continue;
            if (excluded.Contains(f)) continue;
            _factorItems.Add(new FactorItem(f, LocalizedName(f), this));
        }

        var toRemove = _factorPairs.Where(p =>
            excluded.Contains(p.Factor1) || excluded.Contains(p.Factor2)).ToList();
        foreach (var item in toRemove) _factorPairs.Remove(item);
        if (_pendingChoiceA?.Factor.HasValue == true && excluded.Contains(_pendingChoiceA.Factor!.Value)) PendingChoiceA = null;
        if (_pendingChoiceB?.Factor.HasValue == true && excluded.Contains(_pendingChoiceB.Factor!.Value)) PendingChoiceB = null;

        OnPropertyChanged(nameof(AllFactorChoices));
        OnPropertyChanged(nameof(SelectedCountLabel));
        Validate();
    }

    private void Validate()
    {
        StartDateError  = ValidateDate(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar)
                            ? null : T("view.astrocycles.validation.invalidyear");
        EndDateError    = ValidateDate(_endYearText, _endYearCount, _endMonth, _endDay, _endCalendar)
                            ? null : T("view.astrocycles.validation.invalidyear");

        if (StartDateError is null && EndDateError is null)
        {
            var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
            var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
            DateOrderError = jdEnd > jdStart ? null : T("view.astrocycles.validation.enddateorder");
        }
        else
        {
            DateOrderError = null;
        }

        CalculateCommand.NotifyCanExecuteChanged();
        AddPairCommand.NotifyCanExecuteChanged();
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

    private int? StartAstrYear() =>
        int.TryParse(_startYearText, out var y) ? ToAstrYear(y, _startYearCount) : null;
    private int? EndAstrYear() =>
        int.TryParse(_endYearText, out var y) ? ToAstrYear(y, _endYearCount) : null;

    private void AddPair()
    {
        if (_pendingChoiceA?.Factor is null || _pendingChoiceB?.Factor is null) return;
        var f1 = _pendingChoiceA.Factor.Value;
        var f2 = _pendingChoiceB.Factor.Value;
        _factorPairs.Add(new FactorPairItem(f1, f2,
            $"{LocalizedName(f1)} ↔ {LocalizedName(f2)}", this));
        PendingChoiceA = null;
        PendingChoiceB = null;
        OnPropertyChanged(nameof(SelectedCountLabel));
        Validate();
    }

    public void RemovePair(FactorPairItem pair)
    {
        _factorPairs.Remove(pair);
        Validate();
    }

    public void OnFactorSelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCountLabel));
        Validate();
    }

    private void Calculate()
    {
        PeriodWarning = null;
        _model.SingleResults = [];
        _model.PairResults   = [];
        _model.FactorPairs   = [];

        var allFactors = _isPairsMode
            ? _factorPairs.SelectMany(p => new[] { p.Factor1, p.Factor2 }).Distinct().ToList()
            : _factorItems.Where(f => f.IsSelected).Select(f => f.Factor).ToList();

        var specs = allFactors.Select(f => f.CycleSpec()).OfType<FactorCycleSpec>().ToList();
        if (specs.Count == 0) return;

        var smallestInterval  = specs.Min(s => s.Interval);
        var smallestMaxPeriod = specs.Min(s => s.MaxPeriod);

        var jdStart = ToJd(_startYearText, _startYearCount, _startMonth, _startDay, _startCalendar);
        var jdEnd   = ToJd(_endYearText,   _endYearCount,   _endMonth,   _endDay,   _endCalendar);
        var periodDays = (int)Math.Round(jdEnd - jdStart);

        if (periodDays > smallestMaxPeriod)
        {
            PeriodWarning = string.Format(T("view.astrocycles.warning.maxperiod"), smallestMaxPeriod, periodDays);
            return;
        }

        _model.IsPairs     = _isPairsMode;
        _model.Coordinate  = _selectedCoordinate;

        if (!_isPairsMode)
        {
            var results = new List<(Factors, List<(double, double)>)>();
            foreach (var factor in _factorItems.Where(f => f.IsSelected).Select(f => f.Factor))
            {
                var req = new PeriodCalculatorRequest(
                    factor, smallestInterval, jdStart, jdEnd,
                    _selectedCoordinate, _ayanamsha, _observerPosition);
                results.Add((factor, PeriodCalculator.PerformCalculation(req)));
            }
            _model.SingleResults = results;
        }
        else
        {
            var pairs = _factorPairs.Select(p => (p.Factor1, p.Factor2)).ToList();
            _model.FactorPairs = pairs;

            var pairResults = new List<List<(double, double)>>();
            foreach (var (f1, f2) in pairs)
            {
                var req = new PeriodDifferenceRequest(
                    f1, f2, smallestInterval, jdStart, jdEnd,
                    _selectedCoordinate, _ayanamsha, _observerPosition);
                pairResults.Add(PeriodDifference.PerformCalculation(req));
            }
            _model.PairResults = pairResults;
        }
    }

    private string T(string key) => _rosetta.GetText(RbFile.Localizable, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ── Helper items for collections ──────────────────────────────────────────────

public sealed class FactorItem(Factors factor, string displayName, AstronomicalCyclesScreenViewModel owner)
    : INotifyPropertyChanged
{
    private bool _isSelected;
    public Factors Factor      { get; } = factor;
    public string  DisplayName { get; } = displayName;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            owner.OnFactorSelectionChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class FactorPairItem
{
    public Factors Factor1     { get; }
    public Factors Factor2     { get; }
    public string  DisplayName { get; }
    public IRelayCommand RemoveCommand { get; }

    public FactorPairItem(Factors factor1, Factors factor2, string displayName, AstronomicalCyclesScreenViewModel owner)
    {
        Factor1 = factor1;
        Factor2 = factor2;
        DisplayName = displayName;
        RemoveCommand = new RelayCommand(() => owner.RemovePair(this));
    }
}

public sealed class FactorChoiceItem(Factors? factor, string displayName)
{
    public Factors? Factor      { get; } = factor;
    public string   DisplayName { get; } = displayName;
}
