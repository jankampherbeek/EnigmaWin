// LotsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Lots.UI;

/// <summary>Shared state for the Lots feature (a DI singleton): the input screen edits the "use sect"
/// toggle; the results screen observes the same instance to rebuild the table of the seven classic
/// Hellenistic lots.</summary>
public sealed class LotsViewModel : INotifyPropertyChanged
{
    public sealed record LotRow(string Name, string PositionText, string SignGlyph, bool IsEvenRow);

    private readonly IRosetta _rosetta;
    private readonly IChartSession _chartSession;
    private NamedChart? _currentChart;
    private bool _useSect = true;
    private bool _hasData;

    public LotsViewModel(IRosetta rosetta, IChartSession chartSession)
    {
        _rosetta = rosetta;
        _chartSession = chartSession;
        _currentChart = chartSession.Selected;

        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IChartSession.SelectedChart)) return;
                _currentChart = _chartSession.Selected;
                OnPropertyChanged(nameof(ChartName));
                OnPropertyChanged(nameof(LabelTitle));
                OnPropertyChanged(nameof(IsNightChart));
                Recalculate();
            };

        Recalculate();
    }

    public bool UseSect
    {
        get => _useSect;
        set
        {
            if (_useSect == value) return;
            _useSect = value;
            OnPropertyChanged();
            Recalculate();
        }
    }

    /// <summary>Sect only matters for a night chart; for a day chart the toggle has no effect and is
    /// disabled in the input screen.</summary>
    public bool IsNightChart => _currentChart is not null && LotsOrchestrator.IsNightChart(_currentChart.Chart);

    public string ChartName => _currentChart?.Name ?? "";
    public bool HasData { get => _hasData; private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;

    public ObservableCollection<LotRow> Rows { get; } = [];

    private void Recalculate()
    {
        Rows.Clear();

        if (_currentChart is null)
        {
            HasData = false;
            return;
        }

        try
        {
            var results = LotsOrchestrator.Calculate(_currentChart.Chart, UseSect);

            var i = 0;
            foreach (var lot in results)
            {
                var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(lot.Longitude);
                var positionText = ok ? dms : "—";
                var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";
                Rows.Add(new LotRow(NameFor(lot.Type), positionText, signGlyph, i++ % 2 == 0));
            }

            HasData = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Lots calculation failed.");
            HasData = false;
        }
    }

    private string NameFor(LotType type) => type switch
    {
        LotType.Fortune => _rosetta.GetText(RbFile.RadixLots, "lots.name.fortune"),
        LotType.Spirit => _rosetta.GetText(RbFile.RadixLots, "lots.name.spirit"),
        LotType.Eros => _rosetta.GetText(RbFile.RadixLots, "lots.name.eros"),
        LotType.Victory => _rosetta.GetText(RbFile.RadixLots, "lots.name.victory"),
        LotType.Necessity => _rosetta.GetText(RbFile.RadixLots, "lots.name.necessity"),
        LotType.Courage => _rosetta.GetText(RbFile.RadixLots, "lots.name.courage"),
        LotType.Nemesis => _rosetta.GetText(RbFile.RadixLots, "lots.name.nemesis"),
        _ => ""
    };

    // ── Labels ──────────────────────────────────────────────────────────────

    public string LabelTitle => string.Format(_rosetta.GetText(RbFile.RadixLots, "lots.title"), ChartName);
    public string LabelNoChart => _rosetta.GetText(RbFile.RadixLots, "lots.nochart");
    public string LabelUseSect => _rosetta.GetText(RbFile.RadixLots, "lots.usesect");
    public string LabelColName => _rosetta.GetText(RbFile.RadixLots, "lots.col.name");
    public string LabelColPosition => _rosetta.GetText(RbFile.RadixLots, "lots.col.position");
    public string TooltipHelp => _rosetta.GetText(RbFile.RadixLots, "lots.help.tooltip");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixLots, "lots.factsheet.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
