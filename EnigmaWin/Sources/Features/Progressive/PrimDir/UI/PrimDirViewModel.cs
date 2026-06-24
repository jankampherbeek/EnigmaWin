// PrimDirViewModel.cs
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
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir.UI;

public partial class PrimDirViewModel : ObservableObject
{
    private readonly IChartSession        _chartSession;
    private readonly IConfigContext       _configContext;
    private readonly IProgressiveSession  _progressiveSession;
    private readonly IHoroscopeRepository _horoscopeRepository;
    private readonly EventsOrchestrator   _eventsOrchestrator;
    private readonly IRosetta             _rosetta;

    // Input
    [ObservableProperty] private string    _startDateText = string.Empty;
    [ObservableProperty] private string    _endDateText   = string.Empty;
    [ObservableProperty] private bool _isEventMode = false;
    [ObservableProperty] private ChartEvent? _selectedEvent;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEvents))]
    private ObservableCollection<ChartEvent> _events = [];

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasCharts))]
    private NamedChart? _selectedChart;

    // Settings override — index-based for ComboBox binding
    [ObservableProperty] private int _selectedMethodIndex   = 0;
    [ObservableProperty] private int _selectedApproachIndex = 0;
    [ObservableProperty] private int _selectedTimeKeyIndex  = 1; // Naibod default

    // Results
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<PrimDirResultRow> _resultRows = [];
    [ObservableProperty] private bool   _showOrbColumn    = false;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasMethodDescription))]
    private string _methodDescription = string.Empty;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasPeriodText))]
    private string _periodText        = string.Empty;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEventInfo))]
    private string _eventInfoText     = string.Empty;
    [ObservableProperty] private string _errorMessage      = string.Empty;
    [ObservableProperty] private bool   _hasError;

    private List<ChartEvent> _rawEvents   = [];
    private Horoscope?        _horoscope;

    public bool HasResults => ResultRows.Count > 0;
    public bool HasMethodDescription => !string.IsNullOrEmpty(MethodDescription);
    public bool HasPeriodText        => !string.IsNullOrEmpty(PeriodText);
    public bool HasEventInfo         => !string.IsNullOrEmpty(EventInfoText);
    public bool HasEvents  => Events.Count > 0;
    public bool HasCharts  => _chartSession.Charts.Count > 0;
    public IReadOnlyList<NamedChart> Charts => _chartSession.Charts;

    private PrimaryDirectionsConfig SavedConfig =>
        _configContext.ActiveConfig.ProgressionsConfig.PrimaryDirections;

    private static readonly PrimaryMethods[]    _allMethods    = Enum.GetValues<PrimaryMethods>();
    private static readonly PrimaryApproaches[] _allApproaches = Enum.GetValues<PrimaryApproaches>();
    private static readonly PrimaryTimeKeys[]   _allTimeKeys   = Enum.GetValues<PrimaryTimeKeys>();

    public IReadOnlyList<string> MethodNames   { get; private set; } = [];
    public IReadOnlyList<string> ApproachNames { get; private set; } = [];
    public IReadOnlyList<string> TimeKeyNames  { get; private set; } = [];

    // Localised labels — input
    public string LabelTitle             => _rosetta.GetText(RbFile.PrimDir, "view.primdir.title");
    public string LabelNoSession         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.nosession");
    public string LabelChartSection      => _rosetta.GetText(RbFile.PrimDir, "view.primdir.chartsection");
    public string LabelModeLabel         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.mode.label");
    public string LabelModeDateRange     => _rosetta.GetText(RbFile.PrimDir, "view.primdir.mode.daterange");
    public string LabelModeEvent         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.mode.event");
    public string LabelStartDate         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.startdate.label");
    public string LabelEndDate           => _rosetta.GetText(RbFile.PrimDir, "view.primdir.enddate.label");
    public string LabelDatePlaceholder   => _rosetta.GetText(RbFile.PrimDir, "view.primdir.date.placeholder");
    public string LabelEventPicker       => _rosetta.GetText(RbFile.PrimDir, "view.primdir.eventpicker.label");
    public string LabelNoEvents          => _rosetta.GetText(RbFile.PrimDir, "view.primdir.eventpicker.noevents");
    public string LabelSettingsHeader    => _rosetta.GetText(RbFile.PrimDir, "view.primdir.settings.header");
    public string LabelSettingsNote      => _rosetta.GetText(RbFile.PrimDir, "view.primdir.settings.overridenote");
    public string LabelSettingsReset     => _rosetta.GetText(RbFile.PrimDir, "view.primdir.settings.reset");
    public string LabelMethod            => _rosetta.GetText(RbFile.PrimDir, "view.primdir.settings.method");
    public string LabelApproach          => _rosetta.GetText(RbFile.PrimDir, "view.primdir.settings.approach");
    public string LabelTimeKey           => _rosetta.GetText(RbFile.PrimDir, "view.primdir.settings.timekey");
    public string LabelCalculate         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.calculate");
    public string LabelHelp              => _rosetta.GetText(RbFile.PrimDir, "view.primdir.help");
    public string LabelHelpTitle         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.help.title");
    public string LabelHelpClose         => _rosetta.GetText(RbFile.PrimDir, "view.primdir.help.close");
    public string LabelFactsheetTitle    => _rosetta.GetText(RbFile.PrimDir, "view.primdir.factsheet.title");
    public string LabelFactsheetTooltip  => _rosetta.GetText(RbFile.PrimDir, "view.primdir.factsheet.tooltip");

    // Localised labels — results
    public string LabelResultsTitle  => _rosetta.GetText(RbFile.PrimDir, "view.primdir.results.title");
    public string LabelNoResults     => _rosetta.GetText(RbFile.PrimDir, "view.primdir.results.noresults");
    public string LabelNoResultsEvent => _rosetta.GetText(RbFile.PrimDir, "view.primdir.results.noresults.event");
    public string LabelColDate       => _rosetta.GetText(RbFile.PrimDir, "view.primdir.results.col.date");
    public string LabelColOrb        => _rosetta.GetText(RbFile.PrimDir, "view.primdir.results.col.orb");
    public string LabelResultsHelp   => _rosetta.GetText(RbFile.PrimDir, "view.primdir.results.help");

    public PrimDirViewModel(
        IChartSession        chartSession,
        IConfigContext       configContext,
        IProgressiveSession  progressiveSession,
        IHoroscopeRepository horoscopeRepository,
        EventsOrchestrator   eventsOrchestrator,
        IRosetta             rosetta)
    {
        _chartSession        = chartSession;
        _configContext       = configContext;
        _progressiveSession  = progressiveSession;
        _horoscopeRepository = horoscopeRepository;
        _eventsOrchestrator  = eventsOrchestrator;
        _rosetta             = rosetta;

        BuildNameLists();
        SyncFromConfig();

        if (_chartSession.Selected is not null)
            SelectedChart = _chartSession.Selected;

        _chartSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(IChartSession.Charts) or nameof(IChartSession.Selected))
            {
                OnPropertyChanged(nameof(Charts));
                OnPropertyChanged(nameof(HasCharts));
                if (_chartSession.Selected is not null && SelectedChart is null)
                    SelectedChart = _chartSession.Selected;
                _ = LoadEventsAsync();
            }
        };
    }

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        ClearResults();
        CalculateCommand.NotifyCanExecuteChanged();
        _ = LoadEventsAsync();
    }

    partial void OnIsEventModeChanged(bool value)
    {
        ClearResults();
        CalculateCommand.NotifyCanExecuteChanged();
    }

    partial void OnStartDateTextChanged(string value) => CalculateCommand.NotifyCanExecuteChanged();
    partial void OnEndDateTextChanged(string value)   => CalculateCommand.NotifyCanExecuteChanged();
    partial void OnSelectedEventChanged(ChartEvent? value) => CalculateCommand.NotifyCanExecuteChanged();

    private void BuildNameLists()
    {
        MethodNames   = _allMethods.Select(m => _rosetta.GetText(RbFile.Localizable, m.LocalizedName())).ToList();
        ApproachNames = _allApproaches.Select(a => _rosetta.GetText(RbFile.Localizable, a.LocalizedName())).ToList();
        TimeKeyNames  = _allTimeKeys.Select(k => _rosetta.GetText(RbFile.Localizable, k.LocalizedName())).ToList();
    }

    [RelayCommand]
    public void SyncFromConfig()
    {
        var c = SavedConfig;
        SelectedMethodIndex   = (int)c.Method;
        SelectedApproachIndex = (int)c.Approach;
        SelectedTimeKeyIndex  = (int)c.TimeKey;
    }

    private bool CanCalculate() =>
        HasCharts && SelectedChart is not null &&
        (IsEventMode
            ? SelectedEvent is not null
            : !string.IsNullOrWhiteSpace(StartDateText) && !string.IsNullOrWhiteSpace(EndDateText));

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        if (SelectedChart is null) return;
        ErrorMessage = string.Empty;
        HasError     = false;

        var radixChart = SelectedChart.Chart;
        var natalJD    = radixChart.JulianDay;
        var geoLat     = _horoscope?.Latitude ?? 0.0;

        var method   = _allMethods[Math.Clamp(SelectedMethodIndex,   0, _allMethods.Length - 1)];
        var approach = _allApproaches[Math.Clamp(SelectedApproachIndex, 0, _allApproaches.Length - 1)];
        var timeKey  = _allTimeKeys[Math.Clamp(SelectedTimeKeyIndex,  0, _allTimeKeys.Length - 1)];

        var effectiveConfig = new PrimaryDirectionsConfig(
            SavedConfig.Promissors,
            SavedConfig.Significators,
            SavedConfig.Orb,
            method,
            approach,
            timeKey);

        try
        {
            List<PrimDirHit> hits;
            if (IsEventMode)
            {
                if (SelectedEvent is null)
                {
                    ErrorMessage = _rosetta.GetText(RbFile.PrimDir, "view.primdir.error.noeventselected");
                    HasError     = true;
                    return;
                }
                hits = PrimDirOrchestrator.CalculateForEvent(
                    radixChart, geoLat, natalJD, SelectedEvent.JulianDate, effectiveConfig);

                var dt = SEWrapper.DateFromJulianDay(SelectedEvent.JulianDate, true);
                EventInfoText     = $"{SelectedEvent.Title} • {dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
                PeriodText        = string.Empty;
                ShowOrbColumn     = true;
            }
            else
            {
                if (!TryParseDate(StartDateText, out var startJD))
                {
                    ErrorMessage = _rosetta.GetText(RbFile.PrimDir, "view.primdir.error.invalidstartdate");
                    HasError     = true;
                    return;
                }
                if (!TryParseDate(EndDateText, out var endJD))
                {
                    ErrorMessage = _rosetta.GetText(RbFile.PrimDir, "view.primdir.error.invalidenddate");
                    HasError     = true;
                    return;
                }
                if (startJD <= natalJD)
                {
                    ErrorMessage = _rosetta.GetText(RbFile.PrimDir, "view.primdir.error.startbeforebirth");
                    HasError     = true;
                    return;
                }
                if (endJD <= startJD)
                {
                    ErrorMessage = _rosetta.GetText(RbFile.PrimDir, "view.primdir.error.endbeforestart");
                    HasError     = true;
                    return;
                }

                hits          = PrimDirOrchestrator.Calculate(radixChart, geoLat, natalJD, startJD, endJD, effectiveConfig);
                PeriodText    = $"{StartDateText.Trim()} – {EndDateText.Trim()}";
                EventInfoText = string.Empty;
                ShowOrbColumn = false;
            }

            MethodDescription = BuildMethodDescription(effectiveConfig);

            if (hits.Count == 0)
            {
                ErrorMessage = _rosetta.GetText(RbFile.PrimDir,
                    IsEventMode ? "view.primdir.results.nohits.event" : "view.primdir.results.nohits");
                HasError = true;
                ResultRows.Clear();
            }
            else
            {
                ResultRows = new ObservableCollection<PrimDirResultRow>(
                    hits.Select((h, i) => new PrimDirResultRow(h, i)));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError     = true;
        }
    }

    public async Task LoadEventsAsync()
    {
        if (SelectedChart is null) { _rawEvents = []; RebuildEventRows(); return; }
        try
        {
            var all       = await _horoscopeRepository.FetchAllAsync();
            var horoscope = all.FirstOrDefault(h => h.Name == SelectedChart.Name);
            _horoscope = horoscope;
            if (horoscope is null) { _rawEvents = []; RebuildEventRows(); return; }
            var events = await _eventsOrchestrator.EventsForHoroscopeAsync(horoscope.Id);
            _rawEvents = events.OrderBy(e => e.JulianDate).ToList();
        }
        catch
        {
            _rawEvents = [];
        }

        RebuildEventRows();

        if (SelectedEvent is null && _progressiveSession.SelectedEvent is not null
            && _rawEvents.Any(e => e.Id == _progressiveSession.SelectedEvent.Id))
        {
            SelectedEvent = _progressiveSession.SelectedEvent;
        }
    }

    private void RebuildEventRows()
    {
        Events = new ObservableCollection<ChartEvent>(_rawEvents);
        OnPropertyChanged(nameof(HasEvents));
        CalculateCommand.NotifyCanExecuteChanged();
    }

    private void ClearResults()
    {
        ResultRows.Clear();
        ErrorMessage      = string.Empty;
        HasError          = false;
        MethodDescription = string.Empty;
        PeriodText        = string.Empty;
        EventInfoText     = string.Empty;
        ShowOrbColumn     = false;
    }

    private string BuildMethodDescription(PrimaryDirectionsConfig c) =>
        $"{_rosetta.GetText(RbFile.Localizable, c.Method.LocalizedName())} · " +
        $"{_rosetta.GetText(RbFile.Localizable, c.Approach.LocalizedName())} · " +
        $"{_rosetta.GetText(RbFile.Localizable, c.TimeKey.LocalizedName())}";

    private static bool TryParseDate(string text, out double jd)
    {
        jd = 0;
        var trimmed = text.Trim();
        var parts = trimmed.Split('/', '-', '.');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out var y)) return false;
        if (!int.TryParse(parts[1], out var m)) return false;
        if (!int.TryParse(parts[2], out var d)) return false;
        if (m < 1 || m > 12 || d < 1 || d > 31) return false;
        var date = new AstronomicalDate(y, m, d, Gregorian: true);
        var time = new AstronomicalTime(0, 0, 0);
        jd = SEWrapper.JulianDay(date, time);
        return true;
    }
}
