// HarmonicsDrawingViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public sealed class HarmonicsDrawingViewModel : INotifyPropertyChanged
{
    private readonly IRosetta       _rosetta;
    private readonly IConfigContext _configContext;
    private FullChart?              _currentChart;
    private double                  _harmonic = 2.0;
    private bool                    _isBlackWhite;
    private bool                    _hideAspects;
    private bool                    _hideAscMc;

    public WheelPlotData PlotData { get; private set; } = WheelPlotData.Empty;
    public WheelTheme    Theme    => _isBlackWhite ? WheelTheme.BlackWhite : WheelTheme.Color;

    public IRelayCommand ToggleBlackWhiteCommand { get; }
    public IRelayCommand ToggleAspectsCommand    { get; }
    public IRelayCommand ToggleAscMcCommand      { get; }

    public HarmonicsDrawingViewModel(IRosetta rosetta, IConfigContext configContext)
    {
        _rosetta       = rosetta;
        _configContext = configContext;

        ToggleBlackWhiteCommand = new RelayCommand(() =>
        {
            _isBlackWhite = !_isBlackWhite;
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(IsBlackWhite));
        });

        ToggleAspectsCommand = new RelayCommand(() =>
        {
            _hideAspects = !_hideAspects;
            OnPropertyChanged(nameof(ShowAspects));
            OnPropertyChanged(nameof(LabelBtnAspects));
        });

        ToggleAscMcCommand = new RelayCommand(() =>
        {
            _hideAscMc = !_hideAscMc;
            OnPropertyChanged(nameof(LabelBtnAscMc));
            Rebuild();
        });
    }

    public bool IsBlackWhite => _isBlackWhite;
    public bool ShowAspects  => !_hideAspects;

    public string LabelTitle         => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.title.drawing");
    public string TooltipFactsheet   => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.factsheet.tooltip");
    public string TooltipHelp        => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.tooltip");
    public string LabelBtnBlackWhite => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.blackwhite");
    public string LabelBtnExport     => _rosetta.GetText(RbFile.RadixHarmonics, "harmonics.btn.export");
    public string LabelBtnAspects    => _rosetta.GetText(RbFile.ChartWheel,
        _hideAspects ? ChartWheelKeys.ShowAspectsButton : ChartWheelKeys.NoAspectsButton);
    public string LabelBtnAscMc      => _rosetta.GetText(RbFile.RadixHarmonics,
        _hideAscMc ? "harmonics.btn.showascmc" : "harmonics.btn.hideascmc");

    public void Load(FullChart? chart, double harmonic)
    {
        _currentChart = chart;
        _harmonic     = harmonic;
        Rebuild();
    }

    private void Rebuild()
    {
        if (_currentChart is null)
        {
            PlotData = WheelPlotData.Empty;
            OnPropertyChanged(nameof(PlotData));
            return;
        }

        var config = _configContext.ActiveConfig;

        // ASC and MC are left out (also for the aspects) when hidden.
        var harmonicPositions = HarmonicsOrchestrator.Calculate(_currentChart, config, _harmonic);
        if (_hideAscMc)
            harmonicPositions = harmonicPositions
                .Where(p => p.Factor != Factors.Ascendant && p.Factor != Factors.Mc)
                .ToList();

        PlotData = HarmonicWheelPlotDataBuilder.Build(harmonicPositions, config);
        OnPropertyChanged(nameof(PlotData));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
