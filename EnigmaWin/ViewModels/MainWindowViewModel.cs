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

    public string Greeting { get; } = "Welcome to Avalonia!";
    public string Welcome { get; }
    public IRelayCommand SelectRadixCommand { get; }
    public IRelayCommand SelectConfigurationCommand { get; }
    public IRelayCommand SelectResearchCommand { get; }
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

    public bool ShowRadixButtons => ActiveSection == "Radix";
    public bool ShowConfigurationButtons => ActiveSection == "Configuration";
    public bool ShowResearchButtons => ActiveSection == "Research";
    public bool CanGoBackMain => _navigationService.CanGoBackMain;
    public bool CanGoBackDetail => _navigationService.CanGoBackDetail;
    public bool ShowView1Placeholder => CurrentMainViewModel == null;
    public bool ShowView2Placeholder => CurrentDetailViewModel == null;

    private void SelectRadix()
    {
        if (ActiveSection == "Radix")
        {
            // Al actief: navigeer direct naar het juiste hoofdscherm
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
        if (ActiveSection != "Radix")
        {
            return;
        }

        _navigationService.NavigateDetail(AppRoutes.RadixInput, new RadixInputNavigationParameter(Guid.NewGuid()));
    }

    private void OpenNewConfiguration()
    {
        if (ActiveSection != "Configuration")
        {
            return;
        }

        _navigationService.NavigateDetail(AppRoutes.ConfigEditor, new ConfigEditorNavigationParameter(ConfigEditorMode.New));
    }

    private void OpenEditConfiguration()
    {
        if (ActiveSection != "Configuration")
        {
            return;
        }

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
            default:
                _navigationService.NavigateMain(AppRoutes.MainResearchHome);
                _navigationService.NavigateDetail(AppRoutes.None);
                break;
        }
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
        if (ActiveSection == section)
        {
            return;
        }

        ActiveSection = section;
    }

    private void OnNavigationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentMainRoute) ||
            e.PropertyName == nameof(INavigationService.CurrentMainParameter))
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

        if (e.PropertyName != nameof(INavigationService.CurrentDetailRoute) &&
            e.PropertyName != nameof(INavigationService.CurrentDetailParameter))
        {
            return;
        }

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
