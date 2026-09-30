// AltZodiacStartViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

/// <summary>A factor that can be chosen as the start of the alternative zodiac.</summary>
public sealed record AltZodiacStartFactorOption(Factors Factor, string Glyph, string Name);

/// <summary>One row of the positions table: the original and the shifted position of a factor.</summary>
public sealed record AltZodiacStartRow(
    string FactorGlyph,
    string OriginalDms,
    string OriginalSignGlyph,
    string ShiftedDms,
    string ShiftedSignGlyph,
    bool IsStartFactor,
    bool IsEvenRow);

/// <summary>
/// Shared ViewModel for the AltZodiacStart input screen (choice of start factor) and results screen
/// (positions table and wheel). Registered as a singleton so both screens stay in sync.
/// </summary>
public sealed partial class AltZodiacStartViewModel : ObservableObject
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private readonly IChartSession _chartSession;

    private enum Tab { Positions, Wheel }
    private Tab _activeTab = Tab.Positions;

    /// <summary>Factors that are used in the active configuration; any of them can be the start factor.</summary>
    public ObservableCollection<AltZodiacStartFactorOption> FactorOptions { get; } = [];

    public ObservableCollection<AltZodiacStartRow> Rows { get; } = [];

    public WheelPlotData PlotData { get; private set; } = WheelPlotData.Empty;
    public double ZodiacStartLongitude { get; private set; }
    public WheelTheme Theme => IsBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartFactorGlyph), nameof(StartFactorName))]
    private Factors _startFactor = Factors.Sun;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoChart), nameof(HasNoResults))]
    private bool _hasChart;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoResults))]
    private bool _hasRows;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Theme))]
    private bool _isBlackWhite;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AspectsHidden))]
    private bool _showAspects = true;

    public bool HasNoChart    => !HasChart;
    public bool HasNoResults  => HasChart && !HasRows;
    public bool HasNoFactors  => FactorOptions.Count == 0;
    public bool AspectsHidden => !ShowAspects;
    public bool ShowPositions => _activeTab == Tab.Positions;
    public bool ShowWheel     => _activeTab == Tab.Wheel;

    public string StartFactorGlyph => GlyphSelector.GetGlyphForFactor(StartFactor);
    public string StartFactorName  => _rosetta.GetText(RbFile.Localizable, StartFactor.LocalizedName());

    public IRelayCommand ShowPositionsCommand    { get; }
    public IRelayCommand ShowWheelCommand        { get; }
    public IRelayCommand ToggleBlackWhiteCommand { get; }
    public IRelayCommand ToggleAspectsCommand    { get; }

    public AltZodiacStartViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta = rosetta;
        _configContext = configContext;
        _chartSession = chartSession;
        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IChartSession.SelectedChart)) Recalculate();
            };

        ShowPositionsCommand    = new RelayCommand(() => SetTab(Tab.Positions));
        ShowWheelCommand        = new RelayCommand(() => SetTab(Tab.Wheel));
        ToggleBlackWhiteCommand = new RelayCommand(() => IsBlackWhite = !IsBlackWhite);
        ToggleAspectsCommand    = new RelayCommand(() => ShowAspects = !ShowAspects);

        Refresh();
    }

    /// <summary>
    /// Reloads the selectable factors from the active configuration and recalculates for the selected chart.
    /// Called whenever one of the screens is shown, since the configuration may have changed in between.
    /// </summary>
    public void Refresh()
    {
        RebuildFactorOptions();
        Recalculate();
    }

    partial void OnStartFactorChanged(Factors value) => Recalculate();

    private void SetTab(Tab tab)
    {
        if (_activeTab == tab) return;
        _activeTab = tab;
        OnPropertyChanged(nameof(ShowPositions));
        OnPropertyChanged(nameof(ShowWheel));
    }

    private IEnumerable<Factors> UsedFactors()
    {
        var used = _configContext.ActiveConfig.FactorConfig.Settings
            .Where(s => s.IsUsed)
            .Select(s => s.Factor)
            .ToHashSet();
        return FactorsExtensions.SelectableFactors.Where(used.Contains);
    }

    private void RebuildFactorOptions()
    {
        FactorOptions.Clear();
        foreach (var factor in UsedFactors())
            FactorOptions.Add(new AltZodiacStartFactorOption(
                factor,
                GlyphSelector.GetGlyphForFactor(factor),
                _rosetta.GetText(RbFile.Localizable, factor.LocalizedName())));
        OnPropertyChanged(nameof(HasNoFactors));

        // Fall back to the first selectable factor when the current one is no longer used in the configuration.
        if (FactorOptions.Count > 0 && FactorOptions.All(o => o.Factor != StartFactor))
            StartFactor = FactorOptions[0].Factor;
        // Clearing the options resets the list's selection; re-announce the current factor so it is shown again.
        OnPropertyChanged(nameof(StartFactor));
    }

    private void Recalculate()
    {
        Rows.Clear();
        var chart = _chartSession.SelectedChart;
        HasChart = chart is not null;

        var startLongitude = chart is null ? null : AltZodiacStartOrchestrator.LongitudeOf(StartFactor, chart);
        if (chart is null || startLongitude is null)
        {
            HasRows = false;
            PlotData = WheelPlotData.Empty;
            ZodiacStartLongitude = 0.0;
            OnPropertyChanged(nameof(PlotData));
            OnPropertyChanged(nameof(ZodiacStartLongitude));
            return;
        }

        var asc = chart.HousePositions.Ascendant.Longitude;
        var rows = new List<(double MundaneAngle, AltZodiacStartRow Row)>();
        foreach (var factor in UsedFactors())
        {
            if (AltZodiacStartOrchestrator.LongitudeOf(factor, chart) is not { } longitude) continue;
            var shifted = AltZodiacStartOrchestrator.ShiftedLongitude(longitude, startLongitude.Value);
            var (originalDms, originalGlyph) = FormatPosition(longitude);
            var (shiftedDms, shiftedGlyph)   = FormatPosition(shifted);
            rows.Add((WheelGeometry.MundaneAngle(longitude, asc), new AltZodiacStartRow(
                GlyphSelector.GetGlyphForFactor(factor),
                originalDms, originalGlyph,
                shiftedDms, shiftedGlyph,
                factor == StartFactor,
                false)));
        }

        var index = 0;
        foreach (var (_, row) in rows.OrderBy(r => r.MundaneAngle))
            Rows.Add(row with { IsEvenRow = index++ % 2 == 0 });
        HasRows = Rows.Count > 0;

        PlotData = AltZodiacStartOrchestrator.WithShiftedPositionTexts(
            WheelPlotDataBuilder.Build(chart, _configContext.ActiveConfig), startLongitude.Value);
        ZodiacStartLongitude = startLongitude.Value;
        OnPropertyChanged(nameof(PlotData));
        OnPropertyChanged(nameof(ZodiacStartLongitude));
    }

    private static (string Dms, string SignGlyph) FormatPosition(double longitude)
    {
        var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
        return ok && sign.HasValue
            ? (dms, GlyphSelector.GetGlyphForSign(sign.Value))
            : ($"{longitude:F4}°", "");
    }

    // ── Labels ───────────────────────────────────────────────────────────────

    private string T(string key) => _rosetta.GetText(RbFile.RadixAltZodiacStart, key);

    public string LabelInputTitle       => T("altzodiacstart.input.title");
    public string LabelFactorHeader     => T("altzodiacstart.input.factor.header");
    public string LabelNoFactors        => T("altzodiacstart.input.factor.nofactors");
    public string LabelTitle            => T("altzodiacstart.title");
    public string LabelNoChart          => T("altzodiacstart.nochart");
    public string LabelNoResults        => T("altzodiacstart.noresults");
    public string LabelBtnPositions     => T("altzodiacstart.tab.positions");
    public string LabelBtnWheel         => T("altzodiacstart.tab.wheel");
    public string LabelColOriginal      => T("altzodiacstart.col.original");
    public string LabelColShifted       => T("altzodiacstart.col.shifted");
    public string LabelStartFactor      => T("altzodiacstart.startfactorlabel");
    public string LabelBtnBlackWhite    => T("altzodiacstart.btn.blackwhite");
    public string LabelBtnAspects       => T("altzodiacstart.btn.aspects");
    public string LabelBtnExport        => T("altzodiacstart.btn.export");
    public string TooltipFactsheet      => T("altzodiacstart.factsheet.tooltip");
    public string TooltipHelp           => T("altzodiacstart.help.tooltip");
}
