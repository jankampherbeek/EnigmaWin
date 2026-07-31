// MonthlyEphemerisInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Periods.MonthlyEphemeris.UI;

public sealed class MonthlyEphemerisInputViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly MonthlyEphemerisModel _model;
    private readonly IConfigContext _configContext;

    private int _selectedMonth;
    private string _yearText = DateTime.Today.Year.ToString();
    private Ayanamshas _ayanamsha = Ayanamshas.Tropical;
    private string? _yearError;

    private readonly ObservableCollection<EphemerisFactorItem> _factorItems = [];

    public MonthlyEphemerisInputViewModel(IRosetta rosetta, MonthlyEphemerisModel model, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _model         = model;
        _configContext = configContext;

        _selectedMonth = DateTime.Today.Month;

        RebuildFactorList();
        Recalculate();
    }

    // ── Bound collections ─────────────────────────────────────────────────────

    public ObservableCollection<EphemerisFactorItem> FactorItems => _factorItems;

    public static List<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IEnumerable<Ayanamshas> AllAyanamshas => Enum.GetValues<Ayanamshas>();

    // ── Input properties ──────────────────────────────────────────────────────

    public int SelectedMonth
    {
        get => _selectedMonth;
        set { _selectedMonth = value; OnPropertyChanged(); Recalculate(); }
    }

    public string YearText
    {
        get => _yearText;
        set
        {
            _yearText = value;
            OnPropertyChanged();
            YearError = int.TryParse(value, out var y) && y >= -9999 && y <= 9999
                ? null : T("view.ephemeris.validation.invalidyear");
            Recalculate();
        }
    }

    public Ayanamshas Ayanamsha
    {
        get => _ayanamsha;
        set { _ayanamsha = value; _model.Ayanamsha = value; OnPropertyChanged(); Recalculate(); }
    }

    // ── Observer position ──────────────────────────────────────────────────────

    public bool IsGeocentric
    {
        get => !_model.IsHeliocentric;
        set
        {
            if (value) { _model.IsHeliocentric = false; OnPropertyChanged(); OnPropertyChanged(nameof(IsHeliocentric)); Recalculate(); }
        }
    }

    public bool IsHeliocentric
    {
        get => _model.IsHeliocentric;
        set
        {
            if (value) { _model.IsHeliocentric = true; OnPropertyChanged(); OnPropertyChanged(nameof(IsGeocentric)); Recalculate(); }
        }
    }

    // ── Coordinate single-selection ───────────────────────────────────────────

    public bool CoordLongitude
    {
        get => _model.SelectedCoord == EphemerisCoordinate.Longitude;
        set { if (value) SetCoord(EphemerisCoordinate.Longitude); }
    }

    public bool CoordLatitude
    {
        get => _model.SelectedCoord == EphemerisCoordinate.Latitude;
        set { if (value) SetCoord(EphemerisCoordinate.Latitude); }
    }

    public bool CoordRa
    {
        get => _model.SelectedCoord == EphemerisCoordinate.RightAscension;
        set { if (value) SetCoord(EphemerisCoordinate.RightAscension); }
    }

    public bool CoordDeclination
    {
        get => _model.SelectedCoord == EphemerisCoordinate.Declination;
        set { if (value) SetCoord(EphemerisCoordinate.Declination); }
    }

    public bool CoordDistance
    {
        get => _model.SelectedCoord == EphemerisCoordinate.Distance;
        set { if (value) SetCoord(EphemerisCoordinate.Distance); }
    }

    public bool CoordSpeedLon
    {
        get => _model.SelectedCoord == EphemerisCoordinate.SpeedLongitude;
        set { if (value) SetCoord(EphemerisCoordinate.SpeedLongitude); }
    }

    public bool CoordSpeedDec
    {
        get => _model.SelectedCoord == EphemerisCoordinate.SpeedDeclination;
        set { if (value) SetCoord(EphemerisCoordinate.SpeedDeclination); }
    }

    private void SetCoord(EphemerisCoordinate coord)
    {
        _model.SelectedCoord = coord;
        OnPropertyChanged(nameof(CoordLongitude));
        OnPropertyChanged(nameof(CoordLatitude));
        OnPropertyChanged(nameof(CoordRa));
        OnPropertyChanged(nameof(CoordDeclination));
        OnPropertyChanged(nameof(CoordDistance));
        OnPropertyChanged(nameof(CoordSpeedLon));
        OnPropertyChanged(nameof(CoordSpeedDec));
        Recalculate();
    }

    // ── Validation ────────────────────────────────────────────────────────────

    public string? YearError
    {
        get => _yearError;
        private set { _yearError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasYearError)); }
    }
    public bool HasYearError => _yearError is not null;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle        => T("view.ephemeris.title");
    public string LabelMonth        => T("view.ephemeris.month");
    public string LabelYear         => T("view.ephemeris.year");
    public string LabelAyanamsha    => T("view.ephemeris.ayanamsha");
    public string LabelObserver     => T("view.ephemeris.observer");
    public string LabelGeocentric   => T("view.ephemeris.geocentric");
    public string LabelHeliocentric => T("view.ephemeris.heliocentric");
    public string LabelCoordinates  => T("view.ephemeris.coordinates");
    public string LabelLongitude    => T("view.ephemeris.longitude");
    public string LabelLatitude     => T("view.ephemeris.latitude");
    public string LabelRa           => T("view.ephemeris.ra");
    public string LabelDeclination  => T("view.ephemeris.declination");
    public string LabelDistance     => T("view.ephemeris.distance");
    public string LabelSpeedLon     => T("view.ephemeris.speedlon");
    public string LabelSpeedDec     => T("view.ephemeris.speeddec");
    public string LabelFactors      => T("view.ephemeris.factors");

    public string LocalizedName(Ayanamshas a) =>
        _rosetta.GetText(RbFile.Localizable, a.LocalizedName());

    // ── Notification for factor changes ──────────────────────────────────────

    public void OnFactorSelectionChanged() => Recalculate();

    // ── Private ───────────────────────────────────────────────────────────────

    private void RebuildFactorList()
    {
        var configured = _configContext.ActiveConfig.FactorConfig.Settings
            .Where(s => s.IsUsed)
            .Select(s => s.Factor)
            .ToHashSet();

        _factorItems.Clear();
        foreach (var f in Enum.GetValues<Factors>())
        {
            var ct = f.CalculationType();
            if (ct is CalculationTypes.Mundane or CalculationTypes.Lots
                    or CalculationTypes.ZodiacFixed or CalculationTypes.Unknown) continue;

            var isSelected = configured.Contains(f);
            _factorItems.Add(new EphemerisFactorItem(
                f, _rosetta.GetText(RbFile.Localizable, f.LocalizedName()), isSelected, this));
        }
    }

    private void Recalculate()
    {
        if (!int.TryParse(_yearText, out var year)) return;
        if (_yearError is not null) return;
        if (_selectedMonth < 1 || _selectedMonth > 12) return;

        var factors = _factorItems.Where(f => f.IsSelected).Select(f => f.Factor).ToList();
        if (factors.Count == 0)
        {
            _model.Rows    = [];
            _model.Factors = [];
            return;
        }

        var observerPos = _model.IsHeliocentric
            ? ObserverPositions.Heliocentric
            : ObserverPositions.Geocentric;

        try
        {
            var rows = MonthlyEphemerisCalculator.Calculate(year, _selectedMonth, factors, _ayanamsha, observerPos);
            _model.Factors = factors;
            _model.Rows    = rows;
        }
        catch
        {
            _model.Rows    = [];
            _model.Factors = [];
        }
    }

    private string T(string key) => _rosetta.GetText(RbFile.MonthlyEphemeris, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class EphemerisFactorItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private readonly MonthlyEphemerisInputViewModel _owner;

    public Factors Factor      { get; }
    public string  DisplayName { get; }

    public EphemerisFactorItem(Factors factor, string displayName, bool isSelected, MonthlyEphemerisInputViewModel owner)
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
