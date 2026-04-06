// DeclinationLongEquivalentsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.UI;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.Sources.Features.Speed;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclinationLongEquivalentsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta       _rosetta;
    private readonly IConfigContext _configContext;

    private bool         _isBlackWhite;
    private bool         _hideTime;
    private bool         _hideAspects;
    private DrawingTypes _drawingType;
    private bool         _hasData;

    // ── Row record ────────────────────────────────────────────────────────
    public sealed record EquivRow(
        string FactorGlyph,
        string LongitudeDms,
        string LongitudeSignGlyph,
        string EquivDms,
        string EquivSignGlyph,
        bool   IsEvenRow);

    // ── Observable collections ────────────────────────────────────────────
    public ObservableCollection<EquivRow> Rows { get; } = [];

    // ── Commands ──────────────────────────────────────────────────────────
    public IRelayCommand ToggleBlackWhiteCommand { get; }
    public IRelayCommand ToggleTimeCommand       { get; }
    public IRelayCommand ToggleAspectsCommand    { get; }

    public DeclinationLongEquivalentsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;
        _drawingType   = configContext.ActiveConfig.DisplayConfig.DrawingType;

        ToggleBlackWhiteCommand = new RelayCommand(ToggleBlackWhite);
        ToggleTimeCommand       = new RelayCommand(ToggleTime);
        ToggleAspectsCommand    = new RelayCommand(ToggleAspects);
    }

    // ── Toggle state ──────────────────────────────────────────────────────

    public bool IsBlackWhite
    {
        get => _isBlackWhite;
        private set { if (_isBlackWhite == value) return; _isBlackWhite = value; RaiseAll(nameof(IsBlackWhite), nameof(Theme), nameof(LabelBlackWhite)); }
    }

    public bool HideTime
    {
        get => _hideTime;
        private set
        {
            if (_hideTime == value) return;
            _hideTime = value;
            RaiseAll(nameof(HideTime), nameof(LabelTime), nameof(PlotData), nameof(HousePlotData),
                nameof(DialPlotData), nameof(Dial90PlotData), nameof(Dial45PlotData));
        }
    }

    public bool HideAspects
    {
        get => _hideAspects;
        private set { if (_hideAspects == value) return; _hideAspects = value; RaiseAll(nameof(HideAspects), nameof(ShowAspects), nameof(LabelAspects)); }
    }

    public WheelTheme Theme       => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;
    public bool       ShowAspects => !HideAspects;

    // ── HasData ───────────────────────────────────────────────────────────

    public bool HasData
    {
        get => _hasData;
        private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); }
    }

    public bool HasNoData => !HasData;

    // ── Drawing type ──────────────────────────────────────────────────────

    public DrawingTypes DrawingType => _drawingType;

    public void SetDrawingType(DrawingTypes type)
    {
        if (_drawingType == type) return;
        _drawingType = type;
        RaiseAll(nameof(DrawingType), nameof(IsZodiacWheel), nameof(IsHouseWheel), nameof(IsFrenchWheel),
            nameof(IsRingWheel), nameof(IsDial360Wheel), nameof(IsDial90Wheel), nameof(IsDial45Wheel),
            nameof(IsAnyDial), nameof(DialPlotData), nameof(Dial90PlotData), nameof(Dial45PlotData));
    }

    public bool IsZodiacWheel  => DrawingType == DrawingTypes.SignBased;
    public bool IsHouseWheel   => DrawingType == DrawingTypes.HouseBased;
    public bool IsFrenchWheel  => DrawingType == DrawingTypes.French;
    public bool IsRingWheel    => DrawingType == DrawingTypes.Ring;
    public bool IsDial360Wheel => DrawingType == DrawingTypes.Dial360;
    public bool IsDial90Wheel  => DrawingType == DrawingTypes.Dial90;
    public bool IsDial45Wheel  => DrawingType == DrawingTypes.Dial45;
    public bool IsAnyDial      => IsDial360Wheel || IsDial90Wheel || IsDial45Wheel;

    // ── Cached plot data ──────────────────────────────────────────────────

    private WheelPlotData _rawPlotData      = WheelPlotData.Empty;
    private WheelPlotData _rawHousePlotData = WheelPlotData.Empty;
    private WheelPlotData _rawDialPlotData  = WheelPlotData.Empty;
    private WheelPlotData _rawDial90PlotData = WheelPlotData.Empty;
    private WheelPlotData _rawDial45PlotData = WheelPlotData.Empty;

    public WheelPlotData PlotData       => ZodiacTypeWheelViewModel.EffectiveData(_rawPlotData,      HideTime);
    public WheelPlotData HousePlotData  => ZodiacTypeWheelViewModel.EffectiveData(_rawHousePlotData, HideTime);
    public WheelPlotData DialPlotData   => DialPlotDataBuilder.EffectiveData(_rawDialPlotData,       HideTime);
    public WheelPlotData Dial90PlotData => Dial90PlotDataBuilder.EffectiveData(_rawDial90PlotData,   HideTime);
    public WheelPlotData Dial45PlotData => Dial45PlotDataBuilder.EffectiveData(_rawDial45PlotData,   HideTime);

    // ── Localized labels ──────────────────────────────────────────────────

    public string LabelTitle     => T(RbFile.RadixDeclinations, "declinations.equiv.title");
    public string LabelEmpty     => T(RbFile.RadixDeclinations, "declinations.equiv.nodata");
    public string LabelColFactor => T(RbFile.RadixDeclinations, "declinations.equiv.col.factor");
    public string LabelColLon    => T(RbFile.RadixDeclinations, "declinations.equiv.col.longitude");
    public string LabelColEquiv  => T(RbFile.RadixDeclinations, "declinations.equiv.col.equiv");
    public string TooltipHelp    => T(RbFile.RadixDeclinations, "declinations.help.tooltip");

    // Re-use ChartWheel strings for the shared toggle buttons
    public string LabelBlackWhite => TW(IsBlackWhite ? ChartWheelKeys.ColorButton       : ChartWheelKeys.BlackWhiteButton);
    public string LabelTime       => TW(HideTime     ? ChartWheelKeys.WithTimeButton    : ChartWheelKeys.NoTimeButton);
    public string LabelAspects    => TW(HideAspects  ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelExport     => TW(ChartWheelKeys.ExportButton);
    public string LabelDial360    => TW(ChartWheelKeys.DialType360);
    public string LabelDial90     => TW(ChartWheelKeys.DialType90);
    public string LabelDial45     => TW(ChartWheelKeys.DialType45);

    // ── Chart loading ─────────────────────────────────────────────────────

    public void LoadChart(FullChart? chart)
    {
        Rows.Clear();

        if (chart is null)
        {
            _rawPlotData      = WheelPlotData.Empty;
            _rawHousePlotData = WheelPlotData.Empty;
            _rawDialPlotData  = WheelPlotData.Empty;
            _rawDial90PlotData = WheelPlotData.Empty;
            _rawDial45PlotData = WheelPlotData.Empty;
            HasData = false;
            RaisePlotProperties();
            OnPropertyChanged(nameof(Rows));
            return;
        }

        var config = _configContext.ActiveConfig;
        var equivs = DeclinationsOrchestrator.LongitudeEquivalents(chart, config.FactorConfig);

        // Build a patched chart with equivalent longitudes substituted for ecliptic longitudes
        var patchedChart = PatchChartWithEquivLongitudes(chart, equivs);

        // Build all plot data variants from the patched chart
        _rawPlotData      = WheelPlotDataBuilder.Build(patchedChart, config);
        _rawHousePlotData = HouseWheelPlotDataBuilder.Build(patchedChart, config);
        _rawDialPlotData  = DialPlotDataBuilder.Build(patchedChart, config);
        _rawDial90PlotData = Dial90PlotDataBuilder.Build(patchedChart, config);
        _rawDial45PlotData = Dial45PlotDataBuilder.Build(patchedChart, config);

        // Update drawing type from config
        _drawingType = config.DisplayConfig.DrawingType;
        RaiseAll(nameof(DrawingType), nameof(IsZodiacWheel), nameof(IsHouseWheel), nameof(IsFrenchWheel),
            nameof(IsRingWheel), nameof(IsDial360Wheel), nameof(IsDial90Wheel), nameof(IsDial45Wheel),
            nameof(IsAnyDial));
        RaisePlotProperties();

        // Build the equivalent longitude map for table display
        var equivMap = equivs.ToDictionary(e => e.Factor, e => e.LongitudeEquivalent);

        // Build table rows
        var rowIndex = 0;
        foreach (var e in equivs)
        {
            var factorGlyph = GlyphSelector.GetGlyphForFactor(e.Factor);

            // Normal longitude from original chart
            var longitude = GetLongitude(e.Factor, chart);
            var (lonDms, lonSign, lonOk)       = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var (equivDms, equivSign, equivOk) = PositionInDegreesConversion.DoubleToDmsSign(e.LongitudeEquivalent);
            var lonSignGlyph   = lonOk   && lonSign.HasValue   ? GlyphSelector.GetGlyphForSign(lonSign.Value)   : "";
            var equivSignGlyph = equivOk && equivSign.HasValue ? GlyphSelector.GetGlyphForSign(equivSign.Value) : "";

            Rows.Add(new EquivRow(factorGlyph, lonDms, lonSignGlyph, equivDms, equivSignGlyph, rowIndex++ % 2 == 0));
        }

        HasData = Rows.Count > 0;
        OnPropertyChanged(nameof(Rows));
    }

    // ── Patch chart ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns a copy of the chart where each factor's ecliptic MainPos is replaced
    /// by its longitude equivalent. The equatorial and other data are preserved.
    /// </summary>
    private static FullChart PatchChartWithEquivLongitudes(
        FullChart chart, List<LongEquivalentResult> equivs)
    {
        var equivMap = equivs.ToDictionary(e => e.Factor, e => e.LongitudeEquivalent);
        var newCoords = new Dictionary<Factors, FullFactorPosition>();

        foreach (var (factor, pos) in chart.Coordinates)
        {
            if (!equivMap.TryGetValue(factor, out var equivLon))
            {
                newCoords[factor] = pos;
                continue;
            }

            // Replace the first ecliptic position's MainPos with the equivalent longitude
            var newEcliptical = pos.Ecliptical.Length == 0
                ? pos.Ecliptical
                : pos.Ecliptical
                    .Select((p, i) => i == 0
                        ? new MainAstronomicalPosition(
                            MainPos:      equivLon,
                            Deviation:    p.Deviation,
                            Distance:     p.Distance,
                            MainPosSpeed: p.MainPosSpeed,
                            DeviationSpeed: p.DeviationSpeed,
                            DistanceSpeed:  p.DistanceSpeed)
                        : p)
                    .ToArray();

            newCoords[factor] = new FullFactorPosition(newEcliptical, pos.Equatorial, pos.Horizontal);
        }

        return new FullChart(newCoords, chart.HousePositions, chart.SiderealTime, chart.JulianDay, chart.Obliquity);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static double GetLongitude(Factors factor, FullChart chart)
    {
        if (chart.Coordinates.TryGetValue(factor, out var pos) && pos.Ecliptical.Length > 0)
            return pos.Ecliptical[0].MainPos;

        FullCuspPosition? cusp = factor switch
        {
            Factors.Ascendant => chart.HousePositions.Ascendant,
            Factors.Mc        => chart.HousePositions.Midheaven,
            Factors.EastPoint => chart.HousePositions.Eastpoint,
            Factors.Vertex    => chart.HousePositions.Vertex,
            _                 => null
        };
        return cusp?.Longitude ?? 0.0;
    }

    private void RaisePlotProperties() =>
        RaiseAll(nameof(PlotData), nameof(HousePlotData), nameof(DialPlotData),
            nameof(Dial90PlotData), nameof(Dial45PlotData));

    private void RaiseAll(params string[] names)
    {
        foreach (var n in names) OnPropertyChanged(n);
    }

    private void ToggleBlackWhite() => IsBlackWhite = !IsBlackWhite;
    private void ToggleTime()       => HideTime     = !HideTime;
    private void ToggleAspects()    => HideAspects  = !HideAspects;

    private string T(RbFile file, string key) => _rosetta.GetText(file, key);
    private string TW(string key)             => _rosetta.GetText(RbFile.ChartWheel, key);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
