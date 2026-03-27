// RadixAspectsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;

public sealed class RadixAspectsViewModel : INotifyPropertyChanged
{
    private readonly RadixAspectsModel _model = new();
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _hasData;

    public RadixAspectsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta = rosetta;
        _configContext = configContext;
    }

    public ObservableCollection<RadixAspectsModel.AspectRow> AspectRows { get; } = [];

    /// <summary>Raised after LoadChart so the code-behind can update the aspect grid.</summary>
    public event Action<List<Factors>, List<AspectGridControl.AspectCell>>? GridDataReady;

    public bool HasData
    {
        get => _hasData;
        private set
        {
            if (_hasData == value) return;
            _hasData = value;
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public bool HasNoData => !HasData;

    // Localized labels
    public string LabelTitle        => _rosetta.GetText(RbFile.RadixAspects, "aspects.title");
    public string LabelGridTitle    => _rosetta.GetText(RbFile.RadixAspects, "aspects.grid.title");
    public string LabelListTitle    => _rosetta.GetText(RbFile.RadixAspects, "aspects.list.title");
    public string LabelFactor1      => _rosetta.GetText(RbFile.RadixAspects, "aspects.header.factor1");
    public string LabelAspect       => _rosetta.GetText(RbFile.RadixAspects, "aspects.header.aspect");
    public string LabelFactor2      => _rosetta.GetText(RbFile.RadixAspects, "aspects.header.factor2");
    public string LabelOrb          => _rosetta.GetText(RbFile.RadixAspects, "aspects.header.orb");
    public string LabelExactness    => _rosetta.GetText(RbFile.RadixAspects, "aspects.header.exactness");
    public string LabelEmpty        => _rosetta.GetText(RbFile.RadixAspects, "aspects.empty");
    public string TooltipHelp       => _rosetta.GetText(RbFile.RadixAspects, "aspects.help.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadChart(FullChart? chart)
    {
        AspectRows.Clear();

        if (chart == null)
        {
            HasData = false;
            GridDataReady?.Invoke([], []);
            return;
        }

        var config = _configContext.ActiveConfig;
        var aspects = AspectsOrchestrator.Calculate(
            chart,
            config.FactorConfig,
            config.AspectConfig,
            config.OrbConfig);

        // List rows
        var rows = _model.BuildRows(aspects, _rosetta);
        foreach (var row in rows)
            AspectRows.Add(row);

        HasData = AspectRows.Count > 0;
        OnPropertyChanged(nameof(AspectRows));

        // Grid data
        var factors = BuildActiveFactors(chart, config.FactorConfig);
        var cells   = BuildGridCells(aspects, config.AspectConfig);
        GridDataReady?.Invoke(factors, cells);
    }

    private static List<Factors> BuildActiveFactors(FullChart chart, FactorConfig factorConfig)
    {
        var result = new List<Factors>();
        foreach (var setting in factorConfig.Settings)
        {
            if (!setting.IsUsed) continue;

            if (setting.Factor.CalculationType() == CalculationTypes.Mundane)
            {
                result.Add(setting.Factor);
            }
            else if (chart.Coordinates.TryGetValue(setting.Factor, out var pos)
                     && pos.Ecliptical.Length > 0)
            {
                result.Add(setting.Factor);
            }
        }
        return result;
    }

    private static List<AspectGridControl.AspectCell> BuildGridCells(
        IReadOnlyList<FoundAspect> aspects,
        AspectConfig aspectConfig)
    {
        var colorMap = aspectConfig.Settings
            .ToDictionary(s => s.Aspect, s => s.Color);

        var cells = new List<AspectGridControl.AspectCell>();
        foreach (var found in aspects)
        {
            var lo = Math.Min((int)found.Factor1, (int)found.Factor2);
            var hi = Math.Max((int)found.Factor1, (int)found.Factor2);

            var colorCfg = colorMap.TryGetValue(found.Aspect, out var cc)
                ? cc
                : new ColorConfig(0.5, 0.5, 0.5);

            var avaloniaColor = Color.FromArgb(
                (byte)(colorCfg.Opacity * 255),
                (byte)(colorCfg.Red   * 255),
                (byte)(colorCfg.Green * 255),
                (byte)(colorCfg.Blue  * 255));

            cells.Add(new AspectGridControl.AspectCell(lo, hi,
                EnigmaWin.Sources.Features.Shared.Glyphs.GlyphSelector.GetGlyphForAspect(found.Aspect),
                avaloniaColor));
        }
        return cells;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
