// SymbolicViewModel.cs
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
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Progressive.Events.UI;
using EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.SymbolicDir.UI;

public partial class SymbolicViewModel : ObservableObject
{
    private readonly ProgressiveOrchestrator _orchestrator;
    private readonly IConfigContext          _configContext;
    private readonly IChartSession           _chartSession;
    private readonly IProgressiveSession     _progressiveSession;
    private readonly IHoroscopeRepository    _horoscopeRepository;
    private readonly EventsOrchestrator      _eventsOrchestrator;
    private readonly IRosetta                _rosetta;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEvents))]
    private ObservableCollection<EventRow> _eventRows = [];

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<SymbolicPositionRow> _positionRows = [];

    [ObservableProperty]
    private ObservableCollection<TransitMatchRow> _matchRows = [];

    [ObservableProperty]
    private ObservableCollection<ProgressiveMidpointRow> _radixMidpointRows = [];
    [ObservableProperty]
    private ObservableCollection<ProgressiveMidpointRow> _progressiveMidpointRows = [];
    [ObservableProperty] private IReadOnlyList<MidpointMatch> _radixMidpointMatches  = [];
    [ObservableProperty] private IReadOnlyList<MidpointMatch> _progressiveMidpointMatches = [];
    [ObservableProperty] private MidpointDialType             _midpointDialType = MidpointDialType.Dial360;

    [ObservableProperty] private ChartEvent?                      _selectedEvent;
    [ObservableProperty] private NamedChart?                      _selectedChart;
    [ObservableProperty] private DisplayItem<SymbolicKeys>?       _selectedSymbolicKey;
    [ObservableProperty] private string                           _errorMessage = string.Empty;
    [ObservableProperty] private bool                             _hasError;
    [ObservableProperty] private int                              _activeTab;      // 0=Positions, 1=Aspects, 2=Midpoints, 3=DualWheel
    [ObservableProperty] private bool                             _isBlackWhite  = false;
    [ObservableProperty] private bool                             _hideAspects   = false;

    public IReadOnlyList<DisplayItem<SymbolicKeys>> SymbolicKeyValues { get; }

    [ObservableProperty] private WheelPlotData   _radixPlotData    = WheelPlotData.Empty;
    [ObservableProperty] private WheelPlotItem[] _transitPlotItems = [];

    private List<ChartEvent>                         _rawEvents  = [];
    private Dictionary<Factors, ProgressivePosition> _results    = [];
    private FullChart?                               _radixChart;
    private double?                                  _natalJulianDay;

    public bool HasEvents  => EventRows.Count > 0;
    public bool HasResults => PositionRows.Count > 0;
    public bool HasCharts  => _chartSession.Charts.Count > 0;
    public IReadOnlyList<NamedChart> Charts => _chartSession.Charts;

    public WheelTheme Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool       ShowAspects => !HideAspects;

    // Labels — input screen
    public string LabelTitle        => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.title");
    public string LabelNoSession    => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.nosession");
    public string LabelChartSection => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.chartsection");
    public string LabelArcMethod    => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.arcmethod");
    public string LabelEventsHeader => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.eventsheader");
    public string LabelNoEvents     => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.noevents");
    public string LabelCalculate    => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.calculate");
    public string LabelColTitle     => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.col.title");
    public string LabelColDateTime  => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.col.datetime");
    public string LabelColLocation  => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.col.location");
    public string LabelSelect       => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.select");
    public string LabelErrorNoNatal => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.error.nonatal");
    public string LabelHelp         => _rosetta.GetText(RbFile.Symbolic, "view.symbolicscreen.help");
    public string LabelResultsHelp  => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.help");

    // Labels — dual wheel toolbar
    public string LabelWheelBlackWhite => _rosetta.GetText(RbFile.ChartWheel, IsBlackWhite ? ChartWheelKeys.ColorButton       : ChartWheelKeys.BlackWhiteButton);
    public string LabelWheelAspects    => _rosetta.GetText(RbFile.ChartWheel, HideAspects  ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelWheelExport     => _rosetta.GetText(RbFile.ChartWheel, ChartWheelKeys.ExportButton);

    // Labels — results screen
    public string LabelResultsTitle   => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.title");
    public string LabelNoResults      => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.noresults");
    public string LabelTabPositions   => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.tab.positions");
    public string LabelTabMatches     => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.tab.matches");
    public string LabelTabMidpoints   => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.tab.midpoints");
    public string LabelTabDualWheel   => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.tab.dualwheel");
    public string LabelMidpointsRadixHeader      => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.midpoints.radixheader");
    public string LabelMidpointsProgressiveHeader => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.midpoints.symbolicheader");
    public string LabelMidpointsNoMatches        => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.midpoints.nomatches");
    public string LabelMidpointsColFactor1  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor1");
    public string LabelMidpointsColFactor2  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor2");
    public string LabelMidpointsColMidpoint => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.midpoint");
    public string LabelMidpointsColPlanet   => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.planet");
    public string LabelMidpointsColOrb      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.orb");
    public string LabelMidpointsColExactness => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.exactness");
    public string LabelDial360 => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.360");
    public string LabelDial90  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.90");
    public string LabelDial45  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.45");
    public string LabelColFactor      => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.col.factor");
    public string LabelColLongitude   => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.col.longitude");
    public string LabelMatchColOrb    => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.matches.col.orb");
    public string LabelMatchColExactness => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.matches.col.exactness");
    public string LabelNoMatches      => _rosetta.GetText(RbFile.Symbolic, "view.symbolicresults.matches.nomatches");

    public SymbolicViewModel(
        ProgressiveOrchestrator orchestrator,
        IConfigContext configContext,
        IChartSession chartSession,
        IProgressiveSession progressiveSession,
        IHoroscopeRepository horoscopeRepository,
        EventsOrchestrator eventsOrchestrator,
        IRosetta rosetta)
    {
        _orchestrator        = orchestrator;
        _configContext       = configContext;
        _chartSession        = chartSession;
        _progressiveSession  = progressiveSession;
        _horoscopeRepository = horoscopeRepository;
        _eventsOrchestrator  = eventsOrchestrator;
        _rosetta             = rosetta;

        SymbolicKeyValues = Enum.GetValues<SymbolicKeys>()
            .Select(k => new DisplayItem<SymbolicKeys>(k, rosetta.GetText(RbFile.Localizable, k.LocalizedName())))
            .ToList();
        _selectedSymbolicKey = SymbolicKeyValues.FirstOrDefault(d => d.Value == SymbolicKeys.OneDegree)
                               ?? SymbolicKeyValues.FirstOrDefault();

        SelectedEvent = _progressiveSession.SelectedEvent;
        _progressiveSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IProgressiveSession.SelectedEvent))
            {
                SelectedEvent = _progressiveSession.SelectedEvent;
                ClearResults();
                RebuildRows();
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

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        if (value is null) return;
        ClearResults();
        _ = LoadHoroscopeAsync();
    }

    public async Task LoadHoroscopeAsync()
    {
        if (SelectedChart is null) return;
        try
        {
            var all       = await _horoscopeRepository.FetchAllAsync();
            var horoscope = all.FirstOrDefault(h => h.Name == SelectedChart.Name);
            _natalJulianDay = horoscope?
                .DateTimes
                .FirstOrDefault(dt => dt.IsPreferred)?
                .JulianDate;
            _radixChart = _chartSession.SelectedChart;
            if (horoscope is not null)
            {
                var events = await _eventsOrchestrator.EventsForHoroscopeAsync(horoscope.Id);
                _rawEvents = events.OrderBy(e => e.JulianDate).ToList();
                RebuildRows();
            }
            HasError = false;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError     = true;
        }
    }

    public void SetEvents(IEnumerable<ChartEvent> events, FullChart? radixChart, double? natalJulianDay)
    {
        _radixChart     = radixChart;
        _natalJulianDay = natalJulianDay;
        _rawEvents      = events.OrderBy(e => e.JulianDate).ToList();
        ClearResults();
        RebuildRows();
    }

    public void SelectEvent(ChartEvent chartEvent)
    {
        _progressiveSession.Select(chartEvent);
        SelectedEvent = chartEvent;
        ClearResults();
        RebuildRows();
    }

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        if (SelectedEvent is null) return;
        if (_natalJulianDay is null)
        {
            ErrorMessage = LabelErrorNoNatal;
            HasError     = true;
            return;
        }
        try
        {
            ErrorMessage = string.Empty;
            HasError     = false;

            var symbolicKey = SelectedSymbolicKey?.Value ?? SymbolicKeys.OneDegree;
            var request     = new ProgressiveCalcRequest(
                SelectedEvent.JulianDate,
                ProgressiveMethods.SymDir,
                NatalJulianDay: _natalJulianDay);

            _results = _orchestrator.ProgressivePositions(request, symbolicKey);
            BuildPositionRows();
            BuildMatchRows();
            BuildMidpointRows();
            BuildWheelData();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError     = true;
        }
    }

    private bool CanCalculate() => SelectedEvent is not null;

    partial void OnSelectedEventChanged(ChartEvent? value)
    {
        CalculateCommand.NotifyCanExecuteChanged();
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

    // ── Private helpers ──────────────────────────────────────────────────────

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
        TransitPlotItems = [];
    }

    private void RebuildRows()
    {
        var selectedId = SelectedEvent?.Id;
        EventRows = new ObservableCollection<EventRow>(
            _rawEvents.Select((e, i) => new EventRow(
                e, i,
                e.Id == selectedId,
                LabelSelect, string.Empty, string.Empty)));
    }

    private void BuildPositionRows()
    {
        var rows = _results
            .OrderBy(kvp => (int)kvp.Key)
            .Select((kvp, i) => new SymbolicPositionRow(kvp.Key, kvp.Value.Longitude, i))
            .ToList();
        PositionRows = new ObservableCollection<SymbolicPositionRow>(rows);
    }

    private void BuildWheelData()
    {
        if (_radixChart is null) return;
        var config = _configContext.ActiveConfig;
        RadixPlotData = WheelPlotDataBuilder.Build(_radixChart, config);

        var ascLong = _radixChart.HousePositions.Ascendant.Longitude;
        TransitPlotItems = _results
            .Select(kvp => new WheelPlotItem(
                Factor:            kvp.Key,
                Glyph:             GlyphSelector.GetGlyphForFactor(kvp.Key),
                EclipticLongitude: kvp.Value.Longitude,
                MundaneAngle:      WheelGeometry.MundaneAngle(kvp.Value.Longitude, ascLong),
                PlotAngle:         WheelGeometry.MundaneAngle(kvp.Value.Longitude, ascLong),
                PositionText:      $"{(int)(kvp.Value.Longitude % 30)}°{(int)(kvp.Value.Longitude % 30 * 60) % 60:D2}'",
                SpeedType:         SpeedType.Direct))
            .ToArray();
    }

    private void BuildMatchRows()
    {
        if (_radixChart is null || _results.Count == 0)
        {
            MatchRows.Clear();
            return;
        }

        var config  = _configContext.ActiveConfig;
        var baseOrb = config.ProgressionsConfig.SymbolicDirections.Orb;

        var aspects = TransitAspectsOrchestrator.Calculate(
            _results, _radixChart, config.FactorConfig, config.AspectConfig, baseOrb);

        var rows = new List<(double Orb, TransitMatchRow Row)>();

        foreach (var found in aspects)
        {
            var exactness = found.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - found.Orb / found.MaxOrb) * 100)))
                : 100;
            var totalMin = (int)(found.Orb * 60);
            var orbText  = $"{totalMin / 60}°{totalMin % 60:D2}'";
            rows.Add((found.Orb, new TransitMatchRow(
                GlyphSelector.GetGlyphForFactor(found.Factor1),
                GlyphSelector.GetGlyphForAspect(found.Aspect),
                GlyphSelector.GetGlyphForFactor(found.Factor2),
                orbText, exactness, 0)));
        }

        var reindexed = rows.OrderBy(r => r.Orb)
            .Select((r, i) => new TransitMatchRow(
                r.Row.TransitGlyph, r.Row.MatchGlyph, r.Row.RadixGlyph,
                r.Row.OrbText, int.Parse(r.Row.ExactnessText.TrimEnd('%')), i))
            .ToList();

        MatchRows = new ObservableCollection<TransitMatchRow>(reindexed);
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

        var config = _configContext.ActiveConfig;
        var resultsDict = new Dictionary<Factors, ProgressivePosition>(_results);

        var radixMatches = MidpointsOrchestrator.RadixMidpointsOccupiedByProgressive(
            _radixChart, resultsDict, config.FactorConfig, config.OrbConfig, MidpointDialType);
        var progMatches = MidpointsOrchestrator.ProgressiveMidpointsOccupiedByRadix(
            _radixChart, resultsDict, config.FactorConfig, config.OrbConfig, MidpointDialType);

        RadixMidpointMatches      = radixMatches;
        ProgressiveMidpointMatches = progMatches;

        PopulateMidpointRowCollection(radixMatches, RadixMidpointRows);
        PopulateMidpointRowCollection(progMatches, ProgressiveMidpointRows);
    }

    private static void PopulateMidpointRowCollection(
        IEnumerable<MidpointMatch> matches,
        ObservableCollection<ProgressiveMidpointRow> rows)
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
            rows.Add(new ProgressiveMidpointRow(
                GlyphSelector.GetGlyphForFactor(m.Factor1),
                GlyphSelector.GetGlyphForFactor(m.Factor2),
                dms, signGlyph,
                GlyphSelector.GetGlyphForFactor(m.MatchingFactor),
                orbText, $"{exactness}%", i++ % 2 == 0));
        }
    }

    partial void OnMidpointDialTypeChanged(MidpointDialType value) => BuildMidpointRows();
}
