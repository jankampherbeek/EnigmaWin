// LogTimeScaleViewModel.cs
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

namespace EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;

public enum LtsMode { Overview = 0, PositionsForEvent = 1 }

public partial class LogTimeScaleViewModel : ObservableObject
{
    private const double Z            = 0.0766835;
    private const double TropicalYear = 365.242199074;

    private readonly LogTimeScaleOrchestrator  _ltsOrchestrator;
    private readonly IConfigContext            _configContext;
    private readonly IChartSession             _chartSession;
    private readonly IProgressiveSession       _progressiveSession;
    private readonly IHoroscopeRepository      _horoscopeRepository;
    private readonly EventsOrchestrator        _eventsOrchestrator;
    private readonly IRosetta                  _rosetta;

    // ── Observable state ────────────────────────────────────────────────────

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEvents))]
    private ObservableCollection<EventRow> _eventRows = [];

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<LtsOverviewRow> _positionRows = [];

    [ObservableProperty]
    private ObservableCollection<TransitSecDir.UI.TransitMatchRow> _matchRows = [];

    [ObservableProperty]
    private ObservableCollection<TransitSecDir.UI.ProgressiveMidpointRow> _radixMidpointRows = [];
    [ObservableProperty]
    private ObservableCollection<TransitSecDir.UI.ProgressiveMidpointRow> _progressiveMidpointRows = [];
    [ObservableProperty] private IReadOnlyList<MidpointMatch> _radixMidpointMatches       = [];
    [ObservableProperty] private IReadOnlyList<MidpointMatch> _progressiveMidpointMatches  = [];
    [ObservableProperty] private MidpointDialType             _midpointDialType = MidpointDialType.Dial360;

    [ObservableProperty] private ChartEvent? _selectedEvent;
    [ObservableProperty] private NamedChart?  _selectedChart;
    [ObservableProperty] private string  _errorMessage = string.Empty;
    [ObservableProperty] private bool    _hasError;
    [ObservableProperty] private int     _activeTab;     // 0=Positions, 1=Matches, 2=Midpoints, 3=Wheel
    [ObservableProperty] private bool    _isBlackWhite = false;
    [ObservableProperty] private bool    _hideAspects  = false;
    [ObservableProperty] private LtsMode _mode = LtsMode.Overview;

    // Wheel data
    [ObservableProperty] private WheelPlotData  _radixPlotData    = WheelPlotData.Empty;
    [ObservableProperty] private double?        _ltsLongitude;      // single position (PositionsForEvent)
    [ObservableProperty] private LtsWheelMark[] _ltsOverviewMarks  = [];

    private List<ChartEvent>                       _rawEvents  = [];
    private FullChart?                             _radixChart;
    private Dictionary<Factors, ProgressivePosition> _results  = [];

    // ── Derived ─────────────────────────────────────────────────────────────

    public bool HasEvents   => EventRows.Count > 0;
    public bool HasResults  => PositionRows.Count > 0 || _results.Count > 0;
    public bool HasCharts   => _chartSession.Charts.Count > 0;
    public bool IsOverview  => Mode == LtsMode.Overview;
    public bool IsEventMode => Mode == LtsMode.PositionsForEvent;

    public IReadOnlyList<NamedChart> Charts     => _chartSession.Charts;
    public WheelTheme               Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool                     ShowAspects => !HideAspects;

    // For WheelExportService compatibility — empty because the LTS canvas handles its own drawing
    public WheelPlotItem[] TransitPlotItemsForExport => [];

    // ── Labels — input screen ────────────────────────────────────────────────
    public string LabelTitle             => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.title");
    public string LabelNoSession         => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.nosession");
    public string LabelChartSection      => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.chartsection");
    public string LabelEventsHeader      => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.eventsheader");
    public string LabelNoEvents          => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.noevents");
    public string LabelModeOverview      => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.mode.overview");
    public string LabelModeEvent         => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.mode.positionsforevent");
    public string LabelCalculate         => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.calculate");
    public string LabelColTitle          => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.col.title");
    public string LabelColDateTime       => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.col.datetime");
    public string LabelColLocation       => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.col.location");
    public string LabelSelect            => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.select");
    public string LabelHelp              => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescaleinput.help");

    // Labels — wheel toolbar
    public string LabelWheelBlackWhite => _rosetta.GetText(RbFile.ChartWheel, IsBlackWhite ? ChartWheelKeys.ColorButton       : ChartWheelKeys.BlackWhiteButton);
    public string LabelWheelAspects    => _rosetta.GetText(RbFile.ChartWheel, HideAspects  ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelWheelExport     => _rosetta.GetText(RbFile.ChartWheel, ChartWheelKeys.ExportButton);

    // Labels — results screen
    public string LabelResultsTitle               => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.results.title");
    public string LabelNoResults                  => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.noresults");
    public string LabelTabPositions               => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.tab.positions");
    public string LabelTabMatches                 => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.tab.matches");
    public string LabelTabMidpoints               => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.tab.midpoints");
    public string LabelTabWheel                   => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.tab.wheel");
    public string LabelPositionLabel              => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.positionlabel");
    public string LabelColPosition                => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.col.position");
    public string LabelColLabel                   => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.overview.col.label");
    public string LabelMatchColOrb                => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.col.orb");
    public string LabelMatchColExactness          => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.col.exactness");
    public string LabelNoMatches                  => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.noaspects");
    public string LabelAspectsHeader              => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.aspectsheader");
    public string LabelMidpointsRadixHeader       => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.midpoints.radixheader");
    public string LabelMidpointsTimescaleHeader   => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.midpoints.timescaleheader");
    public string LabelMidpointsNoMatches         => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.matches.midpoints.nomatches");
    public string LabelMidpointsColFactor1        => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor1");
    public string LabelMidpointsColFactor2        => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor2");
    public string LabelMidpointsColMidpoint       => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.midpoint");
    public string LabelMidpointsColPlanet         => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.planet");
    public string LabelMidpointsColOrb            => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.orb");
    public string LabelMidpointsColExactness      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.exactness");
    public string LabelDial360                    => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.360");
    public string LabelDial90                     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.90");
    public string LabelDial45                     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.45");
    public string LabelResultsHelp                => _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescaleresults.help");

    // Single-position display text (PositionsForEvent mode)
    public string SinglePositionText
    {
        get
        {
            if (!_results.TryGetValue(Factors.LogTimeScale, out var pos)) return string.Empty;
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(pos.Longitude);
            if (!ok || !sign.HasValue) return $"{pos.Longitude:F4}°";
            return $"{dms} {GlyphSelector.GetGlyphForSign(sign.Value)}";
        }
    }

    // ── Constructor ──────────────────────────────────────────────────────────

    public LogTimeScaleViewModel(
        LogTimeScaleOrchestrator ltsOrchestrator,
        IConfigContext configContext,
        IChartSession chartSession,
        IProgressiveSession progressiveSession,
        IHoroscopeRepository horoscopeRepository,
        EventsOrchestrator eventsOrchestrator,
        IRosetta rosetta)
    {
        _ltsOrchestrator     = ltsOrchestrator;
        _configContext       = configContext;
        _chartSession        = chartSession;
        _progressiveSession  = progressiveSession;
        _horoscopeRepository = horoscopeRepository;
        _eventsOrchestrator  = eventsOrchestrator;
        _rosetta             = rosetta;

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
            var asc = _radixChart.HousePositions.Ascendant.Longitude;

            if (Mode == LtsMode.Overview)
            {
                BuildOverviewResults(asc);
            }
            else
            {
                if (SelectedEvent is null) return;
                var age = (SelectedEvent.JulianDate - _radixChart.JulianDay) / TropicalYear;
                var lon = LogTimeScaleOrchestrator.MannPositionFromAge(age, asc);
                _results[Factors.LogTimeScale] = new ProgressivePosition(lon, 0.0);

                BuildSinglePositionRows();
                BuildMatchRows();
                BuildMidpointRows();
            }

            BuildWheelData(asc);
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
        if (Mode == LtsMode.PositionsForEvent && SelectedEvent is null) return false;
        return true;
    }

    partial void OnSelectedEventChanged(ChartEvent? value) => CalculateCommand.NotifyCanExecuteChanged();
    partial void OnModeChanged(LtsMode value)
    {
        CalculateCommand.NotifyCanExecuteChanged();
        ClearResults();
    }

    [RelayCommand]
    private void ToggleBlackWhite()
    {
        IsBlackWhite = !IsBlackWhite;
    }

    [RelayCommand]
    private void ToggleAspects()
    {
        HideAspects = !HideAspects;
    }

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
        if (Mode == LtsMode.PositionsForEvent) BuildMidpointRows();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void ClearResults()
    {
        _results = [];
        PositionRows.Clear();
        MatchRows.Clear();
        RadixMidpointRows.Clear();
        ProgressiveMidpointRows.Clear();
        RadixMidpointMatches      = [];
        ProgressiveMidpointMatches = [];
        RadixPlotData    = WheelPlotData.Empty;
        LtsLongitude     = null;
        LtsOverviewMarks = [];
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

    private void BuildOverviewResults(double asc)
    {
        var entries = new List<(string Label, double Longitude)>();

        // Prenatal months 0-8
        for (var m = 0; m <= 8; m++)
        {
            var age = -((double)(9 - m)) * Z;
            var lon = LogTimeScaleOrchestrator.MannPositionFromAge(age, asc);
            var label = string.Format(_rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.overview.month"), m);
            entries.Add((label, lon));
        }

        // Birth at ascendant
        entries.Add((string.Format(_rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.overview.age"), 0), asc));

        // Ages 1-40
        for (var y = 1; y <= 40; y++)
        {
            var lon = LogTimeScaleOrchestrator.MannPositionFromAge(y, asc);
            entries.Add((string.Format(_rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.overview.age"), y), lon));
        }
        // Ages 42-50 step 2
        for (var y = 42; y <= 50; y += 2)
        {
            var lon = LogTimeScaleOrchestrator.MannPositionFromAge(y, asc);
            entries.Add((string.Format(_rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.overview.age"), y), lon));
        }
        // Ages 55-70 step 5
        for (var y = 55; y <= 70; y += 5)
        {
            var lon = LogTimeScaleOrchestrator.MannPositionFromAge(y, asc);
            entries.Add((string.Format(_rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.overview.age"), y), lon));
        }

        PositionRows = new ObservableCollection<LtsOverviewRow>(
            entries.Select((e, i) => new LtsOverviewRow(e.Label, e.Longitude, i)));

        // Build wheel marks (prenatal months use isMonth=true)
        var marks = entries
            .Select((e, i) => new LtsWheelMark(
                e.Label,
                WheelGeometry.MundaneAngle(e.Longitude, asc),
                i <= 8))
            .ToArray();
        LtsOverviewMarks = marks;
    }

    private void BuildSinglePositionRows()
    {
        if (!_results.TryGetValue(Factors.LogTimeScale, out var pos)) return;

        LtsLongitude = pos.Longitude;
        var label = _rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.positionlabel");
        PositionRows = new ObservableCollection<LtsOverviewRow>(
        [
            new LtsOverviewRow(label, pos.Longitude, 0)
        ]);
    }

    private void BuildWheelData(double asc)
    {
        if (_radixChart is null) return;
        var config = _configContext.ActiveConfig;
        RadixPlotData = WheelPlotDataBuilder.Build(_radixChart, config);

        if (Mode == LtsMode.PositionsForEvent && _results.TryGetValue(Factors.LogTimeScale, out var pos))
        {
            LtsLongitude = pos.Longitude;
        }
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
            RadixMidpointMatches      = [];
            ProgressiveMidpointMatches = [];
            return;
        }

        var config       = _configContext.ActiveConfig;
        var resultsDict  = new Dictionary<Factors, ProgressivePosition>(_results);

        var radixMatches = MidpointsOrchestrator.RadixMidpointsOccupiedByProgressive(
            _radixChart, resultsDict, config.FactorConfig, config.OrbConfig, MidpointDialType);
        var progMatches  = MidpointsOrchestrator.ProgressiveMidpointsOccupiedByRadix(
            _radixChart, resultsDict, config.FactorConfig, config.OrbConfig, MidpointDialType);

        RadixMidpointMatches      = radixMatches;
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
