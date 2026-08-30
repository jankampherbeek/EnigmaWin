// SynastryInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixSearch.UI;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public sealed partial class SynastryInputViewModel : ObservableObject
{
    private readonly IHoroscopeRepository _repository;
    private readonly INavigationService   _navigationService;
    private readonly SynastryModel        _synastryModel;
    private readonly IConfigContext       _configContext;
    private readonly IRosetta             _rosetta;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchResults), nameof(HasNoSearchResults))]
    private string _searchText = string.Empty;

    public ObservableCollection<HoroscopeSearchRow> SearchResults { get; } = [];

    public bool HasSearchResults   => SearchResults.Count > 0;
    public bool HasNoSearchResults => SearchResults.Count == 0;

    public ObservableCollection<NamedChart> SelectedCharts { get; } = [];

    public bool HasNoSelectedCharts => SelectedCharts.Count == 0;
    public bool HasExactlyTwo       => _synastryModel.HasExactlyTwo;
    public bool HasTwoOrMore        => _synastryModel.HasTwoOrMore;

    public SynastryInputViewModel(
        IHoroscopeRepository repository,
        INavigationService   navigationService,
        SynastryModel        synastryModel,
        IConfigContext       configContext,
        IRosetta             rosetta)
    {
        _repository        = repository;
        _navigationService = navigationService;
        _synastryModel      = synastryModel;
        _configContext      = configContext;
        _rosetta            = rosetta;

        foreach (var chart in _synastryModel.SelectedCharts)
            SelectedCharts.Add(chart);
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle            => T("view.synastry.title");
    public string LabelSelectedHeader   => T("view.synastry.selected.header");
    public string LabelSelectedEmpty    => T("view.synastry.selected.empty");
    public string LabelSelectedMinHint  => T("view.synastry.selected.minhint");
    public string LabelRemove           => T("view.synastry.selected.remove");
    public string LabelSearchHeader     => T("view.synastry.search.header");
    public string LabelPartOfName       => T("view.synastry.search.partofname");
    public string LabelSearchButton     => T("view.synastry.search.button");
    public string LabelSearchNoResults  => T("view.synastry.search.noresults");
    public string LabelButtonCompare              => T("view.synastry.button.compare");
    public string LabelButtonComposite            => T("view.synastry.button.composite");
    public string LabelButtonCombine              => T("view.synastry.button.combine");
    public string LabelButtonAspectComparison     => T("view.synastry.button.aspectcomparison");
    public string LabelButtonMidpointComparison   => T("view.synastry.button.midpointcomparison");
    public string LabelButtonDeclinationComparison => T("view.synastry.button.declinationcomparison");
    public string LabelHelp             => T("view.synastry.help.input");

    // ── Search ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SearchAsync()
    {
        var rows = await RadixSearchModel.SearchAsync(SearchText, _repository, _rosetta);
        SearchResults.Clear();
        foreach (var row in rows)
            SearchResults.Add(row);
        OnPropertyChanged(nameof(HasSearchResults));
        OnPropertyChanged(nameof(HasNoSearchResults));
    }

    public void AddFromSearch(HoroscopeSearchRow row)
    {
        var factorConfig = _configContext.ActiveConfig.FactorConfig;
        var calcConfig   = _configContext.ActiveConfig.CalculationConfig;
        var chart = RadixSearchModel.CalculateChart(row, factorConfig, calcConfig);
        AddChart(new NamedChart(row.HoroscopeId, row.Name, chart, row.Latitude, row.Longitude, 0.0), new GeoLocation(row.Latitude, row.Longitude));
    }

    public void RemoveChart(NamedChart chart)
    {
        _synastryModel.Remove(chart);
        SelectedCharts.Remove(chart);
        NotifySelectionChanged();
    }

    private void AddChart(NamedChart chart, GeoLocation? location = null)
    {
        var before = _synastryModel.SelectedCharts.Count;
        _synastryModel.Add(chart, location);
        if (_synastryModel.SelectedCharts.Count == before) return; // duplicate, ignored

        SelectedCharts.Add(chart);
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(HasNoSelectedCharts));
        OnPropertyChanged(nameof(HasExactlyTwo));
        OnPropertyChanged(nameof(HasTwoOrMore));
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    [RelayCommand]
    private void OpenCompare()
    {
        if (!HasExactlyTwo) return;
        _navigationService.NavigateDetail(AppRoutes.SynastryCompare);
    }

    [RelayCommand]
    private void OpenAspectComparison()
    {
        if (!HasExactlyTwo) return;
        _navigationService.NavigateDetail(AppRoutes.SynastryAspectComparison);
    }

    [RelayCommand]
    private void OpenComposite()
    {
        if (!HasTwoOrMore) return;
        _navigationService.NavigateDetail(AppRoutes.SynastryComposite);
    }

    [RelayCommand]
    private void OpenCombine()
    {
        if (!HasTwoOrMore) return;
        _navigationService.NavigateDetail(AppRoutes.SynastryCombine);
    }

    [RelayCommand]
    private void OpenMidpointComparison()
    {
        if (!HasExactlyTwo) return;
        _navigationService.NavigateDetail(AppRoutes.SynastryMidpointComparison);
    }

    [RelayCommand]
    private void OpenDeclinationComparison()
    {
        if (!HasExactlyTwo) return;
        _navigationService.NavigateDetail(AppRoutes.SynastryDeclinationComparison);
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);
}
