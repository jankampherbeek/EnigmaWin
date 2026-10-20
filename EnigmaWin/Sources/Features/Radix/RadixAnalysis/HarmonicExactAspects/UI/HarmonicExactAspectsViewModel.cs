// HarmonicExactAspectsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicExactAspects.UI;

/// <summary>ViewModel for the Harmonic Exact Aspects screen: for each harmonic, the aspect with the smallest
/// orb between the harmonic positions. Factors and aspects follow the configuration, but the user can
/// (de)select the Ascendant and MC, choose between the configured and the major aspects, and change the
/// maximum harmonic.</summary>
public sealed class HarmonicExactAspectsViewModel : INotifyPropertyChanged
{
    public sealed record ExactAspectRow(
        string HarmonicText,
        string Factor1Glyph,
        string Factor1Name,
        string AspectGlyph,
        string AspectName,
        string Factor2Glyph,
        string Factor2Name,
        string OrbText,
        bool IsEvenRow);

    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;

    private NamedChart? _currentChart;
    private bool _hasData;
    private string _chartName = "";
    private bool _includeAscendant;
    private bool _includeMc;
    private bool _useMajorAspects;
    private int _maxHarmonic = HarmonicExactAspectsOrchestrator.DefaultMaxHarmonic;

    public HarmonicExactAspectsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta = rosetta;
        _configContext = configContext;

        var factorSettings = configContext.ActiveConfig.FactorConfig.Settings;
        _includeAscendant = factorSettings.Any(s => s.Factor == Factors.Ascendant && s.IsUsed);
        _includeMc = factorSettings.Any(s => s.Factor == Factors.Mc && s.IsUsed);

        DecreaseMaxHarmonicCommand = new RelayCommand(() => MaxHarmonic--);
        IncreaseMaxHarmonicCommand = new RelayCommand(() => MaxHarmonic++);
    }

    // ── Options ──────────────────────────────────────────────────────────────

    public bool IncludeAscendant
    {
        get => _includeAscendant;
        set { if (_includeAscendant == value) return; _includeAscendant = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool IncludeMc
    {
        get => _includeMc;
        set { if (_includeMc == value) return; _includeMc = value; OnPropertyChanged(); Recalculate(); }
    }

    public bool UseMajorAspects
    {
        get => _useMajorAspects;
        set
        {
            if (_useMajorAspects == value) return;
            _useMajorAspects = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UseConfiguredAspects));
            Recalculate();
        }
    }

    public bool UseConfiguredAspects
    {
        get => !UseMajorAspects;
        set => UseMajorAspects = !value;
    }

    public int MaxHarmonic
    {
        get => _maxHarmonic;
        set
        {
            var clamped = Math.Clamp(value, HarmonicExactAspectsOrchestrator.MinHarmonic, HarmonicExactAspectsOrchestrator.MaxHarmonicLimit);
            if (_maxHarmonic == clamped)
            {
                // Refresh the text box when an out-of-range value was entered and clamped to the current value.
                OnPropertyChanged();
                return;
            }
            _maxHarmonic = clamped;
            OnPropertyChanged();
            Recalculate();
        }
    }

    public IRelayCommand DecreaseMaxHarmonicCommand { get; }
    public IRelayCommand IncreaseMaxHarmonicCommand { get; }

    // ── Results ──────────────────────────────────────────────────────────────

    public bool HasData { get => _hasData; private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;
    public string ChartName { get => _chartName; private set { if (_chartName == value) return; _chartName = value; OnPropertyChanged(); OnPropertyChanged(nameof(LabelHeaderTitle)); } }

    public ObservableCollection<ExactAspectRow> Rows { get; } = [];

    public bool HasRows => Rows.Count > 0;
    public bool HasNoRows => HasData && Rows.Count == 0;

    public void LoadChart(NamedChart? namedChart)
    {
        _currentChart = namedChart;
        ChartName = namedChart?.Name ?? "";
        Recalculate();
    }

    private void Recalculate()
    {
        Rows.Clear();

        if (_currentChart == null)
        {
            HasData = false;
            OnPropertyChanged(nameof(HasRows));
            OnPropertyChanged(nameof(HasNoRows));
            return;
        }

        try
        {
            var found = HarmonicExactAspectsOrchestrator.Calculate(
                _currentChart.Chart, _configContext.ActiveConfig,
                IncludeAscendant, IncludeMc, UseMajorAspects, MaxHarmonic);

            var index = 0;
            foreach (var item in found)
            {
                Rows.Add(new ExactAspectRow(
                    HarmonicText: item.Harmonic.ToString(),
                    Factor1Glyph: GlyphSelector.GetGlyphForFactor(item.Factor1),
                    Factor1Name:  _rosetta.GetText(RbFile.Localizable, item.Factor1.LocalizedName()),
                    AspectGlyph:  GlyphSelector.GetGlyphForAspect(item.Aspect),
                    AspectName:   _rosetta.GetText(RbFile.Localizable, item.Aspect.LocalizedName()),
                    Factor2Glyph: GlyphSelector.GetGlyphForFactor(item.Factor2),
                    Factor2Name:  _rosetta.GetText(RbFile.Localizable, item.Factor2.LocalizedName()),
                    OrbText:      PositionInDegreesConversion.DoubleToDms(item.Orb),
                    IsEvenRow:    index++ % 2 == 0));
            }

            HasData = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Harmonic exact aspects calculation failed.");
            HasData = false;
        }

        OnPropertyChanged(nameof(HasRows));
        OnPropertyChanged(nameof(HasNoRows));
    }

    // ── Labels ───────────────────────────────────────────────────────────────

    public string LabelTitle => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.title");
    public string LabelNoChart => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.nochart");
    public string LabelNoRows => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.norows");
    public string LabelHeaderTitle => string.Format(_rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.headertitle.format"), ChartName);
    public string LabelFactorsSection => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.factors.section");
    public string LabelIncludeAscendant => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.include.ascendant");
    public string LabelIncludeMc => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.include.mc");
    public string LabelAspectsSection => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.aspects.section");
    public string LabelConfiguredAspects => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.aspects.configured");
    public string LabelMajorAspects => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.aspects.major");
    public string LabelMaxHarmonic => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.maxharmonic");
    public string LabelColHarmonic => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.col.harmonic");
    public string LabelColAspect => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.col.aspect");
    public string LabelColOrb => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.col.orb");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.factsheet.tooltip");
    public string TooltipHelp =>_rosetta.GetText(RbFile.RadixHarmonicExactAspects, "harmonicexact.help.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
