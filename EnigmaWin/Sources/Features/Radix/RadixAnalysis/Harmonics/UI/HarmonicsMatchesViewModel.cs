// HarmonicsMatchesViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public sealed class HarmonicsMatchesViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;

    public sealed record MatchRow(
        string RadixGlyph,
        string HarmonicGlyph,
        string OrbText,
        string ExactnessText);

    public ObservableCollection<MatchRow> MatchRows { get; } = [];

    public HarmonicsMatchesViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
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

    public string LabelTitle          => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.title.matches");
    public string TooltipFactsheet    => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.factsheet.tooltip");
    public string TooltipHelp         => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.tooltip");
    public string LabelRadixFactor    => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.radixfactor");
    public string LabelHarmonicFactor => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.harmonicfactor");
    public string LabelOrb            => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.orb");
    public string LabelExactness      => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.col.exactness");
    public string LabelEmpty          => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.matches.nodata");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Load(FullChart? chart, double harmonic)
    {
        MatchRows.Clear();

        if (chart == null)
        {
            HasData = false;
            return;
        }

        var matches = HarmonicsOrchestrator.Matches(chart, _configContext.ActiveConfig, harmonic);

        foreach (var m in matches)
        {
            var radixGlyph   = GlyphSelector.GetGlyphForFactor(m.RadixFactor);
            var harmonicGlyph = GlyphSelector.GetGlyphForFactor(m.HarmonicFactor);
            var orbText      = OrbText(m.ActualOrb);
            var exactness    = m.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - m.ActualOrb / m.MaxOrb) * 100)))
                : 100;

            MatchRows.Add(new MatchRow(radixGlyph, harmonicGlyph, orbText, $"{exactness}%"));
        }

        HasData = MatchRows.Count > 0;
        OnPropertyChanged(nameof(MatchRows));
    }

    private static string OrbText(double orb)
    {
        var totalMin = (int)(Math.Abs(orb) * 60);
        return $"{totalMin / 60}°{totalMin % 60:D2}'";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
