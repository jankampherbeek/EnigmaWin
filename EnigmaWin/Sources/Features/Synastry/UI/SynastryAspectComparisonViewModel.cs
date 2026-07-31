// SynastryAspectComparisonViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Cross-chart aspects between two selected charts, shown as two directional tables
/// (same underlying aspects, ordered from each person's perspective).</summary>
public sealed class SynastryAspectComparisonViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly SynastryModel _synastryModel;
    private readonly IConfigContext _configContext;

    private List<FoundAspect> _foundAspects = [];

    public SynastryAspectComparisonViewModel(IRosetta rosetta, SynastryModel synastryModel, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _synastryModel = synastryModel;
        _configContext = configContext;

        Rebuild();
    }

    public bool HasTwoCharts => _synastryModel.HasExactlyTwo;
    public bool HasAspects   => _foundAspects.Count > 0;

    public string ChartAName { get; private set; } = string.Empty;
    public string ChartBName { get; private set; } = string.Empty;

    public List<SynastryAspectRow> RowsFromA { get; private set; } = [];
    public List<SynastryAspectRow> RowsFromB { get; private set; } = [];

    public IReadOnlyList<FoundAspect> FoundAspects => _foundAspects;

    // ── Labels ────────────────────────────────────────────────────────────────

    public string LabelTitle    => T("view.synastry.results.title.aspectcomparison");
    public string LabelNoAspects => T("view.synastry.aspectcomparison.noaspects");
    public string LabelColOrb   => T("view.synastry.aspectcomparison.colorb");
    public string LabelHelp     => T("view.synastry.help.aspectcomparison");
    public string LabelExportPdf => T("view.synastry.exportpdf");

    private void Rebuild()
    {
        if (!_synastryModel.HasExactlyTwo)
        {
            _foundAspects = [];
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
        _foundAspects = SynastryAspectsOrchestrator.Calculate(
            chartA.Chart, chartB.Chart, config.FactorConfig, config.AspectConfig, config.OrbConfig);

        RowsFromA = _foundAspects.Select((f, i) => new SynastryAspectRow(
            GlyphSelector.GetGlyphForFactor(f.Factor1),
            GlyphSelector.GetGlyphForAspect(f.Aspect),
            GlyphSelector.GetGlyphForFactor(f.Factor2),
            OrbText(f.Orb), i)).ToList();

        RowsFromB = _foundAspects.Select((f, i) => new SynastryAspectRow(
            GlyphSelector.GetGlyphForFactor(f.Factor2),
            GlyphSelector.GetGlyphForAspect(f.Aspect),
            GlyphSelector.GetGlyphForFactor(f.Factor1),
            OrbText(f.Orb), i)).ToList();

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
        OnPropertyChanged(nameof(HasAspects));
        OnPropertyChanged(nameof(ChartAName));
        OnPropertyChanged(nameof(ChartBName));
        OnPropertyChanged(nameof(RowsFromA));
        OnPropertyChanged(nameof(RowsFromB));
        OnPropertyChanged(nameof(FoundAspects));
    }

    private string T(string key) => _rosetta.GetText(RbFile.Synastry, key);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
