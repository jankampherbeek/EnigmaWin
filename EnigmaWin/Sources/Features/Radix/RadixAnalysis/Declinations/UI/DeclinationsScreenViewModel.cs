// DeclinationsScreenViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclinationsScreenViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;

    public AllDeclinationsViewModel           AllDeclinationsViewModel { get; }
    public DeclinationParallelsViewModel      ParallelsViewModel       { get; }
    public DeclinationLongEquivalentsViewModel EquivalentsViewModel    { get; }
    public DeclinationDiagramViewModel        DiagramViewModel         { get; }
    public DeclinationMidpointsViewModel      MidpointsViewModel       { get; }

    public IRelayCommand ShowAllDeclinationsCommand { get; }
    public IRelayCommand ShowParallelsCommand       { get; }
    public IRelayCommand ShowEquivalentsCommand     { get; }
    public IRelayCommand ShowDiagramCommand         { get; }
    public IRelayCommand ShowMidpointsCommand       { get; }

    public DeclinationsScreenViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta = rosetta;

        AllDeclinationsViewModel = new AllDeclinationsViewModel(rosetta, configContext);
        ParallelsViewModel       = new DeclinationParallelsViewModel(rosetta, configContext);
        EquivalentsViewModel     = new DeclinationLongEquivalentsViewModel(rosetta, configContext);
        DiagramViewModel         = new DeclinationDiagramViewModel(rosetta);
        MidpointsViewModel       = new DeclinationMidpointsViewModel(rosetta);

        ShowAllDeclinationsCommand = new RelayCommand(() => ActiveTab = Tab.AllDeclinations);
        ShowParallelsCommand       = new RelayCommand(() => ActiveTab = Tab.Parallels);
        ShowEquivalentsCommand     = new RelayCommand(() => ActiveTab = Tab.Equivalents);
        ShowDiagramCommand         = new RelayCommand(() => ActiveTab = Tab.Diagram);
        ShowMidpointsCommand       = new RelayCommand(() => ActiveTab = Tab.Midpoints);
    }

    // --- Chart loading ---

    public void LoadChart(FullChart? chart)
    {
        AllDeclinationsViewModel.LoadChart(chart);
        ParallelsViewModel.LoadChart(chart);
        EquivalentsViewModel.LoadChart(chart);
    }

    // --- Tab switching ---

    private enum Tab { AllDeclinations, Parallels, Equivalents, Diagram, Midpoints }

    private Tab _activeTab = Tab.AllDeclinations;
    private Tab ActiveTab
    {
        set
        {
            if (_activeTab == value) return;
            _activeTab = value;
            OnPropertyChanged(nameof(ShowAllDeclinations));
            OnPropertyChanged(nameof(ShowParallels));
            OnPropertyChanged(nameof(ShowEquivalents));
            OnPropertyChanged(nameof(ShowDiagram));
            OnPropertyChanged(nameof(ShowMidpoints));
        }
    }

    public bool ShowAllDeclinations => _activeTab == Tab.AllDeclinations;
    public bool ShowParallels       => _activeTab == Tab.Parallels;
    public bool ShowEquivalents     => _activeTab == Tab.Equivalents;
    public bool ShowDiagram         => _activeTab == Tab.Diagram;
    public bool ShowMidpoints       => _activeTab == Tab.Midpoints;

    // --- Localized labels ---

    public string LabelBtnAllDeclinations => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.all");
    public string LabelBtnParallels       => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.parallels");
    public string LabelBtnEquivalents     => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.equivalents");
    public string LabelBtnDiagram         => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.diagram");
    public string LabelBtnMidpoints       => _rosetta.GetText(RbFile.RadixDeclinations, "declinations.btn.midpoints");

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
