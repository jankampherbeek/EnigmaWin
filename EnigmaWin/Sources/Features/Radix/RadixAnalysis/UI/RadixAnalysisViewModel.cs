// RadixAnalysisViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.UI;

/// <summary>Grouped overview of all analyses, shown in the center pane. Grouping is defined in AnalysisCatalog.</summary>
public sealed class RadixAnalysisViewModel
{
    public string LabelTitle { get; }
    public IReadOnlyList<AnalysisGroupEntry> Groups { get; }

    public RadixAnalysisViewModel(INavigationService navigationService, IRosetta rosetta)
    {
        string T(string key) => rosetta.GetText(RbFile.RadixAnalysis, key);

        LabelTitle = T("analysis.title");
        Groups = AnalysisCatalog.Groups
            .Select(group => new AnalysisGroupEntry(
                T(AnalysisCatalog.RbKey(group)),
                AnalysisCatalog.ItemsOf(group)
                    .Select(item => new AnalysisItemEntry(
                        T(AnalysisCatalog.RbKey(item)),
                        new RelayCommand(() => AnalysisCatalog.Open(navigationService, item))))
                    .ToList()))
            .ToList();
    }
}

public sealed record AnalysisGroupEntry(string Title, IReadOnlyList<AnalysisItemEntry> Items);

public sealed record AnalysisItemEntry(string Label, IRelayCommand OpenCommand);
