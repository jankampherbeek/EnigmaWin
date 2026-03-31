// OccupiedMidpointsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

public sealed class OccupiedMidpointsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;
    private IReadOnlyList<MidpointMatch> _matches = [];

    public sealed record MatchRow(
        string Glyph1,
        string Glyph2,
        string MidpointDms,
        string MidpointSignGlyph,
        string PlanetGlyph,
        string OrbText,
        string ExactnessText);

    public ObservableCollection<MatchRow> MatchRows { get; } = [];

    /// <summary>Raw matches for the canvas diagram.</summary>
    public IReadOnlyList<MidpointMatch> Matches
    {
        get => _matches;
        private set
        {
            _matches = value;
            OnPropertyChanged();
        }
    }

    public OccupiedMidpointsViewModel(IRosetta rosetta, IConfigContext configContext)
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

    public string LabelTitle     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.occupied.title");
    public string LabelFactor1   => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor1");
    public string LabelFactor2   => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.factor2");
    public string LabelMidpoint  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.midpoint");
    public string LabelPlanet    => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.planet");
    public string LabelOrb       => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.orb");
    public string LabelExactness => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.col.exactness");
    public string LabelEmpty     => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.occupied.nomidpoints");
    public string TooltipHelp       => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.help.tooltip");
    public string TooltipFactsheet  => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.factsheet.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadChart(FullChart? chart, MidpointDialType dialType)
    {
        MatchRows.Clear();

        if (chart == null)
        {
            Matches = [];
            HasData = false;
            return;
        }

        var config = _configContext.ActiveConfig;
        var rawMatches = MidpointsOrchestrator.Matches(chart, config, dialType);
        Matches = rawMatches;

        foreach (var m in rawMatches)
        {
            var glyph1 = GlyphSelector.GetGlyphForFactor(m.Factor1);
            var glyph2 = GlyphSelector.GetGlyphForFactor(m.Factor2);
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(m.MidpointPosition);
            var signGlyph = ok && sign.HasValue ? GlyphSelector.GetGlyphForSign(sign.Value) : "";
            var planetGlyph = GlyphSelector.GetGlyphForFactor(m.MatchingFactor);
            var orbText = OrbText(m.ActualOrb);
            var exactness = m.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - m.ActualOrb / m.MaxOrb) * 100)))
                : 100;

            MatchRows.Add(new MatchRow(glyph1, glyph2, dms, signGlyph, planetGlyph, orbText, $"{exactness}%"));
        }

        HasData = MatchRows.Count > 0;
        OnPropertyChanged(nameof(MatchRows));
    }

    private static string OrbText(double orb)
    {
        var totalMin = (int)(Math.Abs(orb) * 60);
        return $"{totalMin / 60}°{(totalMin % 60):D2}'";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
