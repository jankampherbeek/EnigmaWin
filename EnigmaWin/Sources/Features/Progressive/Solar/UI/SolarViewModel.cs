// SolarViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.Solar.UI;

public partial class SolarViewModel : ObservableObject
{
    private readonly IChartSession  _chartSession;
    private readonly IConfigContext _configContext;
    private readonly IRosetta       _rosetta;

    // Input parameters
    [ObservableProperty]
    private string _ageText = string.Empty;

    [ObservableProperty] private bool _useSidereal = false;
    [ObservableProperty] private string _geoLonText  = string.Empty;
    [ObservableProperty] private string _geoLatText  = string.Empty;
    [ObservableProperty][NotifyPropertyChangedFor(nameof(LonDirectionWest))]
    private bool _lonDirectionEast = true;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(LatDirectionSouth))]
    private bool _latDirectionNorth = true;

    public bool LonDirectionWest
    {
        get => !LonDirectionEast;
        set => LonDirectionEast = !value;
    }

    public bool LatDirectionSouth
    {
        get => !LatDirectionNorth;
        set => LatDirectionNorth = !value;
    }

    // Results
    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<SolarPositionRow> _positionRows = [];

    [ObservableProperty]
    private ObservableCollection<SolarAspectRow> _aspectRows = [];

    [ObservableProperty] private int    _activeTab    = 0;  // 0=SolarChart,1=Combined,2=Positions,3=Aspects,4=Details
    [ObservableProperty] private bool   _isBlackWhite = false;
    [ObservableProperty] private bool   _hideAspects  = false;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasError;

    [ObservableProperty] private WheelPlotData   _solarPlotData   = WheelPlotData.Empty;
    [ObservableProperty] private WheelPlotData   _radixPlotData   = WheelPlotData.Empty;
    [ObservableProperty] private WheelPlotItem[] _solarPlotItems  = [];

    // Details
    [ObservableProperty] private string _radixDateText   = string.Empty;
    [ObservableProperty] private string _solarDateText   = string.Empty;
    [ObservableProperty] private string _solarJdText     = string.Empty;
    [ObservableProperty] private string _solarLocationText = string.Empty;
    [ObservableProperty] private string _radixSunText    = string.Empty;
    [ObservableProperty] private string _solarSunText    = string.Empty;
    [ObservableProperty] private bool   _showSidereal    = false;

    private Dictionary<Factors, ProgressivePosition> _results = [];
    private FullChart?  _solarFullChart;
    private FullChart?  _radixChart;
    private double      _usedGeoLon;
    private double      _usedGeoLat;

    public bool HasResults => PositionRows.Count > 0;
    public bool HasCharts  => _chartSession.Charts.Count > 0;
    private bool CanCalculate() => HasCharts && !string.IsNullOrWhiteSpace(AgeText);

    public WheelTheme Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool       ShowAspects => !HideAspects;

    public IReadOnlyList<NamedChart> Charts => _chartSession.Charts;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasCharts))]
    private NamedChart? _selectedChart;

    // Labels — input screen
    public string LabelTitle            => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.title");
    public string LabelNoSession        => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.noSession");
    public string LabelChartSection     => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.chart.label");
    public string LabelAgeLabel         => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.age.label");
    public string LabelAgePlaceholder   => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.age.placeholder");
    public string LabelSidereal         => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.sidereal.label");
    public string LabelRelocationHeader => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.relocation.header");
    public string LabelLongitude        => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.longitude.label");
    public string LabelLatitude         => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.latitude.label");
    public string LabelEast             => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.east");
    public string LabelWest             => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.west");
    public string LabelNorth            => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.north");
    public string LabelSouth            => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.south");
    public string LabelCalculate        => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.calculate");
    public string LabelHelp             => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.help");
    public string LabelFactsheetTooltip => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.factsheet.tooltip");
    public string LabelFactsheetTitle   => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.factsheet.title");
    public string LabelHelpTitle        => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.help.title");
    public string LabelHelpClose        => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.help.close");

    // Labels — results screen
    public string LabelResultsTitle    => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.results.title");
    public string LabelNoResults       => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.results.noresults");
    public string LabelTabSolarChart   => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.tab.solarchart");
    public string LabelTabCombined     => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.tab.combinedchart");
    public string LabelTabPositions    => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.tab.positions");
    public string LabelTabAspects      => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.tab.aspects");
    public string LabelTabDetails      => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.tab.details");
    public string LabelColLongitude    => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.col.longitude");
    public string LabelColDeclination  => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.col.declination");
    public string LabelAspectColSolar  => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.aspects.col.solar");
    public string LabelAspectColAspect => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.aspects.col.aspect");
    public string LabelAspectColRadix  => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.aspects.col.radix");
    public string LabelAspectColOrb    => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.aspects.col.orb");
    public string LabelNoAspects       => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.aspects.noaspects");
    public string LabelDetailsRadixHeader  => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.radix.header");
    public string LabelDetailsRadixDate    => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.radix.date");
    public string LabelDetailsRadixSun     => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.radix.sun");
    public string LabelDetailsSolarHeader  => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.solar.header");
    public string LabelDetailsSolarDate    => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.solar.date");
    public string LabelDetailsSolarJd      => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.solar.jd");
    public string LabelDetailsSolarSun     => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.solar.sun");
    public string LabelDetailsSolarLocation => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.solar.location");
    public string LabelDetailsSidereal     => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.details.sidereal");
    public string LabelResultsHelp         => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.results.help");

    // Labels — wheel toolbar
    public string LabelWheelBw      => _rosetta.GetText(RbFile.SolarReturn, IsBlackWhite ? "view.solarreturn.wheel.color" : "view.solarreturn.wheel.bw");
    public string LabelWheelAspects => _rosetta.GetText(RbFile.SolarReturn, HideAspects  ? "view.solarreturn.wheel.aspects.show" : "view.solarreturn.wheel.aspects.hide");
    public string LabelWheelExport  => _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.wheel.export");

    public SolarViewModel(IChartSession chartSession, IConfigContext configContext, IRosetta rosetta)
    {
        _chartSession  = chartSession;
        _configContext = configContext;
        _rosetta       = rosetta;

        if (_chartSession.Selected is not null)
            SelectedChart = _chartSession.Selected;

        _chartSession.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(IChartSession.Charts) or nameof(IChartSession.Selected))
            {
                OnPropertyChanged(nameof(Charts));
                OnPropertyChanged(nameof(HasCharts));
                if (_chartSession.Selected is not null && SelectedChart is null)
                    SelectedChart = _chartSession.Selected;
            }
        };
    }

    partial void OnSelectedChartChanged(NamedChart? value)
    {
        ClearResults();
        CalculateCommand.NotifyCanExecuteChanged();
    }

    partial void OnAgeTextChanged(string value)
    {
        CalculateCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCalculate))]
    private void Calculate()
    {
        if (SelectedChart is null) return;
        try
        {
            ErrorMessage = string.Empty;
            HasError     = false;

            if (!int.TryParse(AgeText.Trim(), out var age) || age <= 0)
            {
                ErrorMessage = "Please enter a valid age (integer > 0).";
                HasError     = true;
                return;
            }

            // Parse relocation — fall back to birth location when fields are empty
            var lonEmpty = string.IsNullOrWhiteSpace(GeoLonText);
            var latEmpty = string.IsNullOrWhiteSpace(GeoLatText);
            double geoLon, geoLat;

            if (lonEmpty && latEmpty)
            {
                geoLon = _chartSession.SelectedChart?.HousePositions?.Ascendant?.Longitude != null
                    ? _configContext.ActiveConfig.CalculationConfig.HomeLongitude
                    : 0.0;
                geoLat = _configContext.ActiveConfig.CalculationConfig.HomeLatitude;

                // Use the birth chart location from Horoscope stored data (best effort)
                geoLon = _configContext.ActiveConfig.CalculationConfig.HomeLongitude;
                geoLat = _configContext.ActiveConfig.CalculationConfig.HomeLatitude;
            }
            else
            {
                if (!TryParseDms(GeoLonText, out var parsedLon) || !TryParseDms(GeoLatText, out var parsedLat))
                {
                    ErrorMessage = "Invalid coordinates. Use format ddd:mm:ss.";
                    HasError     = true;
                    return;
                }
                geoLon = LonDirectionEast ? parsedLon : -parsedLon;
                geoLat = LatDirectionNorth ? parsedLat : -parsedLat;
            }

            _usedGeoLon   = geoLon;
            _usedGeoLat   = geoLat;
            _radixChart   = _chartSession.SelectedChart;

            var userConfig = _configContext.ActiveConfig;
            var result = SolarReturnOrchestrator.Calculate(
                birthJd:    SelectedChart.Chart.JulianDay,
                age:        age,
                useSidereal: UseSidereal,
                geoLon:     geoLon,
                geoLat:     geoLat,
                userConfig: userConfig);

            _results       = result.Positions;
            _solarFullChart = result.SolarFullChart;

            BuildPositionRows();
            BuildAspectRows();
            BuildWheelData(result);
            BuildDetails(result);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            HasError     = true;
        }
    }

    [RelayCommand]
    private void ToggleBlackWhite()
    {
        IsBlackWhite = !IsBlackWhite;
    }

    [RelayCommand]
    private void ToggleAspects()
    {
        HideAspects = !HideAspects;
    }

    partial void OnIsBlackWhiteChanged(bool value)
    {
        OnPropertyChanged(nameof(Theme));
        OnPropertyChanged(nameof(LabelWheelBw));
    }

    partial void OnHideAspectsChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowAspects));
        OnPropertyChanged(nameof(LabelWheelAspects));
    }

    // MARK: - Private helpers

    private void ClearResults()
    {
        _results        = [];
        _solarFullChart = null;
        PositionRows.Clear();
        AspectRows.Clear();
        SolarPlotData   = WheelPlotData.Empty;
        RadixPlotData   = WheelPlotData.Empty;
        SolarPlotItems  = [];
        RadixDateText   = string.Empty;
        SolarDateText   = string.Empty;
        SolarJdText     = string.Empty;
        SolarLocationText = string.Empty;
        RadixSunText    = string.Empty;
        SolarSunText    = string.Empty;
        ShowSidereal    = false;
    }

    private void BuildPositionRows()
    {
        var rows = _results
            .OrderBy(kvp => (int)kvp.Key)
            .Select((kvp, i) => new SolarPositionRow(kvp.Key, kvp.Value, i))
            .ToList();
        PositionRows = new ObservableCollection<SolarPositionRow>(rows);
    }

    private void BuildAspectRows()
    {
        if (_radixChart is null || _results.Count == 0) { AspectRows.Clear(); return; }

        var config = _configContext.ActiveConfig;
        var orb    = config.ProgressionsConfig.SolarReturn.Orb;
        var found  = TransitAspectsOrchestrator.Calculate(
            _results, _radixChart, config.FactorConfig, config.AspectConfig, orb);

        var rows = found
            .OrderBy(a => a.Orb)
            .Select((a, i) =>
            {
                var totalMin = (int)(a.Orb * 60);
                var orbText  = $"{totalMin / 60}°{totalMin % 60:D2}'";
                return new SolarAspectRow(
                    GlyphSelector.GetGlyphForFactor(a.Factor1),
                    GlyphSelector.GetGlyphForAspect(a.Aspect),
                    GlyphSelector.GetGlyphForFactor(a.Factor2),
                    orbText, i);
            })
            .ToList();
        AspectRows = new ObservableCollection<SolarAspectRow>(rows);
    }

    private void BuildWheelData(SolarReturnOrchestrator.Result result)
    {
        if (_solarFullChart is null) return;
        var config = _configContext.ActiveConfig;

        SolarPlotData = WheelPlotDataBuilder.Build(_solarFullChart, config);

        if (_radixChart is not null)
        {
            RadixPlotData = WheelPlotDataBuilder.Build(_radixChart, config);
            var ascLong   = _radixChart.HousePositions.Ascendant.Longitude;
            SolarPlotItems = _results
                .Select(kvp => new WheelPlotItem(
                    Factor:            kvp.Key,
                    Glyph:             GlyphSelector.GetGlyphForFactor(kvp.Key),
                    EclipticLongitude: kvp.Value.Longitude,
                    MundaneAngle:      WheelGeometry.MundaneAngle(kvp.Value.Longitude, ascLong),
                    PlotAngle:         WheelGeometry.MundaneAngle(kvp.Value.Longitude, ascLong),
                    PositionText:      $"{(int)(kvp.Value.Longitude % 30)}°{(int)(kvp.Value.Longitude % 30 * 60) % 60:D2}'",
                    SpeedType:         SpeedType.Direct))
                .ToArray();
        }
    }

    private void BuildDetails(SolarReturnOrchestrator.Result result)
    {
        if (_radixChart is not null)
        {
            var radixDt = Features.AstronCalc.SEWrapper.DateFromJulianDay(_radixChart.JulianDay, true);
            RadixDateText = $"{radixDt.Date.Year:D4}/{radixDt.Date.Month:D2}/{radixDt.Date.Day:D2} " +
                            $"{radixDt.Time.Hour:D2}:{radixDt.Time.Minute:D2}:{radixDt.Time.Second:D2} UT";
            RadixSunText = PositionInDegreesConversion.DoubleToDms(result.RadixSunLongitude);
        }

        SolarDateText = result.DateText;
        SolarJdText   = $"{result.SolarJd:F6}";
        SolarSunText  = PositionInDegreesConversion.DoubleToDms(result.SolarSunLongitude);
        ShowSidereal  = UseSidereal;

        var lonAbs = Math.Abs(_usedGeoLon);
        var latAbs = Math.Abs(_usedGeoLat);
        var lonDir = _usedGeoLon >= 0
            ? _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.east")
            : _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.west");
        var latDir = _usedGeoLat >= 0
            ? _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.north")
            : _rosetta.GetText(RbFile.SolarReturn, "view.solarreturn.direction.south");
        SolarLocationText = $"{PositionInDegreesConversion.DoubleToDms(lonAbs)} {lonDir} / " +
                            $"{PositionInDegreesConversion.DoubleToDms(latAbs)} {latDir}";
    }

    private static bool TryParseDms(string text, out double result)
    {
        result = 0.0;
        var parts = text.Trim().Split(':');
        if (parts.Length < 2) return false;
        if (!int.TryParse(parts[0], out var deg)) return false;
        if (!int.TryParse(parts[1], out var min)) return false;
        var sec = parts.Length >= 3 && int.TryParse(parts[2], out var s) ? s : 0;
        result = deg + min / 60.0 + sec / 3600.0;
        return true;
    }
}
