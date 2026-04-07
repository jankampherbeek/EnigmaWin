// DeclinationParallelsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclinationParallelsViewModel : INotifyPropertyChanged
{
    // Unicode glyphs for parallel (F000) and contra-parallel (F010) in the EnigmaAstrology2 font
    private const string ParallelGlyph      = "\uF000";
    private const string ContraParallelGlyph = "\uF010";

    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;

    public sealed record ParallelRow(
        string Glyph1,
        string Decl1,
        string TypeGlyph,
        string Glyph2,
        string Decl2,
        string OrbText,
        string ExactnessText,
        bool IsEvenRow);

    public ObservableCollection<ParallelRow> Rows { get; } = [];

    public DeclinationParallelsViewModel(IRosetta rosetta, IConfigContext configContext)
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

    public string LabelTitle      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.title");
    public string LabelFactor1    => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.factor1");
    public string LabelDecl1      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.decl1");
    public string LabelType       => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.type");
    public string LabelFactor2    => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.factor2");
    public string LabelDecl2      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.decl2");
    public string LabelOrb        => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.orb");
    public string LabelExactness  => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.col.exactness");
    public string LabelEmpty      => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.parallels.nodata");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.factsheet.tooltip");
    public string TooltipHelp     => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.help.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadChart(FullChart? chart)
    {
        Rows.Clear();

        if (chart is null)
        {
            HasData = false;
            return;
        }

        var config    = _configContext.ActiveConfig;
        var parallels = DeclinationsOrchestrator.Parallels(chart, config.FactorConfig, config.OrbConfig);

        var rowIndex = 0;
        foreach (var p in parallels)
        {
            var glyph1    = GlyphSelector.GetGlyphForFactor(p.Factor1);
            var glyph2    = GlyphSelector.GetGlyphForFactor(p.Factor2);
            var typeGlyph = p.IsContraParallel ? ContraParallelGlyph : ParallelGlyph;
            var decl1     = PositionInDegreesConversion.DoubleToDms(p.Factor1Declination);
            var decl2     = PositionInDegreesConversion.DoubleToDms(p.Factor2Declination);
            var orbText   = FormatOrb(p.ActualOrb);
            var exactness = p.MaxOrb > 0
                ? Math.Max(0, Math.Min(100, (int)((1.0 - p.ActualOrb / p.MaxOrb) * 100)))
                : 100;

            Rows.Add(new ParallelRow(glyph1, decl1, typeGlyph, glyph2, decl2,
                orbText, $"{exactness}%", rowIndex++ % 2 == 0));
        }

        HasData = Rows.Count > 0;
        OnPropertyChanged(nameof(Rows));
    }

    private static string FormatOrb(double orb)
    {
        var totalSeconds = (int)(Math.Abs(orb) * 3600);
        var deg = totalSeconds / 3600;
        var min = totalSeconds % 3600 / 60;
        var sec = totalSeconds % 60;
        return $"{deg}°{min:D2}'{sec:D2}\"";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
