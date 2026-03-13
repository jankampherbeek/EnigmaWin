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
    private readonly IChartContext _chartContext;
    private readonly IRouteViewModelFactory _routeViewModelFactory;

    public string Greeting { get; } = "Welcome to Avalonia!";
    public string Welcome { get; }
    public IRelayCommand SelectRadixCommand { get; }
    public IRelayCommand SelectConfigurationCommand { get; }
    public IRelayCommand SelectResearchCommand { get; }
    public IRelayCommand NewChartCommand { get; }
    public IRelayCommand NewConfigurationCommand { get; }
    public IRelayCommand EditConfigurationCommand { get; }
    public IRelayCommand BackMainCommand { get; }
    public IRelayCommand BackDetailCommand { get; }

    [ObservableProperty]
    private object? currentMainViewModel;

    [ObservableProperty]
    private object? currentDetailViewModel;

    public MainWindowViewModel(
        INavigationService navigationService,
        IChartContext chartContext,
        IRouteViewModelFactory routeViewModelFactory,
        IRosetta rosetta)
    {
        _navigationService = navigationService;
        _chartContext = chartContext;
        _routeViewModelFactory = routeViewModelFactory;
        Welcome = rosetta.GetText(RbFile.Localizable, "welcome");

        SelectRadixCommand = new RelayCommand(SelectRadix);
        SelectConfigurationCommand = new RelayCommand(SelectConfiguration);
        SelectResearchCommand = new RelayCommand(SelectResearch);
        NewChartCommand = new RelayCommand(OpenNewChart);
        NewConfigurationCommand = new RelayCommand(OpenNewConfiguration);
        EditConfigurationCommand = new RelayCommand(OpenEditConfiguration);
        BackMainCommand = new RelayCommand(_navigationService.GoBackMain);
        BackDetailCommand = new RelayCommand(_navigationService.GoBackDetail);

        _navigationService.PropertyChanged += OnNavigationServicePropertyChanged;
        _navigationService.NavigateMain(AppRoutes.MainRadixHome);
        _navigationService.NavigateDetail(AppRoutes.None);
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

    public void ShowRadixPositionsFromCalculation(FullChart chart)
    {
        _chartContext.CurrentChart = chart;
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
                _navigationService.NavigateMain(AppRoutes.MainRadixHome);
                _navigationService.NavigateDetail(AppRoutes.None);
                break;
            case "Configuration":
                _navigationService.NavigateMain(AppRoutes.MainConfigHome);
                _navigationService.NavigateDetail(AppRoutes.ConfigHome, new ConfigHomeNavigationParameter(ConfigHomeMode.Overview));
                break;
            default:
                _navigationService.NavigateMain(AppRoutes.MainResearchHome);
                _navigationService.NavigateDetail(AppRoutes.None);
                break;
        }
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
