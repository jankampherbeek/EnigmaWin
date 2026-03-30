// MidpointsScreenViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

public sealed class MidpointsScreenViewModel : INotifyPropertyChanged
{
    private readonly IRosetta _rosetta;
    private readonly IConfigContext _configContext;
    private bool _showAllMidpoints = true;
    private MidpointDialType _dialType;

    public AllMidpointsViewModel AllMidpointsViewModel { get; }
    public OccupiedMidpointsViewModel OccupiedMidpointsViewModel { get; }

    public IRelayCommand ShowAllMidpointsCommand      { get; }
    public IRelayCommand ShowOccupiedMidpointsCommand { get; }
    public IRelayCommand SelectDial360Command         { get; }
    public IRelayCommand SelectDial90Command          { get; }
    public IRelayCommand SelectDial45Command          { get; }

    public MidpointsScreenViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        // Derive initial dial from DisplayConfig drawing type
        _dialType = configContext.ActiveConfig.DisplayConfig.DrawingType switch
        {
            DrawingTypes.Dial90 => MidpointDialType.Dial90,
            DrawingTypes.Dial45 => MidpointDialType.Dial45,
            _                   => MidpointDialType.Dial360
        };

        AllMidpointsViewModel      = new AllMidpointsViewModel(rosetta, configContext);
        OccupiedMidpointsViewModel = new OccupiedMidpointsViewModel(rosetta, configContext);

        ShowAllMidpointsCommand      = new RelayCommand(() => ShowAllMidpoints = true);
        ShowOccupiedMidpointsCommand = new RelayCommand(() => ShowAllMidpoints = false);
        SelectDial360Command         = new RelayCommand(() => SetDial(MidpointDialType.Dial360));
        SelectDial90Command          = new RelayCommand(() => SetDial(MidpointDialType.Dial90));
        SelectDial45Command          = new RelayCommand(() => SetDial(MidpointDialType.Dial45));
    }

    public bool ShowAllMidpoints
    {
        get => _showAllMidpoints;
        private set
        {
            if (_showAllMidpoints == value) return;
            _showAllMidpoints = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowOccupiedMidpoints));
            OnPropertyChanged(nameof(ShowDialButtons));
        }
    }

    public bool ShowOccupiedMidpoints => !ShowAllMidpoints;
    public bool ShowDialButtons       => !ShowAllMidpoints;

    // Localized labels
    public string LabelBtnAllMidpoints      => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.btn.allmidpoints");
    public string LabelBtnOccupiedMidpoints => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.btn.occupiedmidpoints");
    public string LabelNoChart              => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.nochart");
    public string LabelDial360              => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.360");
    public string LabelDial90               => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.90");
    public string LabelDial45               => _rosetta.GetText(RbFile.RadixMidpoints, "midpoints.dial.45");

    private FullChart? _currentChart;

    public void LoadChart(FullChart? chart)
    {
        _currentChart = chart;
        AllMidpointsViewModel.LoadChart(chart);
        OccupiedMidpointsViewModel.LoadChart(chart, _dialType);
    }

    private void SetDial(MidpointDialType dial)
    {
        if (_dialType == dial) return;
        _dialType = dial;
        OccupiedMidpointsViewModel.LoadChart(_currentChart, _dialType);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
