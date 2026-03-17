using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EnigmaWin.Sources.Features.Config.UI;

public record LanguageOption(string Code, string DisplayName);

public sealed partial class ConfigEditorViewModel : ObservableObject
{

    private readonly IConfigContext _configContext;
    private readonly INavigationService _navigationService;
    private readonly IRosetta _rosetta;

    public ConfigEditorMode Mode { get; }

    public HouseSystems[] HouseSystemValues { get; } = Enum.GetValues<HouseSystems>();
    public Ayanamshas[] AyanamshaValues { get; } = Enum.GetValues<Ayanamshas>();
    public ObserverPositions[] ObserverPositionValues { get; } = Enum.GetValues<ObserverPositions>();
    public ProjectionTypes[] ProjectionTypeValues { get; } = Enum.GetValues<ProjectionTypes>();
    public BlackMoonCorrectionTypes[] BlackMoonCorrectionTypeValues { get; } = Enum.GetValues<BlackMoonCorrectionTypes>();
    public LunarNodeTypes[] LunarNodeTypeValues { get; } = Enum.GetValues<LunarNodeTypes>();
    public LotsTypes[] LotsTypeValues { get; } = Enum.GetValues<LotsTypes>();

    public LanguageOption[] LanguageValues { get; } =
    [
        new("",   "System default"),
        new("en", "English"),
        new("nl", "Nederlands"),
        new("de", "Deutsch"),
        new("fr", "Français"),
    ];

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

    [ObservableProperty]
    private LanguageOption _language;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public string Title => Mode == ConfigEditorMode.New ? "New Configuration" : "Edit Configuration";

    public ConfigEditorViewModel(
        IConfigContext configContext,
        INavigationService navigationService,
        IRosetta rosetta,
        ConfigEditorMode mode)
    {
        _configContext = configContext;
        _navigationService = navigationService;
        _rosetta = rosetta;
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
        Language = LanguageValues.FirstOrDefault(o => o.Code == source.Language) ?? LanguageValues[0];

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        var languageCode = string.IsNullOrEmpty(Language.Code)
            ? DetectSystemLanguage()
            : Language.Code;
        _rosetta.SetLanguage(languageCode);

        _configContext.ActiveConfig = new ConfigData(
            HouseSystem: HouseSystem,
            Ayanamsha: Ayanamsha,
            ObserverPosition: ObserverPosition,
            ProjectionType: ProjectionType,
            BlackMoonCorrectionType: BlackMoonCorrectionType,
            LunarNodeType: LunarNodeType,
            LotsType: LotsType,
            Language: Language.Code);

        _navigationService.NavigateDetail(
            AppRoutes.ConfigHome,
            new ConfigHomeNavigationParameter(ConfigHomeMode.Overview));
    }

    private static readonly HashSet<string> SupportedLanguages = ["en", "nl", "de", "fr"];

    private static string DetectSystemLanguage()
    {
        var twoLetter = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return SupportedLanguages.Contains(twoLetter) ? twoLetter : "en";
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
