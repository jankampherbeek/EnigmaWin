// SynastryMidpointComparisonViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Cross-chart midpoints between two selected charts: chart A's own midpoints
/// occupied by chart B's factors, and vice versa.</summary>
public sealed class SynastryMidpointComparisonViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly SynastryModel _synastryModel;
    private readonly IConfigContext _configContext;

    private MidpointDialType _dialType = MidpointDialType.Dial360;

    public SynastryMidpointComparisonViewModel(IRosetta rosetta, SynastryModel synastryModel, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _synastryModel = synastryModel;
        _configContext = configContext;

        Rebuild();
    }

    public bool HasTwoCharts => _synastryModel.HasExactlyTwo;

    public string ChartAName { get; private set; } = string.Empty;
    public string ChartBName { get; private set; } = string.Empty;

    public IReadOnlyList<MidpointMatch> MatchesA { get; private set; } = [];
    public IReadOnlyList<MidpointMatch> MatchesB { get; private set; } = [];
    public List<SynastryMidpointRow> RowsA { get; private set; } = [];
    public List<SynastryMidpointRow> RowsB { get; private set; } = [];

    public bool HasMatchesA => MatchesA.Count > 0;
    public bool HasMatchesB => MatchesB.Count > 0;

    public MidpointDialType DialType
    {
        get => _dialType;
        set { _dialType = value; OnPropertyChanged(); Rebuild(); }
    }

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle       => T("view.synastry.results.title.midpointcomparison");
    public string LabelNoMatches   => T("view.synastry.midpointcomparison.nomatches");
    public string LabelColPartner1 => T("view.synastry.midpointcomparison.col.partner1");
    public string LabelColPartner2 => T("view.synastry.midpointcomparison.col.partner2");
    public string LabelColRadix    => T("view.synastry.midpointcomparison.col.radix");
    public string LabelDial360     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.360");
    public string LabelDial90      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.90");
    public string LabelDial45      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.45");
    public string LabelColOrb      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.orb");
    public string LabelColExactness => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.exactness");
    public string LabelHelp        => T("view.synastry.help.midpointcomparison");
    public string LabelExportPdf   => T("view.synastry.exportpdf");

    private void Rebuild()
    {
        if (!_synastryModel.HasExactlyTwo)
        {
            MatchesA = [];
            MatchesB = [];
            RowsA = [];
            RowsB = [];
            NotifyAll();
            return;
        }

        var chartA = _synastryModel.ChartA!;
        var chartB = _synastryModel.ChartB!;
        ChartAName = chartA.Name;
        ChartBName = chartB.Name;

        var config = _configContext.ActiveConfig;

        MatchesA = SynastryMidpointsOrchestrator.Midpoints(
            chartA.Chart, chartB.Chart, config.FactorConfig, config.OrbConfig, _dialType);
        MatchesB = SynastryMidpointsOrchestrator.Midpoints(
            chartB.Chart, chartA.Chart, config.FactorConfig, config.OrbConfig, _dialType);

        RowsA = BuildRows(MatchesA);
        RowsB = BuildRows(MatchesB);

        NotifyAll();
    }

    private List<SynastryMidpointRow> BuildRows(IReadOnlyList<MidpointMatch> matches)
    {
        var rows = new List<SynastryMidpointRow>();
        var i = 0;
        foreach (var m in matches)
        {
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(m.MidpointPosition);
            var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";
            var totalMin  = (int)(Math.Abs(m.ActualOrb) * 60);
            var orbText   = $"{totalMin / 60}°{totalMin % 60:D2}'";
            var exactness = m.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - m.ActualOrb / m.MaxOrb) * 100)))
                : 100;
            rows.Add(new SynastryMidpointRow(
                GlyphSelector.GetGlyphForFactor(m.Factor1),
                GlyphSelector.GetGlyphForFactor(m.Factor2),
                dms, signGlyph,
                GlyphSelector.GetGlyphForFactor(m.MatchingFactor),
                orbText, $"{exactness}%", i++ % 2 == 0));
        }
        return rows;
    }

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(HasTwoCharts));
        OnPropertyChanged(nameof(ChartAName));
        OnPropertyChanged(nameof(ChartBName));
        OnPropertyChanged(nameof(MatchesA));
        OnPropertyChanged(nameof(MatchesB));
        OnPropertyChanged(nameof(RowsA));
        OnPropertyChanged(nameof(RowsB));
        OnPropertyChanged(nameof(HasMatchesA));
        OnPropertyChanged(nameof(HasMatchesB));
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
