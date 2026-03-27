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
            ? (_configContext.EditingConfig ?? _configContext.ActiveConfig)
            : UserConfiguration.Default;

        var calc = source.CalculationConfig;
        HouseSystem           = calc.HouseSystem;
        Ayanamsha             = calc.Ayanamsha;
        ObserverPosition      = calc.ObserverPosition;
        ProjectionType        = calc.ProjectionType;
        BlackMoonCorrectionType = calc.BlackMoonCorrectionType;
        LunarNodeType         = calc.LunarNodeType;
        LotsType              = calc.LotsType;
        Language = LanguageValues.FirstOrDefault(o => o.Code == source.Language) ?? LanguageValues[0];

        SaveCommand   = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        var languageCode = string.IsNullOrEmpty(Language.Code)
            ? DetectSystemLanguage()
            : Language.Code;
        _rosetta.SetLanguage(languageCode);

        var updatedConfig = _configContext.EditingConfig ?? _configContext.ActiveConfig;
        updatedConfig.CalculationConfig = new CalculationConfig(
            HouseSystem:             HouseSystem,
            Ayanamsha:               Ayanamsha,
            ObserverPosition:        ObserverPosition,
            ProjectionType:          ProjectionType,
            BlackMoonCorrectionType: BlackMoonCorrectionType,
            LunarNodeType:           LunarNodeType,
            LotsType:                LotsType);
        updatedConfig.Language = Language.Code;

        _configContext.ActiveConfig = updatedConfig;
        _configContext.EditingConfig = updatedConfig;

        _navigationService.NavigateDetail(
            AppRoutes.ConfigEdit,
            new ConfigEditNavigationParameter(updatedConfig.Id));
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

        var config = _configContext.EditingConfig ?? _configContext.ActiveConfig;
        _navigationService.NavigateDetail(
            AppRoutes.ConfigEdit,
            new ConfigEditNavigationParameter(config.Id));
    }
}
