// MainWindowViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using System;
using System.ComponentModel;

namespace EnigmaWin.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IChartSession _chartSession;
    private readonly IConfigContext _configContext;
    private readonly IRouteViewModelFactory _routeViewModelFactory;

    public string Welcome { get; }
    public IRelayCommand SelectRadixCommand { get; }
    public IRelayCommand SelectConfigurationCommand { get; }
    public IRelayCommand SelectResearchCommand { get; }
    public IRelayCommand SelectResearchProjectsCommand { get; }
    public IRelayCommand SelectCyclesCommand { get; }
    public IRelayCommand SelectCyclesAstronomicalCommand { get; }
    public IRelayCommand SelectCyclesWavesCommand { get; }
    public IRelayCommand SelectCalculatorsCommand { get; }
    public IRelayCommand SelectCalculatorsJulianDayCommand { get; }
    public IRelayCommand SelectCalculatorsObliquityCommand { get; }
    public IRelayCommand SelectProgressiveCommand { get; }
    public IRelayCommand SelectProgressiveEventsCommand { get; }
    public IRelayCommand SelectProgressiveTransitCommand { get; }
    public IRelayCommand SelectProgressiveSecondaryCommand { get; }
    public IRelayCommand SelectProgressiveSymbolicCommand { get; }
    public IRelayCommand SelectProgressiveLogTimeScaleCommand { get; }
    public IRelayCommand ShowOverviewCommand { get; }
    public IRelayCommand ShowPositionsCommand { get; }
    public IRelayCommand SearchRadixCommand { get; }
    public IRelayCommand NewChartCommand { get; }
    public IRelayCommand NewConfigurationCommand { get; }
    public IRelayCommand EditConfigurationCommand { get; }
    public IRelayCommand ShowAnalysisCommand { get; }
    public IRelayCommand ShowDeclinationsCommand { get; }
    public IRelayCommand BackMainCommand { get; }
    public IRelayCommand BackDetailCommand { get; }

    [ObservableProperty]
    private object? currentMainViewModel;

    [ObservableProperty]
    private object? currentDetailViewModel;

    public MainWindowViewModel(
        INavigationService navigationService,
        IChartSession chartSession,
        IConfigContext configContext,
        IRouteViewModelFactory routeViewModelFactory,
        IRosetta rosetta)
    {
        _navigationService = navigationService;
        _chartSession = chartSession;
        _configContext = configContext;
        _routeViewModelFactory = routeViewModelFactory;
        Welcome = rosetta.GetText(RbFile.Localizable, "welcome");

        SelectRadixCommand = new RelayCommand(SelectRadix);
        SelectConfigurationCommand = new RelayCommand(SelectConfiguration);
        SelectResearchCommand = new RelayCommand(SelectResearch);
        SelectResearchProjectsCommand = new RelayCommand(OpenResearchProjects);
        SelectCyclesCommand = new RelayCommand(SelectCycles);
        SelectCyclesAstronomicalCommand = new RelayCommand(OpenCyclesAstronomical);
        SelectCyclesWavesCommand = new RelayCommand(OpenCyclesWaves);
        SelectCalculatorsCommand           = new RelayCommand(SelectCalculators);
        SelectCalculatorsJulianDayCommand  = new RelayCommand(OpenCalculatorsJulianDay);
        SelectCalculatorsObliquityCommand  = new RelayCommand(OpenCalculatorsObliquity);
        SelectProgressiveCommand          = new RelayCommand(SelectProgressive);
        SelectProgressiveEventsCommand    = new RelayCommand(OpenProgressiveEvents);
        SelectProgressiveTransitCommand   = new RelayCommand(OpenProgressiveTransit);
        SelectProgressiveSecondaryCommand = new RelayCommand(OpenProgressiveSecondary);
        SelectProgressiveSymbolicCommand          = new RelayCommand(OpenProgressiveSymbolic);
        SelectProgressiveLogTimeScaleCommand      = new RelayCommand(OpenProgressiveLogTimeScale);
        ShowOverviewCommand = new RelayCommand(OpenRadixOverview);
        ShowPositionsCommand = new RelayCommand(OpenRadixPositions);
        SearchRadixCommand  = new RelayCommand(OpenRadixSearch);
        NewChartCommand = new RelayCommand(OpenNewChart);
        NewConfigurationCommand = new RelayCommand(OpenNewConfiguration);
        EditConfigurationCommand = new RelayCommand(OpenEditConfiguration);
        ShowAnalysisCommand     = new RelayCommand(OpenRadixAnalysis);
        ShowDeclinationsCommand = new RelayCommand(OpenRadixDeclinations);
        BackMainCommand = new RelayCommand(_navigationService.GoBackMain);
        BackDetailCommand = new RelayCommand(_navigationService.GoBackDetail);

        _navigationService.PropertyChanged += OnNavigationServicePropertyChanged;
        _navigationService.NavigateMain(AppRoutes.MainRadixHome);
        _navigationService.NavigateDetail(AppRoutes.RadixOverview);
        UpdateCurrentMainViewModel();
        UpdateCurrentDetailViewModel();
    }

    [ObservableProperty]
    private string activeSection = "Radix";

    public bool ShowRadixButtons        => ActiveSection == "Radix";
    public bool ShowConfigurationButtons => ActiveSection == "Configuration";
    public bool ShowResearchButtons     => ActiveSection == "Research";
    public bool ShowCyclesButtons       => ActiveSection == "Cycles";
    public bool ShowCalculatorsButtons  => ActiveSection == "Calculators";
    public bool ShowProgressiveButtons  => ActiveSection == "Progressive";
    public bool ShowDetailPane          => ActiveSection != "Research";
    public int  MainViewColumnSpan     => ShowDetailPane ? 1 : 3;
    public bool CanGoBackMain => _navigationService.CanGoBackMain;
    public bool CanGoBackDetail => _navigationService.CanGoBackDetail;
    public bool ShowView1Placeholder => CurrentMainViewModel == null;
    public bool ShowView2Placeholder => CurrentDetailViewModel == null;

    private void SelectRadix()
    {
        if (ActiveSection == "Radix")
        {
            NavigateRadixMain();
            return;
        }
        SetActiveSection("Radix");
    }

    private void SelectConfiguration()
    {
        SetActiveSection("Configuration");
    }

    private void SelectResearch()
    {
        SetActiveSection("Research");
    }

    private void SelectCycles()
    {
        SetActiveSection("Cycles");
    }

    private void SelectCalculators()
    {
        SetActiveSection("Calculators");
    }

    private void OpenCalculatorsJulianDay()
    {
        if (ActiveSection != "Calculators") return;
        _navigationService.NavigateDetail(AppRoutes.CalculatorsJulianDay);
    }

    private void OpenCalculatorsObliquity()
    {
        if (ActiveSection != "Calculators") return;
        _navigationService.NavigateDetail(AppRoutes.CalculatorsObliquity);
    }

    private void SelectProgressive()
    {
        SetActiveSection("Progressive");
    }

    private void OpenProgressiveEvents()
    {
        if (ActiveSection != "Progressive") return;
        _navigationService.NavigateDetail(AppRoutes.ProgressiveEventsOverview);
    }

    private void OpenProgressiveTransit()
    {
        if (ActiveSection != "Progressive") return;
        _navigationService.NavigateMain(AppRoutes.ProgressiveTransit);
        _navigationService.NavigateDetail(AppRoutes.ProgressiveTransitInput);
    }

    private void OpenProgressiveSecondary()
    {
        if (ActiveSection != "Progressive") return;
        _navigationService.NavigateMain(AppRoutes.ProgressiveSecondary);
        _navigationService.NavigateDetail(AppRoutes.ProgressiveSecondaryInput);
    }

    private void OpenProgressiveSymbolic()
    {
        if (ActiveSection != "Progressive") return;
        _navigationService.NavigateMain(AppRoutes.ProgressiveSymbolic);
        _navigationService.NavigateDetail(AppRoutes.ProgressiveSymbolicInput);
    }

    private void OpenProgressiveLogTimeScale()
    {
        if (ActiveSection != "Progressive") return;
        _navigationService.NavigateMain(AppRoutes.ProgressiveLogTimeScale);
        _navigationService.NavigateDetail(AppRoutes.ProgressiveLogTimeScaleInput);
    }

    private void OpenResearchProjects()
    {
        if (ActiveSection != "Research") return;
        _navigationService.NavigateMain(AppRoutes.ResearchProjects);
        _navigationService.NavigateDetail(AppRoutes.None);
    }

    private void OpenRadixOverview()
    {
        if (ActiveSection != "Radix") return;
        _navigationService.NavigateDetail(AppRoutes.RadixOverview);
    }

    private void OpenRadixPositions()
    {
        if (ActiveSection != "Radix") return;
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    private void OpenRadixSearch()
    {
        if (ActiveSection != "Radix") return;
        _navigationService.NavigateDetail(AppRoutes.RadixSearch);
    }

    private void OpenRadixAnalysis()
    {
        if (ActiveSection != "Radix") return;
        _navigationService.NavigateDetail(AppRoutes.RadixAnalysis);
    }

    private void OpenRadixDeclinations()
    {
        if (ActiveSection != "Radix") return;
        _navigationService.NavigateDetail(AppRoutes.RadixDeclinations);
    }

    private void OpenNewChart()
    {
        if (ActiveSection != "Radix") return;
        _navigationService.NavigateDetail(AppRoutes.RadixInput, new RadixInputNavigationParameter(Guid.NewGuid()));
    }

    private void OpenNewConfiguration()
    {
        if (ActiveSection != "Configuration") return;
        _navigationService.NavigateDetail(AppRoutes.ConfigEditor, new ConfigEditorNavigationParameter(ConfigEditorMode.New));
    }

    private void OpenEditConfiguration()
    {
        if (ActiveSection != "Configuration") return;
        _navigationService.NavigateDetail(AppRoutes.ConfigEditor, new ConfigEditorNavigationParameter(ConfigEditorMode.Edit));
    }

    public void ShowRadixPositionsFromCalculation(string name, FullChart chart)
    {
        _chartSession.Add(name, chart);
        NavigateRadixMain();
        _navigationService.NavigateDetail(AppRoutes.RadixPositions);
    }

    partial void OnActiveSectionChanged(string value)
    {
        OnPropertyChanged(nameof(ShowRadixButtons));
        OnPropertyChanged(nameof(ShowConfigurationButtons));
        OnPropertyChanged(nameof(ShowResearchButtons));
        OnPropertyChanged(nameof(ShowCyclesButtons));
        OnPropertyChanged(nameof(ShowCalculatorsButtons));
        OnPropertyChanged(nameof(ShowProgressiveButtons));
        OnPropertyChanged(nameof(ShowDetailPane));
        OnPropertyChanged(nameof(MainViewColumnSpan));

        switch (value)
        {
            case "Radix":
                NavigateRadixMain();
                _navigationService.NavigateDetail(AppRoutes.RadixOverview);
                break;
            case "Configuration":
                _navigationService.NavigateMain(AppRoutes.MainConfigHome);
                var activeConfig = _configContext.EditingConfig ?? _configContext.ActiveConfig;
                _configContext.EditingConfig = activeConfig;
                _navigationService.NavigateDetail(AppRoutes.ConfigEdit, new ConfigEditNavigationParameter(activeConfig.Id));
                break;
            case "Cycles":
                _navigationService.NavigateMain(AppRoutes.CyclesAstronomical);
                _navigationService.NavigateDetail(AppRoutes.CyclesAstronomicalInput);
                break;
            case "Calculators":
                _navigationService.NavigateMain(AppRoutes.MainCalculatorsHome);
                _navigationService.NavigateDetail(AppRoutes.CalculatorsJulianDay);
                break;
            case "Progressive":
                _navigationService.NavigateMain(AppRoutes.MainProgressiveHome);
                _navigationService.NavigateDetail(AppRoutes.ProgressiveEventsOverview);
                break;
            default:
                _navigationService.NavigateMain(AppRoutes.ResearchProjects);
                _navigationService.NavigateDetail(AppRoutes.None);
                break;
        }
    }

    private void OpenCyclesAstronomical()
    {
        SetActiveSection("Cycles");
        _navigationService.NavigateMain(AppRoutes.CyclesAstronomical);
        _navigationService.NavigateDetail(AppRoutes.CyclesAstronomicalInput);
    }

    private void OpenCyclesWaves()
    {
        SetActiveSection("Cycles");
        _navigationService.NavigateMain(AppRoutes.CyclesWaves);
        _navigationService.NavigateDetail(AppRoutes.CyclesWavesInput);
    }

    private void NavigateRadixMain()
    {
        var route = _chartSession.SelectedChart is not null
            ? AppRoutes.RadixChart
            : AppRoutes.MainRadixHome;
        _navigationService.NavigateMain(route);
    }

    private void SetActiveSection(string section)
    {
        if (ActiveSection == section) return;
        ActiveSection = section;
    }

    private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == NavigationService.MainNavigatedProperty)
        {
            UpdateCurrentMainViewModel();
            return;
        }

        if (e.PropertyName == nameof(INavigationService.CanGoBackMain) ||
            e.PropertyName == nameof(INavigationService.CanGoBackDetail))
        {
            OnPropertyChanged(nameof(CanGoBackMain));
            OnPropertyChanged(nameof(CanGoBackDetail));
            return;
        }

        if (e.PropertyName != NavigationService.DetailNavigatedProperty) return;

        UpdateCurrentDetailViewModel();
    }

    private void UpdateCurrentMainViewModel()
    {
        CurrentMainViewModel = _routeViewModelFactory.CreateMain(
            _navigationService.CurrentMainRoute,
            _navigationService.CurrentMainParameter);

        OnPropertyChanged(nameof(ShowView1Placeholder));
    }

    private void UpdateCurrentDetailViewModel()
    {
        CurrentDetailViewModel = _routeViewModelFactory.CreateDetail(
            _navigationService.CurrentDetailRoute,
            _navigationService.CurrentDetailParameter);

        OnPropertyChanged(nameof(ShowView2Placeholder));
    }
}
