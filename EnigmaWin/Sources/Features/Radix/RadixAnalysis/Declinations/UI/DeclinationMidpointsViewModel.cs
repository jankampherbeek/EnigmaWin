// DeclinationMidpointsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclinationMidpointsViewModel : INotifyPropertyChanged
{
    private readonly IRosetta      _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;

    public sealed record MidpointRow(
        string Glyph1,
        string Glyph2,
        string MidpointText,
        string OccupantGlyph,
        string OccDeclText,
        string OrbText,
        string ExactnessText,
        bool   IsEvenRow);

    public IReadOnlyList<DeclArcItem>          ArcItems { get; private set; } = [];
    public ObservableCollection<MidpointRow>   Rows     { get; }               = [];
    public double                               OrbDeg   { get; private set; }  = 0.75;

    public DeclinationMidpointsViewModel(IRosetta rosetta, IConfigContext configContext)
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

    // ── Localized labels ──────────────────────────────────────────────────────

    public string LabelTitle     => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.title");
    public string LabelNorth     => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.north");
    public string LabelSouth     => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.south");
    public string LabelEmpty     => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.nodata");
    public string LabelF1        => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.f1");
    public string LabelF2        => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.f2");
    public string LabelMidpoint  => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.midpoint");
    public string LabelOccupant  => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.occupant");
    public string LabelOccDecl   => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.occdecl");
    public string LabelOrb       => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.orb");
    public string LabelExactness => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.midpoints.col.exactness");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.factsheet.tooltip");
    public string TooltipHelp    => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.help.tooltip");

    // ── Chart loading ─────────────────────────────────────────────────────────

    public void LoadChart(FullChart? chart)
    {
        Rows.Clear();
        ArcItems = [];
        HasData  = false;

        if (chart is null)
        {
            OnPropertyChanged(nameof(ArcItems));
            return;
        }

        var config = _configContext.ActiveConfig;
        OrbDeg     = config.OrbConfig.DeclinationMidpointOrb;

        // Build arc items (one per active factor with a declination)
        var rawArc = new List<DeclArcItem>();
        foreach (var setting in config.FactorConfig.Settings)
        {
            if (!setting.IsUsed) continue;
            var decl = GetDeclination(setting.Factor, chart);
            if (decl is null) continue;
            rawArc.Add(new DeclArcItem(
                Factor:      setting.Factor,
                Glyph:       GlyphSelector.GetGlyphForFactor(setting.Factor),
                Declination: decl.Value,
                VisualAngle: WheelGeometry.Normalise(-decl.Value * 4.0)));
        }
        ArcItems = DrawDeclArc.ResolveOverlaps(rawArc);
        OnPropertyChanged(nameof(ArcItems));
        OnPropertyChanged(nameof(OrbDeg));

        // Build table rows
        var occupied = DeclinationsOrchestrator.OccupiedMidpoints(chart, config.FactorConfig, config.OrbConfig);
        var rowIndex = 0;
        foreach (var item in occupied)
        {
            Rows.Add(new MidpointRow(
                Glyph1:        GlyphSelector.GetGlyphForFactor(item.Midpoint.Factor1),
                Glyph2:        GlyphSelector.GetGlyphForFactor(item.Midpoint.Factor2),
                MidpointText:  PositionInDegreesConversion.DoubleToDms(item.Midpoint.Position),
                OccupantGlyph: GlyphSelector.GetGlyphForFactor(item.OccupyingFactor),
                OccDeclText:   PositionInDegreesConversion.DoubleToDms(item.OccupyingPosition),
                OrbText:       FormatOrb(item.ActualOrb),
                ExactnessText: $"{(int)Math.Round(item.Exactness)}%",
                IsEvenRow:     rowIndex++ % 2 == 0));
        }

        HasData = Rows.Count > 0;
        OnPropertyChanged(nameof(Rows));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static double? GetDeclination(Factors factor, FullChart chart)
        => DeclMidpointsCalculator.GetDeclination(factor, chart);

    private static string FormatOrb(double orb)
    {
        var total = (int)(Math.Abs(orb) * 3600);
        var deg   = total / 3600;
        var min   = total % 3600 / 60;
        var sec   = total % 60;
        return $"{deg}°{min:D2}'{sec:D2}\"";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
