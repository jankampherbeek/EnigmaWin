// CountingsViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Countings.UI;

/// <summary>ViewModel for the Countings screen: element/cross counts (by zodiac sign) for the factors
/// currently active in the configuration. Unlike the BLA schema (fixed point set), this always follows
/// the app's regular FactorConfig on the already-computed chart — no separate recompute needed.</summary>
public sealed class CountingsViewModel : INotifyPropertyChanged
{
    public sealed record CountingsRow(CountingsGroup Group, string Name, int Count, bool IsEvenRow);

    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;

    private bool _hasData;
    private string _chartName = "";

    public CountingsViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta = rosetta;
        _configContext = configContext;
    }

    public bool HasData { get => _hasData; private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;
    public string ChartName { get => _chartName; private set { if (_chartName == value) return; _chartName = value; OnPropertyChanged(); OnPropertyChanged(nameof(LabelHeaderTitle)); } }

    public ObservableCollection<CountingsRow> ElementsCounts { get; } = [];
    public ObservableCollection<CountingsRow> CrossesCounts { get; } = [];

    public void LoadChart(NamedChart? namedChart)
    {
        ElementsCounts.Clear();
        CrossesCounts.Clear();

        if (namedChart == null) { ChartName = ""; HasData = false; return; }

        ChartName = namedChart.Name;

        try
        {
            var factorConfig = _configContext.ActiveConfig.FactorConfig;
            var (elements, crosses) = CountingsOrchestrator.ElementsAndCrosses(namedChart.Chart, factorConfig);

            var i = 0;
            foreach (var line in elements)
                ElementsCounts.Add(new CountingsRow(line.Group, GroupName(line.Group), line.Count, i++ % 2 == 0));

            i = 0;
            foreach (var line in crosses)
                CrossesCounts.Add(new CountingsRow(line.Group, GroupName(line.Group), line.Count, i++ % 2 == 0));

            HasData = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Countings calculation failed.");
            HasData = false;
        }
    }

    private string GroupName(CountingsGroup group) => group switch
    {
        CountingsGroup.Cardinal => _rosetta.GetText(RbFile.RadixCountings, "countings.group.cardinal"),
        CountingsGroup.Fixed => _rosetta.GetText(RbFile.RadixCountings, "countings.group.fixed"),
        CountingsGroup.Mutable => _rosetta.GetText(RbFile.RadixCountings, "countings.group.mutable"),
        CountingsGroup.Fire => _rosetta.GetText(RbFile.RadixCountings, "countings.group.fire"),
        CountingsGroup.Earth => _rosetta.GetText(RbFile.RadixCountings, "countings.group.earth"),
        CountingsGroup.Air => _rosetta.GetText(RbFile.RadixCountings, "countings.group.air"),
        CountingsGroup.Water => _rosetta.GetText(RbFile.RadixCountings, "countings.group.water"),
        _ => ""
    };

    // ── Labels ──────────────────────────────────────────────────────────────

    public string LabelTitle => _rosetta.GetText(RbFile.RadixCountings, "countings.title");
    public string LabelNoChart => _rosetta.GetText(RbFile.RadixCountings, "countings.nochart");
    public string LabelHeaderTitle => string.Format(_rosetta.GetText(RbFile.RadixCountings, "countings.headertitle.format"), ChartName);
    public string TooltipHelp => _rosetta.GetText(RbFile.RadixCountings, "countings.help.tooltip");
    public string HelpText => _rosetta.GetText(RbFile.RadixCountings, "countings.help");
    public string LabelHelpClose => _rosetta.GetText(RbFile.RadixCountings, "countings.help.close");

    public string LabelElementsTitle => _rosetta.GetText(RbFile.RadixCountings, "countings.elements.title");
    public string LabelCrossesTitle => _rosetta.GetText(RbFile.RadixCountings, "countings.crosses.title");
    public string LabelElementsChartTitle => _rosetta.GetText(RbFile.RadixCountings, "countings.elements.chart.title");
    public string LabelCrossesChartTitle => _rosetta.GetText(RbFile.RadixCountings, "countings.crosses.chart.title");
    public string LabelColCount => _rosetta.GetText(RbFile.RadixCountings, "countings.col.count");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
