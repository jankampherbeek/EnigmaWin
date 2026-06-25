// ConfigCalcSectionViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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
using EnigmaWin.Sources.Features.Shared.I18n;
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
    public IReadOnlyList<string> LunarNodeNames        { get; }
    public IReadOnlyList<string> LotsTypeNames         { get; }
    public IReadOnlyList<int> LongitudeDegreeValues    { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues     { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues       { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<DisplayItem<LongitudeHemisphere>> LongitudeDirectionValues { get; private set; } = [];
    public IReadOnlyList<DisplayItem<LatitudeHemisphere>>  LatitudeDirectionValues  { get; private set; } = [];

    // ── Original values for dirty-tracking ──────────────────────────────────

    private int _origHouseSystem, _origAyanamsha, _origObserver, _origProjection,
                _origLunarNode, _origLots;
    private int _origStationary, _origSlow;
    private int _origLatDeg, _origLatMin, _origLatSec;
    private int _origLonDeg, _origLonMin, _origLonSec;
    private LatitudeHemisphere  _origLatDir;
    private LongitudeHemisphere _origLonDir;

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
    private int _selectedLunarNodeIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _selectedLotsTypeIndex;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private double _stationaryPercentage = 10;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private double _slowPercentage = 20;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _homeLatDeg;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _homeLatMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _homeLatSec;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private DisplayItem<LatitudeHemisphere> _homeLatDir = null!;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _homeLonDeg;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _homeLonMin;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private int _homeLonSec;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(IsDirty))]
    private DisplayItem<LongitudeHemisphere> _homeLonDir = null!;

    // ── Computed ────────────────────────────────────────────────────────────

    public bool IsDirty =>
        SelectedHouseSystemIndex          != _origHouseSystem   ||
        SelectedAyanamshaIndex            != _origAyanamsha     ||
        SelectedObserverPositionIndex     != _origObserver      ||
        SelectedProjectionTypeIndex       != _origProjection    ||
        SelectedLunarNodeIndex            != _origLunarNode     ||
        SelectedLotsTypeIndex             != _origLots          ||
        (int)Math.Round(StationaryPercentage) != _origStationary ||
        (int)Math.Round(SlowPercentage)       != _origSlow      ||
        HomeLatDeg != _origLatDeg || HomeLatMin != _origLatMin || HomeLatSec != _origLatSec ||
        HomeLatDir?.Value != _origLatDir                         ||
        HomeLonDeg != _origLonDeg || HomeLonMin != _origLonMin || HomeLonSec != _origLonSec ||
        HomeLonDir?.Value != _origLonDir;

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
    public string LabelHelpLine9          => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.help.line9");
    public string LabelSectionHomeLocation => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.section.homelocation");
    public string LabelHomeLatitude        => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelatitude");
    public string LabelHomeLongitude       => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelongitude");
    public string LabelHomeLocationFooter  => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelocation.footer");
    public string LabelDegrees             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelocation.degrees");
    public string LabelMinutes             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelocation.minutes");
    public string LabelSeconds             => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelocation.seconds");
    public string LabelDirection           => _rosetta.GetText(RbFile.ConfigEdit, "view.configedit.calc.homelocation.direction");

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
        LunarNodeNames = Enum.GetValues<LunarNodeTypes>()
            .Select(l => rosetta.GetText(RbFile.Localizable, l.LocalizedName()))
            .ToList();
        LotsTypeNames = Enum.GetValues<LotsTypes>()
            .Select(l => rosetta.GetText(RbFile.Localizable, l.LocalizedName()))
            .ToList();

        LatitudeDirectionValues =
        [
            new DisplayItem<LatitudeHemisphere>(LatitudeHemisphere.North, rosetta.GetText(RbFile.Localizable, EnumKeySelector.Key(LatitudeHemisphere.North))),
            new DisplayItem<LatitudeHemisphere>(LatitudeHemisphere.South, rosetta.GetText(RbFile.Localizable, EnumKeySelector.Key(LatitudeHemisphere.South)))
        ];
        LongitudeDirectionValues =
        [
            new DisplayItem<LongitudeHemisphere>(LongitudeHemisphere.East, rosetta.GetText(RbFile.Localizable, EnumKeySelector.Key(LongitudeHemisphere.East))),
            new DisplayItem<LongitudeHemisphere>(LongitudeHemisphere.West, rosetta.GetText(RbFile.Localizable, EnumKeySelector.Key(LongitudeHemisphere.West)))
        ];

        var config = configContext.EditingConfig;
        if (config is not null)
        {
            var calc      = config.CalculationConfig;
            var houseVals = Enum.GetValues<HouseSystems>();
            var ayanVals  = Enum.GetValues<Ayanamshas>();
            var obsVals   = Enum.GetValues<ObserverPositions>();
            var projVals  = Enum.GetValues<ProjectionTypes>();
            var lnVals    = Enum.GetValues<LunarNodeTypes>();
            var lotsVals  = Enum.GetValues<LotsTypes>();

            _selectedHouseSystemIndex        = Math.Max(0, Array.IndexOf(houseVals, calc.HouseSystem));
            _selectedAyanamshaIndex          = Math.Max(0, Array.IndexOf(ayanVals,  calc.Ayanamsha));
            _selectedObserverPositionIndex   = Math.Max(0, Array.IndexOf(obsVals,   calc.ObserverPosition));
            _selectedProjectionTypeIndex     = Math.Max(0, Array.IndexOf(projVals,  calc.ProjectionType));
            _selectedLunarNodeIndex          = Math.Max(0, Array.IndexOf(lnVals,    calc.LunarNodeType));
            _selectedLotsTypeIndex           = Math.Max(0, Array.IndexOf(lotsVals,  calc.LotsType));
            _stationaryPercentage            = calc.StationaryPercentage;
            _slowPercentage                  = calc.SlowPercentage;

            var (latDeg, latMin, latSec, latDir) = DecimalToDms(calc.HomeLatitude, isLatitude: true);
            _homeLatDeg = latDeg; _homeLatMin = latMin; _homeLatSec = latSec;
            _homeLatDir = LatitudeDirectionValues.First(d => d.Value == (LatitudeHemisphere)latDir);

            var (lonDeg, lonMin, lonSec, lonDir) = DecimalToDms(calc.HomeLongitude, isLatitude: false);
            _homeLonDeg = lonDeg; _homeLonMin = lonMin; _homeLonSec = lonSec;
            _homeLonDir = LongitudeDirectionValues.First(d => d.Value == (LongitudeHemisphere)lonDir);
        }
        else
        {
            _homeLatDir = LatitudeDirectionValues[0];
            _homeLonDir = LongitudeDirectionValues[0];
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
        _origLunarNode    = SelectedLunarNodeIndex;
        _origLots         = SelectedLotsTypeIndex;
        _origStationary = (int)Math.Round(StationaryPercentage);
        _origSlow       = (int)Math.Round(SlowPercentage);
        _origLatDeg = HomeLatDeg; _origLatMin = HomeLatMin; _origLatSec = HomeLatSec;
        _origLatDir = HomeLatDir?.Value ?? LatitudeHemisphere.North;
        _origLonDeg = HomeLonDeg; _origLonMin = HomeLonMin; _origLonSec = HomeLonSec;
        _origLonDir = HomeLonDir?.Value ?? LongitudeHemisphere.East;
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
            Enum.GetValues<LunarNodeTypes>()[SelectedLunarNodeIndex],
            Enum.GetValues<LotsTypes>()[SelectedLotsTypeIndex],
            (int)Math.Round(StationaryPercentage),
            (int)Math.Round(SlowPercentage),
            DmsToDecimal(HomeLatDeg, HomeLatMin, HomeLatSec, HomeLatDir?.Value == LatitudeHemisphere.South),
            DmsToDecimal(HomeLonDeg, HomeLonMin, HomeLonSec, HomeLonDir?.Value == LongitudeHemisphere.West));

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
        SelectedLunarNodeIndex            = _origLunarNode;
        SelectedLotsTypeIndex             = _origLots;
        StationaryPercentage = _origStationary;
        SlowPercentage       = _origSlow;
        HomeLatDeg = _origLatDeg; HomeLatMin = _origLatMin; HomeLatSec = _origLatSec;
        HomeLatDir = LatitudeDirectionValues.First(d => d.Value == _origLatDir);
        HomeLonDeg = _origLonDeg; HomeLonMin = _origLonMin; HomeLonSec = _origLonSec;
        HomeLonDir = LongitudeDirectionValues.First(d => d.Value == _origLonDir);
    }

    internal void GoBack()
    {
        if (_nav.CanGoBackDetail)
            _nav.GoBackDetail();
        else
            _nav.NavigateDetail(AppRoutes.ConfigEdit);
    }

    // ── DMS ↔ decimal conversion ─────────────────────────────────────────────

    private static double DmsToDecimal(int deg, int min, int sec, bool negative)
    {
        var value = deg + min / 60.0 + sec / 3600.0;
        return negative ? -value : value;
    }

    // Returns (degrees, minutes, seconds, direction) where direction is
    // 0 = North/East (positive), 1 = South/West (negative).
    private static (int deg, int min, int sec, int dir) DecimalToDms(double value, bool isLatitude)
    {
        var negative = value < 0;
        var abs = Math.Abs(value);
        var deg = (int)abs;
        var minFrac = (abs - deg) * 60.0;
        var min = (int)minFrac;
        var sec = (int)Math.Round((minFrac - min) * 60.0);
        if (sec == 60) { sec = 0; min++; }
        if (min == 60) { min = 0; deg++; }
        var maxDeg = isLatitude ? 89 : 180;
        deg = Math.Min(deg, maxDeg);
        return (deg, min, sec, negative ? 1 : 0);
    }
}
