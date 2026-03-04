using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using System;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigEditorViewModel : ObservableObject
{
    private readonly IConfigContext _configContext;
    private readonly INavigationService _navigationService;

    public ConfigEditorMode Mode { get; }

    public HouseSystems[] HouseSystemValues { get; } = Enum.GetValues<HouseSystems>();
    public Ayanamshas[] AyanamshaValues { get; } = Enum.GetValues<Ayanamshas>();
    public ObserverPositions[] ObserverPositionValues { get; } = Enum.GetValues<ObserverPositions>();
    public ProjectionTypes[] ProjectionTypeValues { get; } = Enum.GetValues<ProjectionTypes>();
    public BlackMoonCorrectionTypes[] BlackMoonCorrectionTypeValues { get; } = Enum.GetValues<BlackMoonCorrectionTypes>();
    public LunarNodeTypes[] LunarNodeTypeValues { get; } = Enum.GetValues<LunarNodeTypes>();
    public LotsTypes[] LotsTypeValues { get; } = Enum.GetValues<LotsTypes>();

    [ObservableProperty]
    private HouseSystems _houseSystem;

    [ObservableProperty]
    private Ayanamshas _ayanamsha;

    [ObservableProperty]
    private ObserverPositions _observerPosition;

    [ObservableProperty]
    private ProjectionTypes _projectionType;

    [ObservableProperty]
    private BlackMoonCorrectionTypes _blackMoonCorrectionType;

    [ObservableProperty]
    private LunarNodeTypes _lunarNodeType;

    [ObservableProperty]
    private LotsTypes _lotsType;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public string Title => Mode == ConfigEditorMode.New ? "New Configuration" : "Edit Configuration";

    public ConfigEditorViewModel(
        IConfigContext configContext,
        INavigationService navigationService,
        ConfigEditorMode mode)
    {
        _configContext = configContext;
        _navigationService = navigationService;
        Mode = mode;

        var source = mode == ConfigEditorMode.Edit
            ? _configContext.ActiveConfig
            : ConfigData.Default;

        HouseSystem = source.HouseSystem;
        Ayanamsha = source.Ayanamsha;
        ObserverPosition = source.ObserverPosition;
        ProjectionType = source.ProjectionType;
        BlackMoonCorrectionType = source.BlackMoonCorrectionType;
        LunarNodeType = source.LunarNodeType;
        LotsType = source.LotsType;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        _configContext.ActiveConfig = new ConfigData(
            HouseSystem: HouseSystem,
            Ayanamsha: Ayanamsha,
            ObserverPosition: ObserverPosition,
            ProjectionType: ProjectionType,
            BlackMoonCorrectionType: BlackMoonCorrectionType,
            LunarNodeType: LunarNodeType,
            LotsType: LotsType);

        _navigationService.NavigateDetail(
            AppRoutes.ConfigHome,
            new ConfigHomeNavigationParameter(ConfigHomeMode.Overview));
    }

    private void Cancel()
    {
        if (_navigationService.CanGoBackDetail)
        {
            _navigationService.GoBackDetail();
            return;
        }

        _navigationService.NavigateDetail(
            AppRoutes.ConfigHome,
            new ConfigHomeNavigationParameter(ConfigHomeMode.Overview));
    }
}
