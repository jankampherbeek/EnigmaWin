// RadixSearchViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixSearch.UI;

public sealed partial class RadixSearchViewModel : ObservableObject
{
    private readonly IHoroscopeRepository  _repository;
    private readonly INavigationService    _navigationService;
    private readonly IChartSession         _chartSession;
    private readonly IRosetta              _rosetta;
    private readonly IConfigContext?       _configContext;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults), nameof(HasNoResults))]
    private string _searchText = string.Empty;

    public ObservableCollection<HoroscopeSearchRow> SearchResults { get; } = [];

    public bool HasResults   => SearchResults.Count > 0;
    public bool HasNoResults => SearchResults.Count == 0;

    public string LabelTitle      => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.title");
    public string LabelPartOfName => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.partofname");
    public string LabelSearch     => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.search");
    public string LabelNoResults  => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.noresults");
    public string LabelColName     => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.column.name");
    public string LabelColDateTime => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.column.datetime");
    public string LabelColLocation => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.column.location");
    public string LabelDeleteTitle   => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.delete.title");
    public string LabelDeleteMessage => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.delete.message");
    public string LabelDeleteConfirm => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.delete.confirm");
    public string LabelDeleteCancel  => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.delete.cancel");
    public string TooltipHelp        => _rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.help.tooltip");

    public RadixSearchViewModel(
        IHoroscopeRepository repository,
        INavigationService   navigationService,
        IChartSession        chartSession,
        IRosetta             rosetta,
        IConfigContext?      configContext = null)
    {
        _repository        = repository;
        _navigationService = navigationService;
        _chartSession      = chartSession;
        _rosetta           = rosetta;
        _configContext     = configContext;
    }

    internal async Task SearchAsync()
    {
        var rows = await RadixSearchModel.SearchAsync(SearchText, _repository, _rosetta);
        SearchResults.Clear();
        foreach (var row in rows)
            SearchResults.Add(row);

        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(HasNoResults));
    }

    internal void Select(HoroscopeSearchRow row)
    {
        var factorConfig = _configContext?.ActiveConfig.FactorConfig;
        var calcConfig  = _configContext?.ActiveConfig.CalculationConfig;
        var chart = RadixSearchModel.CalculateChart(row, factorConfig, calcConfig);
        _chartSession.Add(row.Name, chart, row.Latitude, row.Longitude, 0.0);
        _navigationService.NavigateMain(AppRoutes.RadixChart);
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    internal async Task DeleteAsync(HoroscopeSearchRow row)
    {
        await _repository.DeleteAsync(row.HoroscopeId);
        SearchResults.Remove(row);
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(HasNoResults));
    }
}
