// RadixOverviewViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

public sealed partial class RadixOverviewViewModel : ObservableObject
{
    private readonly IChartSession        _chartSession;
    private readonly INavigationService   _navigationService;
    private readonly IRosetta             _rosetta;
    private readonly IHoroscopeRepository _horoscopeRepository;

    public ObservableCollection<SessionChartRow> SessionRows { get; } = [];

    public bool HasRows   => SessionRows.Count > 0;
    public bool HasNoRows => SessionRows.Count == 0;

    public string LabelTitle        => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.title");
    public string LabelNewChart     => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.newchart");
    public string LabelSearchChart  => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.searchchart");
    public string LabelSessionGroup => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.sessiongroup");
    public string LabelNoCharts     => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.nocharts");
    public string LabelColName      => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.column.name");
    public string LabelColJulianDay => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.column.julianday");
    public string LabelSelect       => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.select");
    public string LabelEdit         => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.edit");
    public string TooltipHelp       => _rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.help.tooltip");

    public RadixOverviewViewModel(
        IChartSession        chartSession,
        INavigationService   navigationService,
        IRosetta             rosetta,
        IHoroscopeRepository horoscopeRepository)
    {
        _chartSession        = chartSession;
        _navigationService   = navigationService;
        _rosetta             = rosetta;
        _horoscopeRepository = horoscopeRepository;

        _chartSession.PropertyChanged += OnSessionPropertyChanged;
        RefreshRows();
    }

    internal void NewChart()
    {
        _navigationService.NavigateDetail(AppRoutes.RadixInput,
            new RadixInputNavigationParameter(System.Guid.NewGuid()));
    }

    internal void SearchChart()
    {
        _navigationService.NavigateDetail(AppRoutes.RadixSearch);
    }

    internal void Select(SessionChartRow row)
    {
        _chartSession.Select(row.NamedChart);
        _navigationService.NavigateMain(AppRoutes.RadixChart);
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    internal async Task EditAsync(SessionChartRow row)
    {
        var all = await _horoscopeRepository.FetchAllAsync();
        var horoscope = all.FirstOrDefault(h => h.Name == row.Name);

        if (horoscope is not null)
        {
            var withDateTimes = await _horoscopeRepository.FetchAsync(horoscope.Id);
            _chartSession.EditingHoroscope = withDateTimes;
        }

        _chartSession.EditingNamedChart = row.NamedChart;
        _navigationService.NavigateDetail(AppRoutes.RadixEdit);
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IChartSession.Charts))
            RefreshRows();
    }

    private void RefreshRows()
    {
        var labelSelect = LabelSelect;
        var labelEdit   = LabelEdit;
        SessionRows.Clear();
        foreach (var namedChart in _chartSession.Charts)
            SessionRows.Add(new SessionChartRow(namedChart, labelSelect, labelEdit));

        OnPropertyChanged(nameof(HasRows));
        OnPropertyChanged(nameof(HasNoRows));
    }
}
