// HarmonicOrbsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs.UI;

/// <summary>Shared state for the Harmonic Orbs feature (a DI singleton): the input screen edits the
/// maximum orb and the aspect selection; the drawing screen observes the same instance to redraw
/// the chart and rebuild the aspect grid/list. Finds aspects via a harmonic-based orb (maximum orb
/// divided by each aspect's harmonic number) instead of the app's regular Aspect/Orb configuration.</summary>
public sealed class HarmonicOrbsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private readonly IChartSession _chartSession;
    private readonly RadixAspectsModel _rowBuilder = new();

    private NamedChart? _currentChart;
    private int _orbDegrees = 15;
    private int _orbMinutes;
    private bool _hasData;
    private bool _isBlackWhite;

    private enum Tab { Chart, Aspects }
    private Tab _activeTab = Tab.Chart;

    public HarmonicOrbsViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta = rosetta;
        _configContext = configContext;
        _chartSession = chartSession;
        _currentChart = chartSession.Selected;

        Settings = new ObservableCollection<HarmonicOrbSettingRow>(
            HarmonicOrbSetting.Defaults.Select((s, i) => new HarmonicOrbSettingRow(
                s.Aspect,
                s.HarmonicNumber,
                GlyphSelector.GetGlyphForAspect(s.Aspect),
                rosetta.GetText(RbFile.Localizable, s.Aspect.LocalizedName()),
                i % 2 == 0,
                Recalculate)));
        RefreshEffectiveOrbs();

        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IChartSession.SelectedChart)) return;
                _currentChart = _chartSession.Selected;
                OnPropertyChanged(nameof(ChartName));
                OnPropertyChanged(nameof(LabelTitle));
                Recalculate();
            };

        ShowChartCommand = new RelayCommand(() => SetTab(Tab.Chart));
        ShowAspectsCommand = new RelayCommand(() => SetTab(Tab.Aspects));
        ToggleBlackWhiteCommand = new RelayCommand(() => IsBlackWhite = !IsBlackWhite);

        Recalculate();
    }

    // ── Aspect selection ────────────────────────────────────────────────────

    public ObservableCollection<HarmonicOrbSettingRow> Settings { get; }

    public int OrbDegrees
    {
        get => _orbDegrees;
        set
        {
            var clamped = Math.Max(0, value);
            if (_orbDegrees == clamped) return;
            _orbDegrees = clamped;
            OnPropertyChanged();
            RefreshEffectiveOrbs();
            Recalculate();
        }
    }

    public int OrbMinutes
    {
        get => _orbMinutes;
        set
        {
            var clamped = Math.Clamp(value, 0, 59);
            if (_orbMinutes == clamped) return;
            _orbMinutes = clamped;
            OnPropertyChanged();
            RefreshEffectiveOrbs();
            Recalculate();
        }
    }

    public double MaxOrbDegrees => OrbDegrees + OrbMinutes / 60.0;

    private void RefreshEffectiveOrbs()
    {
        foreach (var row in Settings) row.UpdateEffectiveOrb(MaxOrbDegrees);
        OnPropertyChanged(nameof(MaxOrbDegrees));
    }

    // ── Tab switching ───────────────────────────────────────────────────────

    public IRelayCommand ShowChartCommand { get; }
    public IRelayCommand ShowAspectsCommand { get; }

    public bool ShowChart => _activeTab == Tab.Chart;
    public bool ShowAspects => _activeTab == Tab.Aspects;

    private void SetTab(Tab tab)
    {
        if (_activeTab == tab) return;
        _activeTab = tab;
        OnPropertyChanged(nameof(ShowChart));
        OnPropertyChanged(nameof(ShowAspects));
    }

    // ── Chart display ───────────────────────────────────────────────────────

    public IRelayCommand ToggleBlackWhiteCommand { get; }

    public bool IsBlackWhite
    {
        get => _isBlackWhite;
        set
        {
            if (_isBlackWhite == value) return;
            _isBlackWhite = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Theme));
        }
    }

    public WheelTheme Theme => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;

    public string ChartName => _currentChart?.Name ?? "";
    public bool HasData { get => _hasData; private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;

    /// <summary>The normal chart wheel's planets/cusps, with AspectItems replaced by the
    /// harmonic-orb aspects instead of the active configuration's aspect/orb settings.</summary>
    public WheelPlotData PlotData
    {
        get
        {
            if (_currentChart is null) return WheelPlotData.Empty;

            var chart = _currentChart.Chart;
            var activeConfig = _configContext.ActiveConfig;
            var basePlot = WheelPlotDataBuilder.Build(chart, activeConfig);

            var foundAspects = HarmonicOrbsOrchestrator.Calculate(
                chart, activeConfig.FactorConfig, SettingsTuples(), MaxOrbDegrees);

            var angleMap = new Dictionary<Factors, double>();
            foreach (var item in basePlot.PlanetItems)
                angleMap[item.Factor] = item.MundaneAngle;

            var colorMap = new Dictionary<Domain.Aspects, Color>();
            foreach (var setting in activeConfig.AspectConfig.Settings)
            {
                var c = setting.Color;
                colorMap[setting.Aspect] = Color.FromArgb(
                    (byte)(c.Opacity * 255), (byte)(c.Red * 255), (byte)(c.Green * 255), (byte)(c.Blue * 255));
            }

            var aspectItems = new List<WheelAspectItem>();
            foreach (var found in foundAspects)
            {
                if (!angleMap.TryGetValue(found.Factor1, out var angle1)) continue;
                if (!angleMap.TryGetValue(found.Factor2, out var angle2)) continue;

                colorMap.TryGetValue(found.Aspect, out var color);
                var exactness = found.MaxOrb > 0 ? Math.Max(0.0, 1.0 - found.Orb / found.MaxOrb) : 1.0;
                aspectItems.Add(new WheelAspectItem(angle1, angle2, color, exactness, found.Aspect));
            }

            return new WheelPlotData(
                basePlot.AscendantLongitude,
                basePlot.McLongitude,
                basePlot.CuspLongitudes,
                basePlot.PlanetItems,
                basePlot.HasTime,
                [.. aspectItems]);
        }
    }

    // ── Aspects grid + list ─────────────────────────────────────────────────

    public ObservableCollection<RadixAspectsModel.AspectRow> AspectRows { get; } = [];

    public event Action<List<Factors>, List<AspectGridControl.AspectCell>>? GridDataReady;

    private List<(Domain.Aspects Aspect, int HarmonicNumber, bool IsSelected)> SettingsTuples() =>
        Settings.Select(s => (s.Aspect, s.HarmonicNumber, s.IsSelected)).ToList();

    private void Recalculate()
    {
        AspectRows.Clear();

        if (_currentChart is null)
        {
            HasData = false;
            GridDataReady?.Invoke([], []);
            OnPropertyChanged(nameof(PlotData));
            return;
        }

        try
        {
            var chart = _currentChart.Chart;
            var activeConfig = _configContext.ActiveConfig;
            var factorConfig = activeConfig.FactorConfig;

            var foundAspects = HarmonicOrbsOrchestrator.Calculate(
                chart, factorConfig, SettingsTuples(), MaxOrbDegrees);

            foreach (var row in _rowBuilder.BuildRows(foundAspects, _rosetta))
                AspectRows.Add(row);

            HasData = true;

            var factors = BuildActiveFactors(chart, factorConfig);
            var cells = BuildGridCells(foundAspects, activeConfig.AspectConfig);
            GridDataReady?.Invoke(factors, cells);

            OnPropertyChanged(nameof(PlotData));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "HarmonicOrbs calculation failed.");
            HasData = false;
        }
    }

    private static List<Factors> BuildActiveFactors(FullChart chart, FactorConfig factorConfig)
    {
        var result = new List<Factors>();
        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;
            if (chart.Coordinates.TryGetValue(setting.Factor, out var pos) && pos.Ecliptical.Length > 0)
                result.Add(setting.Factor);
        }
        return result;
    }

    private static List<AspectGridControl.AspectCell> BuildGridCells(
        IReadOnlyList<FoundAspect> aspects,
        AspectConfig aspectConfig)
    {
        var colorMap = aspectConfig.Settings.ToDictionary(s => s.Aspect, s => s.Color);

        var cells = new List<AspectGridControl.AspectCell>();
        foreach (var found in aspects)
        {
            var lo = Math.Min((int)found.Factor1, (int)found.Factor2);
            var hi = Math.Max((int)found.Factor1, (int)found.Factor2);

            var colorCfg = colorMap.TryGetValue(found.Aspect, out var cc) ? cc : new ColorConfig(0.5, 0.5, 0.5);
            var wpfColor = Color.FromArgb(
                (byte)(colorCfg.Opacity * 255),
                (byte)(colorCfg.Red * 255),
                (byte)(colorCfg.Green * 255),
                (byte)(colorCfg.Blue * 255));

            cells.Add(new AspectGridControl.AspectCell(lo, hi, GlyphSelector.GetGlyphForAspect(found.Aspect), wpfColor));
        }
        return cells;
    }

    // ── Labels ──────────────────────────────────────────────────────────────

    public string LabelTitle => string.Format(_rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.title"), ChartName);
    public string LabelNoChart => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.nochart");
    public string LabelDrawingNoChart => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.drawing.nochart");
    public string LabelOrbSection => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.orb.section");
    public string LabelOrbLabel => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.orb.label");
    public string LabelAspectsSection => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.aspects.section");
    public string LabelColAspect => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.col.aspect");
    public string LabelColEffectiveOrb => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.col.effectiveorb");
    public string LabelTabChart => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.tab.chart");
    public string LabelTabAspects => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.tab.aspects");
    public string LabelGridTitle => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.grid.title");
    public string LabelListTitle => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.list.title");
    public string LabelColFactor1 => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.col.factor1");
    public string LabelColFactor2 => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.col.factor2");
    public string LabelColOrb => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.col.orb");
    public string LabelColExactness => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.col.exactness");
    public string LabelNoAspectsFound => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.noaspectsfound");
    public string TooltipHelp => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.help.tooltip");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.factsheet.tooltip");
    public string TooltipBlackWhite => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.blackwhite.tooltip");
    public string TooltipExport => _rosetta.GetText(RbFile.RadixHarmonicOrbs, "harmonicorbs.export.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
