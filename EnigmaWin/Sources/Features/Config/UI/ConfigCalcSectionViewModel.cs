// ConfigCalcSectionViewModel.cs
// EnigmaWin
// Created by Jan Kampherbeek on 22-03-2026

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.UserConfiguration;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class ConfigCalcSectionViewModel : ObservableObject
{
    private readonly IUserConfigurationRepository _repo;
    private readonly INavigationService _nav;
    private readonly IConfigContext _configContext;
    private readonly IRosetta _rosetta;

    // ── Picker item lists ───────────────────────────────────────────────────

    public IReadOnlyList<string> HouseSystemNames      { get; }
    public IReadOnlyList<string> AyanamshaNames        { get; }
    public IReadOnlyList<string> ObserverPositionNames { get; }
    public IReadOnlyList<string> ProjectionTypeNames   { get; }
    public IReadOnlyList<string> BlackMoonNames        { get; }
    public IReadOnlyList<string> LunarNodeNames        { get; }
    public IReadOnlyList<string> LotsTypeNames         { get; }

    // ── Original values for dirty-tracking ──────────────────────────────────

    private int _origHouseSystem, _origAyanamsha, _origObserver, _origProjection,
                _origBlackMoon, _origLunarNode, _origLots;
    private int _origStationary, _origSlow;

    // ── Observable selected indices ─────────────────────────────────────────

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedHouseSystemIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedAyanamshaIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedObserverPositionIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedProjectionTypeIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedBlackMoonCorrectionIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedLunarNodeIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedLotsTypeIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private double _stationaryPercentage = 10;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private double _slowPercentage = 20;

    // ── Computed ────────────────────────────────────────────────────────────

    public bool IsDirty =>
        SelectedHouseSystemIndex          != _origHouseSystem   ||
        SelectedAyanamshaIndex            != _origAyanamsha     ||
        SelectedObserverPositionIndex     != _origObserver      ||
        SelectedProjectionTypeIndex       != _origProjection    ||
        SelectedBlackMoonCorrectionIndex  != _origBlackMoon     ||
        SelectedLunarNodeIndex            != _origLunarNode     ||
        SelectedLotsTypeIndex             != _origLots          ||
        (int)Math.Round(StationaryPercentage) != _origStationary ||
        (int)Math.Round(SlowPercentage)       != _origSlow;

    // ── Localized labels ────────────────────────────────────────────────────

    public string SectionTitle            => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.section.calculation");
    public string LabelBackToOverview     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.backtoverview");
    public string LabelSave               => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.edit.save");
    public string LabelCancel             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.cancel");
    public string LabelSectionSystems     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.section.systems");
    public string LabelSectionCalc        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.section.calculation");
    public string LabelSectionLunar       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.section.lunar");
    public string LabelSectionSpeeds      => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.section.speeds");
    public string LabelHouseSystem        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.housesystem");
    public string LabelAyanamsha          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.ayanamsha");
    public string LabelObserverPosition   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.observerposition");
    public string LabelProjectionType     => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.projectiontype");
    public string LabelBlackMoon          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.blackmooncorrection");
    public string LabelLunarNode          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.lunarnode");
    public string LabelLotsType           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.lotstype");
    public string LabelStationary         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.stationarypercentage");
    public string LabelSlow               => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.slowpercentage");
    public string LabelStationaryFooter   => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.stationaryfooter");
    public string LabelSlowFooter         => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.slowfooter");
    public string LabelHelpTooltip        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.tooltip");
    public string LabelHelpTitle          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.groupbox");
    public string LabelHelpClose          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.help.close");
    public string LabelHelpLine1          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line1");
    public string LabelHelpLine2          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line2");
    public string LabelHelpLine3          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line3");
    public string LabelHelpLine4          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line4");
    public string LabelHelpLine5          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line5");
    public string LabelHelpLine6          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line6");
    public string LabelHelpLine7          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line7");
    public string LabelHelpLine8          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line8");

    // ── Constructor ─────────────────────────────────────────────────────────

    public ConfigCalcSectionViewModel(
        IUserConfigurationRepository repo,
        INavigationService nav,
        IConfigContext configContext,
        IRosetta rosetta)
    {
        _repo = repo;
        _nav = nav;
        _configContext = configContext;
        _rosetta = rosetta;

        HouseSystemNames = Enum.GetValues<HouseSystems>()
            .Select(h => rosetta.GetText(RbFile.Localizable, h.LocalizedName()))
            .ToList();
        AyanamshaNames = Enum.GetValues<Ayanamshas>()
            .Select(a => rosetta.GetText(RbFile.Localizable, a.LocalizedName()))
            .ToList();
        ObserverPositionNames = Enum.GetValues<ObserverPositions>()
            .Select(o => rosetta.GetText(RbFile.Localizable, o.LocalizedName()))
            .ToList();
        ProjectionTypeNames = Enum.GetValues<ProjectionTypes>()
            .Select(p => rosetta.GetText(RbFile.Localizable, p.LocalizedName()))
            .ToList();
        BlackMoonNames = Enum.GetValues<BlackMoonCorrectionTypes>()
            .Select(b => rosetta.GetText(RbFile.Localizable, b.LocalizedName()))
            .ToList();
        LunarNodeNames = Enum.GetValues<LunarNodeTypes>()
            .Select(l => rosetta.GetText(RbFile.Localizable, l.LocalizedName()))
            .ToList();
        LotsTypeNames = Enum.GetValues<LotsTypes>()
            .Select(l => rosetta.GetText(RbFile.Localizable, l.LocalizedName()))
            .ToList();

        var config = configContext.EditingConfig;
        if (config is not null)
        {
            var calc      = config.CalculationConfig;
            var houseVals = Enum.GetValues<HouseSystems>();
            var ayanVals  = Enum.GetValues<Ayanamshas>();
            var obsVals   = Enum.GetValues<ObserverPositions>();
            var projVals  = Enum.GetValues<ProjectionTypes>();
            var bmVals    = Enum.GetValues<BlackMoonCorrectionTypes>();
            var lnVals    = Enum.GetValues<LunarNodeTypes>();
            var lotsVals  = Enum.GetValues<LotsTypes>();

            _selectedHouseSystemIndex        = Math.Max(0, Array.IndexOf(houseVals, calc.HouseSystem));
            _selectedAyanamshaIndex          = Math.Max(0, Array.IndexOf(ayanVals,  calc.Ayanamsha));
            _selectedObserverPositionIndex   = Math.Max(0, Array.IndexOf(obsVals,   calc.ObserverPosition));
            _selectedProjectionTypeIndex     = Math.Max(0, Array.IndexOf(projVals,  calc.ProjectionType));
            _selectedBlackMoonCorrectionIndex = Math.Max(0, Array.IndexOf(bmVals,   calc.BlackMoonCorrectionType));
            _selectedLunarNodeIndex          = Math.Max(0, Array.IndexOf(lnVals,    calc.LunarNodeType));
            _selectedLotsTypeIndex           = Math.Max(0, Array.IndexOf(lotsVals,  calc.LotsType));
            _stationaryPercentage            = calc.StationaryPercentage;
            _slowPercentage                  = calc.SlowPercentage;
        }

        SaveOriginals();
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private void SaveOriginals()
    {
        _origHouseSystem  = SelectedHouseSystemIndex;
        _origAyanamsha    = SelectedAyanamshaIndex;
        _origObserver     = SelectedObserverPositionIndex;
        _origProjection   = SelectedProjectionTypeIndex;
        _origBlackMoon    = SelectedBlackMoonCorrectionIndex;
        _origLunarNode    = SelectedLunarNodeIndex;
        _origLots         = SelectedLotsTypeIndex;
        _origStationary   = (int)Math.Round(StationaryPercentage);
        _origSlow         = (int)Math.Round(SlowPercentage);
    }

    // ── Commands ────────────────────────────────────────────────────────────

    internal async Task SaveAsync()
    {
        var config = _configContext.EditingConfig;
        if (config is null) return;

        config.CalculationConfig = new CalculationConfig(
            Enum.GetValues<HouseSystems>()[SelectedHouseSystemIndex],
            Enum.GetValues<Ayanamshas>()[SelectedAyanamshaIndex],
            Enum.GetValues<ObserverPositions>()[SelectedObserverPositionIndex],
            Enum.GetValues<ProjectionTypes>()[SelectedProjectionTypeIndex],
            Enum.GetValues<BlackMoonCorrectionTypes>()[SelectedBlackMoonCorrectionIndex],
            Enum.GetValues<LunarNodeTypes>()[SelectedLunarNodeIndex],
            Enum.GetValues<LotsTypes>()[SelectedLotsTypeIndex],
            (int)Math.Round(StationaryPercentage),
            (int)Math.Round(SlowPercentage));

        await _repo.UpdateAsync(config);
        if (config.Id == _configContext.ActiveConfig.Id)
            _configContext.ActiveConfig = config;
        SaveOriginals();
        OnPropertyChanged(nameof(IsDirty));
    }

    internal void Revert()
    {
        SelectedHouseSystemIndex          = _origHouseSystem;
        SelectedAyanamshaIndex            = _origAyanamsha;
        SelectedObserverPositionIndex     = _origObserver;
        SelectedProjectionTypeIndex       = _origProjection;
        SelectedBlackMoonCorrectionIndex  = _origBlackMoon;
        SelectedLunarNodeIndex            = _origLunarNode;
        SelectedLotsTypeIndex             = _origLots;
        StationaryPercentage              = _origStationary;
        SlowPercentage                    = _origSlow;
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }
}
