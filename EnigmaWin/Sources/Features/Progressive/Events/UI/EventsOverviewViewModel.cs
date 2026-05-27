// EventsOverviewViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.Events.UI;

public partial class EventsOverviewViewModel : ObservableObject
{
    private readonly EventsOrchestrator _orchestrator;
    private readonly IHoroscopeRepository _horoscopeRepository;
    private readonly IChartSession _chartSession;
    private readonly IProgressiveSession _progressiveSession;
    private readonly INavigationService _navigationService;
    private readonly IRosetta _rosetta;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasEvents))]
    private ObservableCollection<EventRow> _eventRows = [];
    [ObservableProperty] private ChartEvent? _selectedEvent;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private NamedChart? _selectedChart;

    public bool HasEvents => EventRows.Count > 0;
    private List<ChartEvent> _rawEvents = [];
    private Guid? _currentHoroscopeId;

    public IReadOnlyList<NamedChart> Charts => _chartSession.Charts;
    public bool HasCharts => _chartSession.Charts.Count > 0;

    public string LabelTitle          => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.title");
    public string LabelChartSection   => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.chartsection");
    public string LabelNoSession      => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.nosession");
    public string LabelEventsHeader   => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.eventsheader");
    public string LabelEmpty          => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.empty");
    public string LabelCreate         => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.create");
    public string LabelColTitle       => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.column.title");
    public string LabelColDateTime    => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.column.datetime");
    public string LabelColLocation    => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.column.location");
    public string LabelSelect         => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.select");
    public string LabelEdit           => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.edit");
    public string LabelDelete         => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.delete");
    public string LabelDeleteTitle    => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.delete.title");
    public string LabelDeleteConfirm  => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.delete.confirm");
    public string LabelDeleteCancel   => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.delete.cancel");
    public string LabelHelp           => _rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.help");

    public string DeleteMessageFor(ChartEvent e) =>
        string.Format(_rosetta.GetText(RbFile.EventsOverview, "view.eventsoverviewscreen.delete.message"), e.Title);

    public EventsOverviewViewModel(
        EventsOrchestrator orchestrator,
        IHoroscopeRepository horoscopeRepository,
        IChartSession chartSession,
        IProgressiveSession progressiveSession,
        INavigationService navigationService,
        IRosetta rosetta)
    {
        _orchestrator        = orchestrator;
        _horoscopeRepository = horoscopeRepository;
        _chartSession        = chartSession;
        _progressiveSession  = progressiveSession;
        _navigationService   = navigationService;
        _rosetta             = rosetta;

        SelectedEvent = _progressiveSession.SelectedEvent;
        _progressiveSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IProgressiveSession.SelectedEvent))
                SelectedEvent = _progressiveSession.SelectedEvent;
        };

        _chartSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IChartSession.Charts) ||
                args.PropertyName == nameof(IChartSession.Selected))
            {
                OnPropertyChanged(nameof(Charts));
                OnPropertyChanged(nameof(HasCharts));
            }
        };

        if (_chartSession.Selected is not null)
        {
            SelectedChart = _chartSession.Selected;
            _ = LoadEventsAsync();
        }
    }

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        if (value is null) return;
        var wasDifferent = _currentHoroscopeId.HasValue;
        _ = LoadEventsAsync(clearSelectionIfChanged: wasDifferent);
    }

    public async Task LoadEventsAsync(bool clearSelectionIfChanged = false)
    {
        if (SelectedChart is null) return;
        try
        {
            var all = await _horoscopeRepository.FetchAllAsync();
            var horoscope = all.FirstOrDefault(h => h.Name == SelectedChart.Name);
            if (horoscope is null) { EventRows.Clear(); _rawEvents.Clear(); return; }

            if (clearSelectionIfChanged && _currentHoroscopeId.HasValue && _currentHoroscopeId != horoscope.Id)
                _progressiveSession.ClearSelection();

            _currentHoroscopeId = horoscope.Id;
            var events = await _orchestrator.EventsForHoroscopeAsync(horoscope.Id);
            _rawEvents = events.ToList();
            RebuildRows();
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
        RebuildRows();
    }

    public async Task DeleteEventAsync(ChartEvent chartEvent)
    {
        try
        {
            _progressiveSession.ClearIfDeleted(chartEvent.Id);
            await _orchestrator.DeleteAsync(chartEvent.Id);
            await LoadEventsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError = true;
        }
    }

    private void RebuildRows()
    {
        EventRows = new ObservableCollection<EventRow>(
            _rawEvents.Select((e, i) => new EventRow(
                e, i, e.Id == SelectedEvent?.Id,
                LabelSelect, LabelEdit, LabelDelete)));
    }

    [RelayCommand]
    private void NavigateToCreateEvent()
    {
        if (_currentHoroscopeId is null) return;
        _navigationService.NavigateDetail(AppRoutes.ProgressiveEventInput,
            new ProgressiveEventNavigationParameter(_currentHoroscopeId.Value));
    }

    [RelayCommand]
    private void NavigateToEditEvent(ChartEvent chartEvent)
    {
        _navigationService.NavigateDetail(AppRoutes.ProgressiveEventEdit,
            new ProgressiveEventEditNavigationParameter(chartEvent));
    }
}
