// ZodiacDivisionsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.Sources.Features.ChartDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;

public sealed class ZodiacDivisionsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private readonly IChartSession _chartSession;
    private FullChart? _currentChart;

    private bool _useDecansPlanet;
    private bool _useDodecatsOriginal = true;
    private bool _useBoundsEgyptian = true;
    private bool _isBlackWhite;
    private bool _showAspects = true;

    private enum Tab { Positions, Figure }
    private Tab _activeTab = Tab.Positions;

    public record ZodiacDivisionRow(
        string FactorGlyph,
        string PositionDms,
        string SignGlyph,
        string DecanGlyph,
        string DodecatGlyph,
        string BoundGlyph,
        bool IsEvenRow);

    public ObservableCollection<ZodiacDivisionRow> Rows { get; } = [];

    public WheelPlotData      PlotData { get; private set; } = WheelPlotData.Empty;
    public ZodiacDivisionMark[] Marks  { get; private set; } = [];
    public WheelTheme Theme => _isBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;

    private bool _hasData;
    public bool HasData
    {
        get => _hasData;
        private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); }
    }
    public bool HasNoData => !HasData;

    public bool ShowPositions => _activeTab == Tab.Positions;
    public bool ShowFigure    => _activeTab == Tab.Figure;
    public bool IsBlackWhite  => _isBlackWhite;

    public bool UseDecansPlanet
    {
        get => _useDecansPlanet;
        set { if (_useDecansPlanet == value) return; _useDecansPlanet = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseDodecatsOriginal
    {
        get => _useDodecatsOriginal;
        set { if (_useDodecatsOriginal == value) return; _useDodecatsOriginal = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseBoundsEgyptian
    {
        get => _useBoundsEgyptian;
        set { if (_useBoundsEgyptian == value) return; _useBoundsEgyptian = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool ShowAspects
    {
        get => _showAspects;
        private set { if (_showAspects == value) return; _showAspects = value; OnPropertyChanged(); OnPropertyChanged(nameof(AspectsHidden)); }
    }
    public bool AspectsHidden => !_showAspects;

    public IRelayCommand ShowPositionsCommand    { get; }
    public IRelayCommand ShowFigureCommand       { get; }
    public IRelayCommand ToggleBlackWhiteCommand { get; }
    public IRelayCommand ToggleAspectsCommand    { get; }

    private ZodiacDivisionMethod DecansMethod   => _useDecansPlanet    ? ZodiacDivisionMethod.DecansPlanet    : ZodiacDivisionMethod.DecansSign;
    private ZodiacDivisionMethod DodecatsMethod => _useDodecatsOriginal ? ZodiacDivisionMethod.DodecatsOriginal : ZodiacDivisionMethod.DodecatsPaulus;
    private ZodiacDivisionMethod BoundsMethod   => _useBoundsEgyptian  ? ZodiacDivisionMethod.BoundsEgyptian  : ZodiacDivisionMethod.BoundsPtolemy;

    private static readonly Factors[] RulersForIndex = [Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus, Factors.Mars, Factors.Jupiter, Factors.Saturn];

    public ZodiacDivisionsViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta = rosetta;
        _configContext = configContext;
        _chartSession = chartSession;
        _currentChart = chartSession.SelectedChart;
        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IChartSession.SelectedChart)) return;
                _currentChart = _chartSession.SelectedChart;
                Recalculate();
            };

        ShowPositionsCommand    = new RelayCommand(() => SetTab(Tab.Positions));
        ShowFigureCommand       = new RelayCommand(() => SetTab(Tab.Figure));
        ToggleBlackWhiteCommand = new RelayCommand(() =>
        {
            _isBlackWhite = !_isBlackWhite;
            OnPropertyChanged(nameof(IsBlackWhite));
            OnPropertyChanged(nameof(Theme));
        });
        ToggleAspectsCommand = new RelayCommand(() => ShowAspects = !_showAspects);
    }

    private void SetTab(Tab tab)
    {
        if (_activeTab == tab) return;
        _activeTab = tab;
        OnPropertyChanged(nameof(ShowPositions));
        OnPropertyChanged(nameof(ShowFigure));
    }

    public void Refresh()
    {
        _currentChart = _chartSession.SelectedChart;
        Recalculate();
    }

    public void LoadChart(FullChart? chart)
    {
        _currentChart = chart;
        Recalculate();
    }

    private void Recalculate()
    {
        Rows.Clear();

        if (_currentChart == null)
        {
            HasData = false;
            PlotData = WheelPlotData.Empty;
            Marks    = [];
            OnPropertyChanged(nameof(PlotData));
            OnPropertyChanged(nameof(Marks));
            return;
        }

        var config = _configContext.ActiveConfig;
        var rowIndex = 0;
        var markList = new List<ZodiacDivisionMark>();
        var plotItems = new List<WheelPlotItem>();
        var asc = _currentChart.HousePositions.Ascendant.Longitude;
        var mc  = _currentChart.HousePositions.Midheaven.Longitude;

        foreach (var setting in config.FactorConfig.Settings)
        {
            if (!setting.IsUsed) continue;

            double longitude;
            if (setting.Factor.CalculationType() == CalculationTypes.Mundane)
                longitude = MundaneLongitude(setting.Factor, _currentChart.HousePositions);
            else if (_currentChart.Coordinates.TryGetValue(setting.Factor, out var pos) && pos.Ecliptical.Length > 0)
                longitude = pos.Ecliptical[0].MainPos;
            else continue;

            if (longitude < 0.0) continue;

            var signIdx    = ZodiacDivisionsOrchestrator.Calculate(longitude, ZodiacDivisionMethod.Signs);
            var decanIdx   = ZodiacDivisionsOrchestrator.Calculate(longitude, DecansMethod);
            var dodecatIdx = ZodiacDivisionsOrchestrator.Calculate(longitude, DodecatsMethod);
            var boundIdx   = ZodiacDivisionsOrchestrator.Calculate(longitude, BoundsMethod);

            if (signIdx < 0 || decanIdx < 0 || dodecatIdx < 0 || boundIdx < 0) continue;

            var factorGlyph  = GlyphSelector.GetGlyphForFactor(setting.Factor);
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var posStr       = ok && sign.HasValue ? dms : $"{longitude:F4}°";
            var signGlyph    = SignGlyphFor(signIdx);
            var decanGlyph   = _useDecansPlanet ? PlanetGlyphFor(decanIdx) : SignGlyphFor(decanIdx);
            var dodecatGlyph = SignGlyphFor(dodecatIdx);
            var boundGlyph   = PlanetGlyphFor(boundIdx);

            Rows.Add(new ZodiacDivisionRow(factorGlyph, posStr, signGlyph, decanGlyph, dodecatGlyph, boundGlyph, rowIndex++ % 2 == 0));

            // Wheel data
            var mundane = WheelGeometry.MundaneAngle(longitude, asc);
            markList.Add(new ZodiacDivisionMark(mundane, signGlyph, decanGlyph, dodecatGlyph, boundGlyph));

            var speedType = SpeedType.Direct;
            if (_currentChart.Coordinates.TryGetValue(setting.Factor, out var sp) && sp.Ecliptical.Length > 1)
                speedType = sp.Ecliptical[1].MainPos < 0 ? SpeedType.Retrograde : SpeedType.Direct;

            plotItems.Add(new WheelPlotItem(
                Factor:            setting.Factor,
                Glyph:             factorGlyph,
                EclipticLongitude: longitude,
                MundaneAngle:      mundane,
                PlotAngle:         mundane,
                PositionText:      PositionText(longitude),
                SpeedType:         speedType));
        }

        HasData = Rows.Count > 0;
        OnPropertyChanged(nameof(Rows));

        // Build PlotData via the standard builder so aspects are included.
        // Then overlay the division marks using the mundane angles from the built items.
        var fullPlotData = WheelPlotDataBuilder.Build(_currentChart, config);
        PlotData = fullPlotData;

        // Rebuild marks aligned to the resolved plot angles from the full builder.
        var angleMap = new Dictionary<Factors, double>();
        foreach (var item in fullPlotData.PlanetItems)
            angleMap[item.Factor] = item.MundaneAngle;

        markList.Clear();
        foreach (var setting in config.FactorConfig.Settings)
        {
            if (!setting.IsUsed) continue;
            if (!angleMap.TryGetValue(setting.Factor, out var mundane)) continue;

            double longitude;
            if (setting.Factor.CalculationType() == CalculationTypes.Mundane)
                longitude = MundaneLongitude(setting.Factor, _currentChart.HousePositions);
            else if (_currentChart.Coordinates.TryGetValue(setting.Factor, out var pos2) && pos2.Ecliptical.Length > 0)
                longitude = pos2.Ecliptical[0].MainPos;
            else continue;

            if (longitude < 0.0) continue;

            var signIdx    = ZodiacDivisionsOrchestrator.Calculate(longitude, ZodiacDivisionMethod.Signs);
            var decanIdx   = ZodiacDivisionsOrchestrator.Calculate(longitude, DecansMethod);
            var dodecatIdx = ZodiacDivisionsOrchestrator.Calculate(longitude, DodecatsMethod);
            var boundIdx   = ZodiacDivisionsOrchestrator.Calculate(longitude, BoundsMethod);

            if (signIdx < 0 || decanIdx < 0 || dodecatIdx < 0 || boundIdx < 0) continue;

            markList.Add(new ZodiacDivisionMark(
                mundane,
                SignGlyphFor(signIdx),
                _useDecansPlanet ? PlanetGlyphFor(decanIdx) : SignGlyphFor(decanIdx),
                SignGlyphFor(dodecatIdx),
                PlanetGlyphFor(boundIdx)));
        }

        Marks = [.. markList];
        OnPropertyChanged(nameof(PlotData));
        OnPropertyChanged(nameof(Marks));
    }

    private static string PositionText(double longitude)
    {
        var inSign   = longitude % 30.0;
        var totalMin = (int)(System.Math.Abs(inSign) * 60);
        var deg      = totalMin / 60;
        var min      = totalMin % 60;
        return $"{deg}°{min:D2}'";
    }

    private static string SignGlyphFor(int index)
    {
        var sign = index switch
        {
            0 => Signs.Aries,  1 => Signs.Taurus,     2 => Signs.Gemini,     3 => Signs.Cancer,
            4 => Signs.Leo,    5 => Signs.Virgo,       6 => Signs.Libra,      7 => Signs.Scorpio,
            8 => Signs.Sagittarius, 9 => Signs.Capricorn, 10 => Signs.Aquarius, 11 => Signs.Pisces,
            _ => (Signs?)null
        };
        return sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "?";
    }

    private static string PlanetGlyphFor(int index)
    {
        if (index < 0 || index >= RulersForIndex.Length) return "?";
        return GlyphSelector.GetGlyphForFactor(RulersForIndex[index]);
    }

    private static double MundaneLongitude(Factors factor, HousePositions hp) => factor switch
    {
        Factors.Ascendant => hp.Ascendant.Longitude,
        Factors.Mc        => hp.Midheaven.Longitude,
        Factors.EastPoint => hp.Eastpoint.Longitude,
        Factors.Vertex    => hp.Vertex.Longitude,
        _                 => -1.0
    };

    // i18n labels
    public string LabelTitle           => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.title");
    public string LabelNoChart         => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.nochart");
    public string LabelNoResults       => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.noresults");
    public string LabelDecansHeader    => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.decans.header");
    public string LabelDecansSigns     => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.decans.signs");
    public string LabelDecansPlanets   => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.decans.planets");
    public string LabelDodecatsHeader  => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.dodecatemoria.header");
    public string LabelDodecatsOriginal => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.dodecatemoria.original");
    public string LabelDodecatsPaulus  => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.dodecatemoria.paulus");
    public string LabelBoundsHeader    => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.bounds.header");
    public string LabelBoundsEgyptian  => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.bounds.egyptian");
    public string LabelBoundsPtolemy   => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.bounds.ptolemy");
    public string LabelBtnPositions    => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.tab.positions");
    public string LabelBtnFigure       => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.tab.figure");
    public string LabelColPosition     => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.col.position");
    public string LabelColSign         => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.col.sign");
    public string LabelColDecan        => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.col.decan");
    public string LabelColDodecat      => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.col.dodecatemoria");
    public string LabelColBound        => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.col.bound");
    public string LabelBtnBlackWhite   => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.btn.blackwhite");
    public string LabelBtnAspects      => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.btn.aspects");
    public string LabelBtnExport       => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.btn.export");
    public string TooltipFactsheet     => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.factsheet.tooltip");
    public string TooltipHelp          => _rosetta.GetText(RbFile.RadixZodiacDivisions, "zodiacdivisions.help.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
