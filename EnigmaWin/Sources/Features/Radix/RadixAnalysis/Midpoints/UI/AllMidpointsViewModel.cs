// AllMidpointsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

public sealed class AllMidpointsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;

    public record MidpointRow(string Glyph1, string Glyph2, string PositionDms, string SignGlyph, bool IsEvenRow);

    public ObservableCollection<MidpointRow> MidpointRows { get; } = [];
    public ObservableCollection<MidpointRow> LeftRows { get; } = [];
    public ObservableCollection<MidpointRow> RightRows { get; } = [];

    public AllMidpointsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta = rosetta;
        _configContext = configContext;
    }

    public bool HasData
    {
        get => _hasData;
        private set
        {
            if (_hasData == value) return;
            _hasData = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public bool HasNoData => !HasData;

    public string LabelTitle     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.allmidpoints.title");
    public string LabelFactor1   => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor1");
    public string LabelFactor2   => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor2");
    public string LabelPosition  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.position");
    public string LabelEmpty     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.allmidpoints.nomidpoints");
    public string TooltipHelp       => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.help.tooltip");
    public string TooltipFactsheet  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.factsheet.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadChart(FullChart? chart)
    {
        MidpointRows.Clear();
        LeftRows.Clear();
        RightRows.Clear();

        if (chart == null)
        {
            HasData = false;
            return;
        }

        var config = _configContext.ActiveConfig;
        var midpoints = MidpointsOrchestrator.Calculate(chart, config);

        var rowIndex = 0;
        foreach (var mp in midpoints)
        {
            var glyph1 = GlyphSelector.GetGlyphForFactor(mp.Factor1);
            var glyph2 = GlyphSelector.GetGlyphForFactor(mp.Factor2);
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(mp.Position);
            var signGlyph = ok && sign.HasValue
                ? GlyphSelector.GetGlyphForSign(sign.Value)
                : "";
            MidpointRows.Add(new MidpointRow(glyph1, glyph2, dms, signGlyph, rowIndex++ % 2 == 0));
        }

        // Split evenly over two columns — each column gets its own even/odd index
        var half = (MidpointRows.Count + 1) / 2;
        for (var i = 0; i < MidpointRows.Count; i++)
        {
            var original = MidpointRows[i];
            if (i < half)
                LeftRows.Add(original with { IsEvenRow = i % 2 == 0 });
            else
                RightRows.Add(original with { IsEvenRow = (i - half) % 2 == 0 });
        }

        HasData = MidpointRows.Count > 0;
        OnPropertyChanged(nameof(MidpointRows));
        OnPropertyChanged(nameof(LeftRows));
        OnPropertyChanged(nameof(RightRows));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
