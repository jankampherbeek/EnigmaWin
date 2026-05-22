// CyclesChartView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using Avalonia.Controls;
using ScottPlot.Avalonia;

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical.UI;

public partial class CyclesChartView : UserControl
{
    private readonly AvaPlot _avaPlot = new();
    private CyclesChartViewModel? _vm;

    public CyclesChartView()
    {
        InitializeComponent();
        _avaPlot.Plot.FigureBackground.Color = ScottPlot.Colors.White;
        ChartPanel.Children.Add(_avaPlot);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (_vm is not null)
            _vm.PropertyChanged -= OnVmPropertyChanged;

        _vm = DataContext as CyclesChartViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += OnVmPropertyChanged;

        ApplyPlot();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CyclesChartViewModel.PlotActions))
            ApplyPlot();
    }

    private void ApplyPlot()
    {
        var action = _vm?.PlotActions;
        if (action is null)
        {
            _avaPlot.Reset();
            _avaPlot.Refresh();
        }
        else
        {
            action(_avaPlot);
        }
    }
}
