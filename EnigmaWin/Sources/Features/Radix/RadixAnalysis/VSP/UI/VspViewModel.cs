// VspViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Serilog;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.VSP.UI;

public sealed class VspViewModel : INotifyPropertyChanged
{
    private readonly IRosetta      _rosetta;
    private readonly IChartSession _chartSession;
    private readonly IConfigContext _configContext;
    private FullChart?              _currentChart;

    public ObservableCollection<PresentableVspPosition> VspPositions { get; } = new();

    private bool _hasData;
    public bool HasData   { get => _hasData;   private set { if (_hasData == value) return; _hasData = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNoData)); } }
    public bool HasNoData => !HasData;

    // The Head point (sequence 3) longitude — used to rotate the wheel so Head sits at top
    public double HeadLongitude { get; private set; }

    // The real Ascendant longitude before VSP rotation — so A/D/M/I draw at their actual zodiacal positions
    public double OriginalAscLongitude { get; private set; }

    // WheelPlotData for the canvas
    public WheelPlotData PlotData { get; private set; } = WheelPlotData.Empty;

    public VspViewModel(IRosetta rosetta, IConfigContext configContext, IChartSession chartSession)
    {
        _rosetta       = rosetta;
        _configContext = configContext;
        _chartSession  = chartSession;
        _currentChart  = chartSession.SelectedChart;

        if (chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(IChartSession.SelectedChart)) return;
                _currentChart = _chartSession.SelectedChart;
                Recalculate();
            };

        Recalculate();
    }

    public void Refresh()
    {
        _currentChart = _chartSession.SelectedChart;
        Recalculate();
    }

    private void Recalculate()
    {
        VspPositions.Clear();
        PlotData    = WheelPlotData.Empty;
        HasData     = false;

        if (_currentChart == null) return;

        try
        {
            PlotData = WheelPlotDataBuilder.Build(_currentChart, _configContext.ActiveConfig);
            OnPropertyChanged(nameof(PlotData));

            var positions = VspCalculator.Calculate(_currentChart.JulianDay);
            foreach (var pos in positions)
                VspPositions.Add(new PresentableVspPosition(pos));

            OriginalAscLongitude = PlotData.AscendantLongitude;
            OnPropertyChanged(nameof(OriginalAscLongitude));

            var head     = VspPositions.FirstOrDefault(p => p.SequenceId == 3);
            var headLon  = head?.Longitude ?? 0.0;
            // +90 shifts Head from the ASC position (left/9-o'clock) to the top (0°/12-o'clock).
            HeadLongitude = (headLon + 90.0) % 360.0;
            OnPropertyChanged(nameof(HeadLongitude));
            OnPropertyChanged(nameof(VspPositions));

            HasData = VspPositions.Count > 0;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "VSP calculation failed.");
            HasData = false;
        }
    }

    // i18n labels
    public string LabelTitle       => _rosetta.GetText(RbFile.RadixVsp, "vsp.title");
    public string LabelNoChart     => _rosetta.GetText(RbFile.RadixVsp, "vsp.nochart");
    public string LabelExplanation => _rosetta.GetText(RbFile.RadixVsp, "vsp.explanation");
    public string LabelColSeq      => _rosetta.GetText(RbFile.RadixVsp, "vsp.col.seq");
    public string LabelColName     => _rosetta.GetText(RbFile.RadixVsp, "vsp.col.name");
    public string LabelColPhen     => _rosetta.GetText(RbFile.RadixVsp, "vsp.col.phenomenon");
    public string LabelColDate     => _rosetta.GetText(RbFile.RadixVsp, "vsp.col.date");
    public string LabelColLon      => _rosetta.GetText(RbFile.RadixVsp, "vsp.col.longitude");
    public string TooltipHelp      => _rosetta.GetText(RbFile.RadixVsp, "vsp.help.tooltip");
    public string TooltipFactsheet => _rosetta.GetText(RbFile.RadixVsp, "vsp.factsheet.tooltip");

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
