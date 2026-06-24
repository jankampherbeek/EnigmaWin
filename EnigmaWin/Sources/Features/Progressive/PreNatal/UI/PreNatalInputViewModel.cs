// PreNatalInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

/// <summary>Represents a single event row in the Events list inside the Correction section.</summary>
public sealed record EventDisplayRow(Guid Id, string Title, string DateText, double JulianDate);

public partial class PreNatalInputViewModel : ObservableObject
{
    private readonly PreNatalViewModel   _outer;
    private readonly IChartSession       _chartSession;
    private readonly IRosetta            _rosetta;
    private readonly EventsOrchestrator  _eventsOrchestrator;
    private readonly IHoroscopeRepository _horoscopeRepository;

    // Chart selection
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasCharts))]
    private NamedChart? _selectedChart;

    // Options
    [ObservableProperty] private bool _useEclipses    = true;
    [ObservableProperty] private bool _useIngresses   = false;
    [ObservableProperty] private bool _useRetroDirect = false;
    [ObservableProperty] private bool _useAspects     = true;

    private Factors[] _selectedFactors = PreNatalOrchestrator.DefaultFactors;
    private Aspects[] _selectedAspects = PreNatalOrchestrator.DefaultAspects;

    [ObservableProperty] private string _selectedFactorsText = string.Empty;
    [ObservableProperty] private string _selectedAspectsText = string.Empty;

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasError;

    // Conception dates (shown after Calculate)
    [ObservableProperty] private string _standardConceptionText = string.Empty;
    [ObservableProperty] private string _actualConceptionText   = string.Empty;
    private double _baseConceptionJD = 0;

    // Events list
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEvents))]
    private ObservableCollection<EventDisplayRow> _events = [];

    [ObservableProperty] private EventDisplayRow? _selectedEvent;

    public bool HasEvents => Events.Count > 0;

    // Correction section
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanCombine))]
    private string _knownDateText = string.Empty;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanCombine))]
    private string _knownTimeText = "12:00:00";

    [ObservableProperty] private bool _isCombined = false;
    [ObservableProperty] private string _combineErrorMessage = string.Empty;
    [ObservableProperty] private bool   _hasCombineError;

    public bool HasCharts  => _chartSession.Charts.Count > 0;
    public bool CanCombine => _outer.SelectedMomentRow is not null && !string.IsNullOrWhiteSpace(KnownDateText);
    public IReadOnlyList<NamedChart> Charts => _chartSession.Charts;
    public double NatalJD => SelectedChart?.Chart.JulianDay ?? 0.0;

    // Forward results from outer VM for the results panel
    public ObservableCollection<PreNatalResultRow> ResultRows => _outer.ResultRows;
    public bool HasResults => _outer.HasResults;

    // Labels — input screen
    public string LabelTitle             => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.title");
    public string LabelNoSession         => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.nosession");
    public string LabelChartSection      => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.chartsection");
    public string LabelOptionsHeader     => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.options.header");
    public string LabelUseEclipses       => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.options.eclipses");
    public string LabelUseIngresses      => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.options.ingresses");
    public string LabelUseRetroDirect    => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.options.retrodirect");
    public string LabelUseAspects        => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.options.aspects");
    public string LabelFactorsBtn        => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.factors.button");
    public string LabelAspectsBtn        => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.aspects.button");
    public string LabelCalculate         => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.calculate");
    public string LabelHelp              => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.help");
    public string LabelHelpTitle         => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.help.title");
    public string LabelHelpClose         => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.help.close");
    public string LabelFactsheetTitle    => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.factsheet.title");
    public string LabelFactsheetTooltip  => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.factsheet.tooltip");
    // Conception section
    public string LabelConceptionHeader  => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.conception.header");
    public string LabelConceptionStd     => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.conception.standard");
    public string LabelConceptionActual  => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.conception.actual");
    // Correction section
    public string LabelCorrectionHeader  => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.combine.header");
    public string LabelEventsHeader      => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.events.header");
    public string LabelEventsDescription => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.events.description");
    public string LabelEventsDate        => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.events.date");
    public string LabelEventsEmpty       => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.events.empty");
    public string LabelEventsNoSave      => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.events.nosave");
    public string LabelSelectMoment      => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.results.selectmoment");
    public string LabelCombineDateLabel  => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.combine.date");
    public string LabelCombineTimeLabel  => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.combine.time");
    public string LabelCombineBtn        => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.combine.button");
    public string LabelResetBtn          => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.reset.button");

    public PreNatalInputViewModel(
        PreNatalViewModel    outer,
        IChartSession        chartSession,
        IRosetta             rosetta,
        EventsOrchestrator   eventsOrchestrator,
        IHoroscopeRepository horoscopeRepository)
    {
        _outer               = outer;
        _chartSession        = chartSession;
        _rosetta             = rosetta;
        _eventsOrchestrator  = eventsOrchestrator;
        _horoscopeRepository = horoscopeRepository;

        if (_chartSession.Selected is not null)
            SelectedChart = _chartSession.Selected;

        _chartSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(IChartSession.Charts) or nameof(IChartSession.Selected))
            {
                OnPropertyChanged(nameof(Charts));
                OnPropertyChanged(nameof(HasCharts));
                if (_chartSession.Selected is not null)
                    SelectedChart = _chartSession.Selected;
            }
        };

        _outer.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(PreNatalViewModel.HasResults) or nameof(PreNatalViewModel.ResultRows))
            {
                OnPropertyChanged(nameof(ResultRows));
                OnPropertyChanged(nameof(HasResults));
            }
            if (args.PropertyName is nameof(PreNatalViewModel.SelectedMomentRow))
            {
                OnPropertyChanged(nameof(CanCombine));
                CombineCommand.NotifyCanExecuteChanged();
            }
        };

        UpdateFactorsText();
        UpdateAspectsText();
    }

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        _outer.ClearResults();
        ClearCorrectionState();
        UpdateConceptionDate();
        CalculateCommand.NotifyCanExecuteChanged();
        LoadEventsAsync();
    }

    partial void OnSelectedEventChanged(EventDisplayRow? value)
    {
        if (value is null) return;
        // Populate date/time fields from the selected event
        var dt = SEWrapper.DateFromJulianDay(value.JulianDate, true);
        KnownDateText = $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
        KnownTimeText = $"{dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{Math.Min(dt.Time.Second, 59):D2}";
    }

    internal void TriggerCalculate() => Calculate();

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        if (SelectedChart is null) return;

        ErrorMessage = string.Empty;
        HasError     = false;
        ClearCorrectionState();

        try
        {
            var natalJD       = SelectedChart.Chart.JulianDay;
            _baseConceptionJD = natalJD - 273.217;
            UpdateConceptionDateTexts(_baseConceptionJD, _baseConceptionJD);

            var moments = PreNatalOrchestrator.Calculate(
                natalJD, _baseConceptionJD,
                _selectedFactors, _selectedAspects,
                UseAspects, UseEclipses, UseIngresses, UseRetroDirect);

            _outer.SetResults(moments);
        }
        catch (Exception ex)
        {
            _outer.SetError(ex.Message);
        }
    }

    private bool CanCalculate() => HasCharts && SelectedChart is not null;

    [RelayCommand(CanExecute = nameof(CanCombine))]
    private void Combine()
    {
        CombineErrorMessage = string.Empty;
        HasCombineError     = false;

        if (_outer.SelectedMomentRow is null)
        {
            CombineErrorMessage = _rosetta.GetText(RbFile.PreNatal, "view.prenatal.combine.nomoment");
            HasCombineError = true;
            return;
        }

        if (!TryParseDateTime(KnownDateText, KnownTimeText, out var eventJD))
        {
            CombineErrorMessage = _rosetta.GetText(RbFile.PreNatal, "view.prenatal.combine.invaliddate");
            HasCombineError = true;
            return;
        }

        const double conversionFactor = 365.242199074 * (70.0 / 273.217);
        var momentJD       = _outer.SelectedMomentRow.ActualJD;
        var correctedConJD = _baseConceptionJD + (momentJD - eventJD) / conversionFactor;
        UpdateConceptionDateTexts(_baseConceptionJD, correctedConJD);

        if (SelectedChart is not null)
        {
            var natalJD = SelectedChart.Chart.JulianDay;
            var moments = PreNatalOrchestrator.Calculate(
                natalJD, correctedConJD,
                _selectedFactors, _selectedAspects,
                UseAspects, UseEclipses, UseIngresses, UseRetroDirect);
            _outer.SetResults(moments);
        }

        IsCombined = true;
    }

    [RelayCommand]
    private void Reset()
    {
        ClearCorrectionState();
        if (SelectedChart is not null)
        {
            var natalJD = SelectedChart.Chart.JulianDay;
            _baseConceptionJD = natalJD - 273.217;
            UpdateConceptionDateTexts(_baseConceptionJD, _baseConceptionJD);
            var moments = PreNatalOrchestrator.Calculate(
                natalJD, _baseConceptionJD,
                _selectedFactors, _selectedAspects,
                UseAspects, UseEclipses, UseIngresses, UseRetroDirect);
            _outer.SetResults(moments);
        }
    }

    internal void ApplyFactors(Factors[] factors)
    {
        _selectedFactors = factors;
        UpdateFactorsText();
    }

    internal void ApplyAspects(Aspects[] aspects)
    {
        _selectedAspects = aspects;
        UpdateAspectsText();
    }

    internal Factors[] GetSelectedFactors() => _selectedFactors;
    internal Aspects[] GetSelectedAspects() => _selectedAspects;

    private void UpdateFactorsText()
        => SelectedFactorsText = $"{_selectedFactors.Length} factors";

    private void UpdateAspectsText()
        => SelectedAspectsText = $"{_selectedAspects.Length} aspects";

    private void UpdateConceptionDate()
    {
        if (SelectedChart is null) return;
        var natalJD = SelectedChart.Chart.JulianDay;
        _baseConceptionJD = natalJD - 273.217;
        UpdateConceptionDateTexts(_baseConceptionJD, _baseConceptionJD);
    }

    private void UpdateConceptionDateTexts(double stdJD, double actualJD)
    {
        var stdDt    = SEWrapper.DateFromJulianDay(stdJD, true);
        var actualDt = SEWrapper.DateFromJulianDay(actualJD, true);
        StandardConceptionText = FormatDateTime(stdDt);
        ActualConceptionText   = FormatDateTime(actualDt);
    }

    private void ClearCorrectionState()
    {
        IsCombined          = false;
        KnownDateText       = string.Empty;
        KnownTimeText       = "12:00:00";
        SelectedEvent       = null;
        CombineErrorMessage = string.Empty;
        HasCombineError     = false;
    }

    private async void LoadEventsAsync()
    {
        if (SelectedChart is null)
        {
            Events = [];
            return;
        }

        try
        {
            // NamedChart.Id is a session-only GUID; look up the DB horoscope by name
            var allHoroscopes = await _horoscopeRepository.FetchAllAsync();
            var horoscope = allHoroscopes.FirstOrDefault(h => h.Name == SelectedChart.Name);
            if (horoscope is null)
            {
                Events = [];
                return;
            }

            var chartEvents = await _eventsOrchestrator.EventsForHoroscopeAsync(horoscope.Id);
            var rows = chartEvents.Select(e =>
            {
                var dt      = SEWrapper.DateFromJulianDay(e.JulianDate, true);
                var dateTxt = $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
                return new EventDisplayRow(e.Id, e.Title, dateTxt, e.JulianDate);
            }).ToList();

            Events = new ObservableCollection<EventDisplayRow>(rows);
        }
        catch
        {
            Events = [];
        }
    }

    private static string FormatDateTime(AstronomicalDateTime dt)
        => $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2} {dt.Time.Hour:D2}:{dt.Time.Minute:D2}:{Math.Min(dt.Time.Second, 59):D2}";

    private static bool TryParseDateTime(string dateText, string timeText, out double jd)
    {
        jd = 0;
        var dp = dateText.Trim().Split('/', '-', '.');
        if (dp.Length != 3) return false;
        if (!int.TryParse(dp[0], out var y)) return false;
        if (!int.TryParse(dp[1], out var m)) return false;
        if (!int.TryParse(dp[2], out var d)) return false;
        if (m < 1 || m > 12 || d < 1 || d > 31) return false;

        var tp = timeText.Trim().Split(':');
        if (tp.Length < 2) return false;
        if (!int.TryParse(tp[0], out var h)) return false;
        if (!int.TryParse(tp[1], out var min)) return false;
        var sec = tp.Length > 2 && int.TryParse(tp[2], out var s) ? Math.Min(s, 59) : 0;
        if (h < 0 || h > 23 || min < 0 || min > 59) return false;

        var date = new AstronomicalDate(y, m, d, Gregorian: true);
        var time = new AstronomicalTime(h, min, sec);
        jd = SEWrapper.JulianDay(date, time);
        return true;
    }
}
