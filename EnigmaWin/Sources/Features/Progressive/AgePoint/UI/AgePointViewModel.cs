// AgePointViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

public enum AgePointMode { Overview = 0, PositionsForEvent = 1 }

public partial class AgePointViewModel : ObservableObject
{
    private const double TropicalYear = 365.242199074;

    private readonly AgePointOrchestrator  _apOrchestrator;
    private readonly IConfigContext        _configContext;
    private readonly IChartSession         _chartSession;
    private readonly IProgressiveSession   _progressiveSession;
    private readonly IHoroscopeRepository  _horoscopeRepository;
    private readonly EventsOrchestrator    _eventsOrchestrator;
    private readonly IRosetta              _rosetta;

    // ── Observable state ────────────────────────────────────────────────────

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEvents))]
    private ObservableCollection<EventRow> _eventRows = [];

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<AgePointOverviewRow> _positionRows = [];

    [ObservableProperty]
    private ObservableCollection<TransitSecDir.UI.TransitMatchRow> _matchRows = [];

    [ObservableProperty]
    private ObservableCollection<TransitSecDir.UI.ProgressiveMidpointRow> _radixMidpointRows = [];
    [ObservableProperty]
    private ObservableCollection<TransitSecDir.UI.ProgressiveMidpointRow> _progressiveMidpointRows = [];
    [ObservableProperty] private IReadOnlyList<MidpointMatch> _radixMidpointMatches       = [];
    [ObservableProperty] private IReadOnlyList<MidpointMatch> _progressiveMidpointMatches  = [];
    [ObservableProperty] private MidpointDialType             _midpointDialType = MidpointDialType.Dial360;

    [ObservableProperty] private ChartEvent?    _selectedEvent;
    [ObservableProperty] private NamedChart?    _selectedChart;
    [ObservableProperty] private string         _errorMessage = string.Empty;
    [ObservableProperty] private bool           _hasError;
    [ObservableProperty] private int            _activeTab;     // 0=Positions, 1=Matches, 2=Midpoints, 3=Wheel
    [ObservableProperty] private bool           _isBlackWhite = false;
    [ObservableProperty] private bool           _hideAspects  = false;
    [ObservableProperty] private AgePointMode   _mode = AgePointMode.Overview;
    [ObservableProperty] private NamedHouseSystem? _selectedNamedHouseSystem;

    // Wheel data
    [ObservableProperty] private WheelPlotData        _radixPlotData   = WheelPlotData.Empty;
    [ObservableProperty] private double?              _apLongitude;        // single position (PositionsForEvent)
    [ObservableProperty] private AgePointWheelMark[]  _apOverviewMarks = [];

    private List<ChartEvent>                          _rawEvents  = [];
    private FullChart?                                _radixChart;
    private Horoscope?                                _radixHoroscope;
    private Dictionary<Factors, ProgressivePosition>  _results    = [];

    // ── Derived ─────────────────────────────────────────────────────────────

    public bool HasEvents   => EventRows.Count > 0;
    public bool HasResults  => PositionRows.Count > 0 || _results.Count > 0;
    public bool HasCharts   => _chartSession.Charts.Count > 0;
    public bool IsOverview  => Mode == AgePointMode.Overview;
    public bool IsEventMode => Mode == AgePointMode.PositionsForEvent;

    public IReadOnlyList<NamedChart> Charts     => _chartSession.Charts;
    public WheelTheme               Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool                     ShowAspects => !HideAspects;

    public WheelPlotItem[] TransitPlotItemsForExport => [];

    // ── Labels — input screen ────────────────────────────────────────────────
    public string LabelTitle             => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.title");
    public string LabelNoSession         => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.nosession");
    public string LabelChartSection      => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.chartsection");
    public string LabelEventsHeader      => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.eventsheader");
    public string LabelNoEvents          => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.noevents");
    public string LabelModeOverview      => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.mode.overview");
    public string LabelModeEvent         => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.mode.positionsforevent");
    public string LabelHouseSystem       => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.housesystem.label");
    public HouseSystems SelectedHouseSystem => SelectedNamedHouseSystem?.Value ?? HouseSystems.Koch;
    public string LabelCalculate         => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.calculate");
    public string LabelColTitle          => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.col.title");
    public string LabelColDateTime       => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.col.datetime");
    public string LabelColLocation       => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.col.location");
    public string LabelSelect            => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.select");
    public string LabelHelp              => _rosetta.GetText(RbFile.AgePoint, "view.agepointinput.help");

    // Labels — wheel toolbar
    public string LabelWheelBlackWhite => _rosetta.GetText(RbFile.ChartWheel, IsBlackWhite ? ChartWheelKeys.ColorButton       : ChartWheelKeys.BlackWhiteButton);
    public string LabelWheelAspects    => _rosetta.GetText(RbFile.ChartWheel, HideAspects  ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelWheelExport     => _rosetta.GetText(RbFile.ChartWheel, ChartWheelKeys.ExportButton);

    // Labels — results screen
    public string LabelResultsTitle              => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.results.title");
    public string LabelNoResults                 => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.noresults");
    public string LabelTabPositions              => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.tab.positions");
    public string LabelTabMatches                => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.tab.matches");
    public string LabelTabMidpoints              => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.tab.midpoints");
    public string LabelTabWheel                  => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.tab.wheel");
    public string LabelPositionLabel             => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.positionlabel");
    public string LabelColPosition               => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.col.position");
    public string LabelColLabel                  => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.overview.col.label");
    public string LabelMatchColOrb               => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.col.orb");
    public string LabelMatchColExactness         => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.col.exactness");
    public string LabelNoMatches                 => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.noaspects");
    public string LabelAspectsHeader             => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.aspectsheader");
    public string LabelMidpointsRadixHeader      => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.midpoints.radixheader");
    public string LabelMidpointsAgePointHeader   => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.midpoints.agepointheader");
    public string LabelMidpointsNoMatches        => _rosetta.GetText(RbFile.AgePoint, "view.agepoint.matches.midpoints.nomatches");
    public string LabelMidpointsColFactor1       => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor1");
    public string LabelMidpointsColFactor2       => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor2");
    public string LabelMidpointsColMidpoint      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.midpoint");
    public string LabelMidpointsColPlanet        => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.planet");
    public string LabelMidpointsColOrb           => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.orb");
    public string LabelMidpointsColExactness     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.exactness");
    public string LabelDial360                   => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.360");
    public string LabelDial90                    => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.90");
    public string LabelDial45                    => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.45");
    public string LabelResultsHelp               => _rosetta.GetText(RbFile.AgePoint, "view.agepointresults.help");

    public string SinglePositionText
    {
        get
        {
            if (!_results.TryGetValue(Factors.AgePoint, out var pos)) return string.Empty;
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(pos.Longitude);
            if (!ok || !sign.HasValue) return $"{pos.Longitude:F4}°";
            return $"{dms} {GlyphSelector.GetGlyphForSign(sign.Value)}";
        }
    }

    // All available house systems for the dropdown (excluding NoHouses and Gauquelin)
    public IReadOnlyList<NamedHouseSystem> AvailableHouseSystems { get; }

    // ── Constructor ──────────────────────────────────────────────────────────

    public AgePointViewModel(
        AgePointOrchestrator apOrchestrator,
        IConfigContext configContext,
        IChartSession chartSession,
        IProgressiveSession progressiveSession,
        IHoroscopeRepository horoscopeRepository,
        EventsOrchestrator eventsOrchestrator,
        IRosetta rosetta)
    {
        _apOrchestrator      = apOrchestrator;
        _configContext       = configContext;
        _chartSession        = chartSession;
        _progressiveSession  = progressiveSession;
        _horoscopeRepository = horoscopeRepository;
        _eventsOrchestrator  = eventsOrchestrator;
        _rosetta             = rosetta;

        AvailableHouseSystems = Enum.GetValues<HouseSystems>()
            .Where(h => h != HouseSystems.NoHouses && h != HouseSystems.Gauquelin)
            .Select(h => new NamedHouseSystem(h, rosetta.GetText(RbFile.Localizable, h.LocalizedName())))
            .ToList();
        _selectedNamedHouseSystem = AvailableHouseSystems.FirstOrDefault(h => h.Value == HouseSystems.Koch)
                                    ?? AvailableHouseSystems.FirstOrDefault();

        SelectedEvent = _progressiveSession.SelectedEvent;
        _progressiveSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IProgressiveSession.SelectedEvent))
            {
                SelectedEvent = _progressiveSession.SelectedEvent;
                ClearResults();
                RebuildEventRows();
            }
        };

        _chartSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(IChartSession.Charts) or nameof(IChartSession.Selected))
            {
                OnPropertyChanged(nameof(Charts));
                OnPropertyChanged(nameof(HasCharts));
            }
        };

        if (_chartSession.Selected is not null)
            SelectedChart = _chartSession.Selected;
    }

    // ── Chart / event loading ────────────────────────────────────────────────

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        CalculateCommand.NotifyCanExecuteChanged();
        if (value is null) return;
        ClearResults();
        _ = LoadEventsAsync();
    }

    public async Task LoadEventsAsync()
    {
        if (SelectedChart is null) return;
        try
        {
            _radixChart = _chartSession.SelectedChart;
            var all       = await _horoscopeRepository.FetchAllAsync();
            var horoscope = all.FirstOrDefault(h => h.Name == SelectedChart.Name);
            _radixHoroscope = horoscope;
            if (horoscope is null) { _rawEvents = []; RebuildEventRows(); return; }
            var events = await _eventsOrchestrator.EventsForHoroscopeAsync(horoscope.Id);
            _rawEvents = events.OrderBy(e => e.JulianDate).ToList();
            RebuildEventRows();
            HasError = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    public void SelectEvent(ChartEvent chartEvent)
    {
        _progressiveSession.Select(chartEvent);
        SelectedEvent = chartEvent;
        ClearResults();
        RebuildEventRows();
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        try
        {
            ErrorMessage = string.Empty;
            HasError     = false;
            ClearResults();

            if (_radixChart is null) return;

            // Get cusps — try recalculating for the selected house system if possible
            double[] cusps;
            if (_radixHoroscope is not null)
                cusps = _apOrchestrator.CalculateCusps(_radixChart, _radixHoroscope, SelectedHouseSystem)
                        ?? AgePointOrchestrator.CuspsFromChart(_radixChart);
            else
                cusps = AgePointOrchestrator.CuspsFromChart(_radixChart);

            var asc = _radixChart.HousePositions.Ascendant.Longitude;

            if (Mode == AgePointMode.Overview)
            {
                BuildOverviewResults(cusps, asc);
            }
            else
            {
                if (SelectedEvent is null) return;
                var age = (SelectedEvent.JulianDate - _radixChart.JulianDay) / TropicalYear;
                var lon = _apOrchestrator.AgePointLongitude(age, cusps);
                _results[Factors.AgePoint] = new ProgressivePosition(lon, 0.0);

                BuildSinglePositionRows();
                BuildMatchRows();
                BuildMidpointRows();
            }

            BuildWheelData(asc, cusps);
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(SinglePositionText));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError     = true;
        }
    }

    private bool CanCalculate()
    {
        if (SelectedChart is null) return false;
        if (Mode == AgePointMode.PositionsForEvent && SelectedEvent is null) return false;
        return true;
    }

    partial void OnSelectedEventChanged(ChartEvent? value) => CalculateCommand.NotifyCanExecuteChanged();
    partial void OnModeChanged(AgePointMode value)
    {
        CalculateCommand.NotifyCanExecuteChanged();
        ClearResults();
    }

    [RelayCommand]
    private void ToggleBlackWhite() => IsBlackWhite = !IsBlackWhite;

    [RelayCommand]
    private void ToggleAspects() => HideAspects = !HideAspects;

    partial void OnIsBlackWhiteChanged(bool value)
    {
        OnPropertyChanged(nameof(Theme));
        OnPropertyChanged(nameof(LabelWheelBlackWhite));
    }

    partial void OnHideAspectsChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowAspects));
        OnPropertyChanged(nameof(LabelWheelAspects));
    }

    partial void OnMidpointDialTypeChanged(MidpointDialType value)
    {
        if (Mode == AgePointMode.PositionsForEvent) BuildMidpointRows();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void ClearResults()
    {
        _results = [];
        PositionRows.Clear();
        MatchRows.Clear();
        RadixMidpointRows.Clear();
        ProgressiveMidpointRows.Clear();
        RadixMidpointMatches       = [];
        ProgressiveMidpointMatches = [];
        RadixPlotData    = WheelPlotData.Empty;
        ApLongitude      = null;
        ApOverviewMarks  = [];
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(SinglePositionText));
    }

    private void RebuildEventRows()
    {
        var selectedId = SelectedEvent?.Id;
        EventRows = new ObservableCollection<EventRow>(
            _rawEvents.Select((e, i) => new EventRow(
                e, i,
                e.Id == selectedId,
                LabelSelect, string.Empty, string.Empty)));
    }

    private void BuildOverviewResults(double[] cusps, double asc)
    {
        var ageLabel = _rosetta.GetText(RbFile.AgePoint, "view.agepoint.overview.age");
        var ageFmt   = _rosetta.GetText(RbFile.AgePoint, "view.agepoint.overview.agefmt");

        var entries = new List<(string Label, double Longitude)>();
        for (var y = 0; y <= 71; y++)
        {
            var lon   = _apOrchestrator.AgePointLongitude(y, cusps);
            var label = y <= 48
                ? string.Format(ageFmt, y, y + 72)
                : string.Format(ageLabel, y);
            entries.Add((label, lon));
        }

        PositionRows = new ObservableCollection<AgePointOverviewRow>(
            entries.Select((e, i) => new AgePointOverviewRow(e.Label, e.Longitude, i)));

        ApOverviewMarks = entries
            .Select(e => new AgePointWheelMark(e.Label, WheelGeometry.MundaneAngle(e.Longitude, asc)))
            .ToArray();
    }

    private void BuildSinglePositionRows()
    {
        if (!_results.TryGetValue(Factors.AgePoint, out var pos)) return;

        ApLongitude = pos.Longitude;
        var label = _rosetta.GetText(RbFile.AgePoint, "view.agepoint.positionlabel");
        PositionRows = new ObservableCollection<AgePointOverviewRow>(
        [
            new AgePointOverviewRow(label, pos.Longitude, 0)
        ]);
    }

    private void BuildWheelData(double asc, double[] cusps)
    {
        if (_radixChart is null) return;
        var config = _configContext.ActiveConfig;
        RadixPlotData = WheelPlotDataBuilder.Build(_radixChart, config);

        if (Mode == AgePointMode.PositionsForEvent && _results.TryGetValue(Factors.AgePoint, out var pos))
            ApLongitude = pos.Longitude;
    }

    private void BuildMatchRows()
    {
        if (_radixChart is null || _results.Count == 0)
        {
            MatchRows.Clear();
            return;
        }

        var config      = _configContext.ActiveConfig;
        var baseOrb     = config.ProgressionsConfig.Transits.Orb;
        var parallelOrb = config.OrbConfig.ParallelOrb;

        var aspects   = TransitAspectsOrchestrator.Calculate(
            _results, _radixChart, config.FactorConfig, config.AspectConfig, baseOrb);
        var parallels = TransitParallelsOrchestrator.Calculate(
            _results, _radixChart, config.FactorConfig, parallelOrb);

        var rows = new List<(double Orb, TransitSecDir.UI.TransitMatchRow Row)>();

        foreach (var found in aspects)
        {
            var exactness = found.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - found.Orb / found.MaxOrb) * 100)))
                : 100;
            var totalMin = (int)(found.Orb * 60);
            var orbText  = $"{totalMin / 60}°{totalMin % 60:D2}'";
            rows.Add((found.Orb, new TransitSecDir.UI.TransitMatchRow(
                GlyphSelector.GetGlyphForFactor(found.Factor1),
                GlyphSelector.GetGlyphForAspect(found.Aspect),
                GlyphSelector.GetGlyphForFactor(found.Factor2),
                orbText, exactness, 0)));
        }

        foreach (var found in parallels)
        {
            var exactness = found.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - found.Orb / found.MaxOrb) * 100)))
                : 100;
            var totalMin = (int)(found.Orb * 60);
            var orbText  = $"{totalMin / 60}°{totalMin % 60:D2}'";
            var matchGlyph = found.IsContraParallel ? "" : "";
            rows.Add((found.Orb, new TransitSecDir.UI.TransitMatchRow(
                GlyphSelector.GetGlyphForFactor(found.Factor1),
                matchGlyph,
                GlyphSelector.GetGlyphForFactor(found.Factor2),
                orbText, exactness, 0)));
        }

        var reindexed = rows.OrderBy(r => r.Orb)
            .Select((r, i) => new TransitSecDir.UI.TransitMatchRow(
                r.Row.TransitGlyph, r.Row.MatchGlyph, r.Row.RadixGlyph,
                r.Row.OrbText, int.Parse(r.Row.ExactnessText.TrimEnd('%')), i))
            .ToList();

        MatchRows = new ObservableCollection<TransitSecDir.UI.TransitMatchRow>(reindexed);
    }

    private void BuildMidpointRows()
    {
        RadixMidpointRows.Clear();
        ProgressiveMidpointRows.Clear();

        if (_radixChart is null || _results.Count == 0)
        {
            RadixMidpointMatches       = [];
            ProgressiveMidpointMatches = [];
            return;
        }

        var config      = _configContext.ActiveConfig;
        var resultsDict = new Dictionary<Factors, ProgressivePosition>(_results);

        var radixMatches = MidpointsOrchestrator.RadixMidpointsOccupiedByProgressive(
            _radixChart, resultsDict, config.FactorConfig, config.OrbConfig, MidpointDialType);
        var progMatches  = MidpointsOrchestrator.ProgressiveMidpointsOccupiedByRadix(
            _radixChart, resultsDict, config.FactorConfig, config.OrbConfig, MidpointDialType);

        RadixMidpointMatches       = radixMatches;
        ProgressiveMidpointMatches = progMatches;

        PopulateMidpointRows(radixMatches, RadixMidpointRows);
        PopulateMidpointRows(progMatches,  ProgressiveMidpointRows);
    }

    private static void PopulateMidpointRows(
        IEnumerable<MidpointMatch> matches,
        ObservableCollection<TransitSecDir.UI.ProgressiveMidpointRow> rows)
    {
        var i = 0;
        foreach (var m in matches)
        {
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(m.MidpointPosition);
            var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";
            var totalMin  = (int)(Math.Abs(m.ActualOrb) * 60);
            var orbText   = $"{totalMin / 60}°{totalMin % 60:D2}'";
            var exactness = m.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - m.ActualOrb / m.MaxOrb) * 100)))
                : 100;
            rows.Add(new TransitSecDir.UI.ProgressiveMidpointRow(
                GlyphSelector.GetGlyphForFactor(m.Factor1),
                GlyphSelector.GetGlyphForFactor(m.Factor2),
                dms, signGlyph,
                GlyphSelector.GetGlyphForFactor(m.MatchingFactor),
                orbText, $"{exactness}%", i++ % 2 == 0));
        }
    }
}
