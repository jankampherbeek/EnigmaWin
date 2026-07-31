// SynastryDeclinationComparisonViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Progressive;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Cross-chart parallels/contra-parallels between two selected charts, shown as two
/// directional tables (same underlying data, ordered from each person's perspective).</summary>
public sealed class SynastryDeclinationComparisonViewModel : INotifyPropertyChanged
{
    // Parallel/contra-parallel glyphs live in the EnigmaAstrology3 font's private-use area
    // (U+F000 / U+F010); built via ConvertFromUtf32 so the raw codepoints survive round-tripping.
    private static readonly string ParallelGlyph = char.ConvertFromUtf32(0xF000);
    private static readonly string ContraParallelGlyph = char.ConvertFromUtf32(0xF010);

    private readonly IRosetta _rosetta;
    private readonly SynastryModel _synastryModel;
    private readonly IConfigContext _configContext;

    private List<FoundParallel> _foundParallels = [];

    public SynastryDeclinationComparisonViewModel(IRosetta rosetta, SynastryModel synastryModel, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _synastryModel = synastryModel;
        _configContext = configContext;

        Rebuild();
    }

    public bool HasTwoCharts => _synastryModel.HasExactlyTwo;
    public bool HasMatches   => _foundParallels.Count > 0;

    public string ChartAName { get; private set; } = string.Empty;
    public string ChartBName { get; private set; } = string.Empty;

    public List<SynastryDeclinationRow> RowsFromA { get; private set; } = [];
    public List<SynastryDeclinationRow> RowsFromB { get; private set; } = [];

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle     => T("view.synastry.results.title.declinationcomparison");
    public string LabelNoMatches => T("view.synastry.declinationcomparison.nomatches");
    public string LabelColOrb    => T("view.synastry.declinationcomparison.colorb");
    public string LabelHelp      => T("view.synastry.help.declinationcomparison");
    public string LabelExportPdf => T("view.synastry.exportpdf");

    private void Rebuild()
    {
        if (!_synastryModel.HasExactlyTwo)
        {
            _foundParallels = [];
            RowsFromA = [];
            RowsFromB = [];
            NotifyAll();
            return;
        }

        var chartA = _synastryModel.ChartA!;
        var chartB = _synastryModel.ChartB!;
        ChartAName = chartA.Name;
        ChartBName = chartB.Name;

        var config = _configContext.ActiveConfig;
        _foundParallels = SynastryDeclinationOrchestrator.Calculate(
            chartA.Chart, chartB.Chart, config.FactorConfig, config.OrbConfig.ParallelOrb);

        RowsFromA = _foundParallels.Select((p, i) => new SynastryDeclinationRow(
            GlyphSelector.GetGlyphForFactor(p.Factor1),
            p.IsContraParallel ? ContraParallelGlyph : ParallelGlyph,
            GlyphSelector.GetGlyphForFactor(p.Factor2),
            OrbText(p.Orb), i)).ToList();

        RowsFromB = _foundParallels.Select((p, i) => new SynastryDeclinationRow(
            GlyphSelector.GetGlyphForFactor(p.Factor2),
            p.IsContraParallel ? ContraParallelGlyph : ParallelGlyph,
            GlyphSelector.GetGlyphForFactor(p.Factor1),
            OrbText(p.Orb), i)).ToList();

        NotifyAll();
    }

    private static string OrbText(double orb)
    {
        var totalMin = (int)(orb * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
    }

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(HasTwoCharts));
        OnPropertyChanged(nameof(HasMatches));
        OnPropertyChanged(nameof(ChartAName));
        OnPropertyChanged(nameof(ChartBName));
        OnPropertyChanged(nameof(RowsFromA));
        OnPropertyChanged(nameof(RowsFromB));
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
