// WavesChartView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.Cycles.CyclesWaves.UI;

public partial class WavesChartView : UserControl
{
    private readonly WpfPlot _wpfPlot = new();
    private WavesChartViewModel? _vm;

    public WavesChartView()
    {
        InitializeComponent();
        _wpfPlot.Plot.FigureBackground.Color = ScottPlot.Colors.White;
        _wpfPlot.SizeChanged += (_, _) => ApplyPlot();
        ChartPanel.Children.Add(_wpfPlot);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.PropertyChanged    -= OnVmPropertyChanged;
            _vm.ExportRequested    -= OnExportRequested;
            _vm.ExportPngRequested -= OnExportPngRequested;
        }

        _vm = DataContext as WavesChartViewModel;

        if (_vm is not null)
        {
            _vm.PropertyChanged    += OnVmPropertyChanged;
            _vm.ExportRequested    += OnExportRequested;
            _vm.ExportPngRequested += OnExportPngRequested;
        }

        ApplyPlot();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WavesChartViewModel.PlotActions))
            ApplyPlot();
    }

    private void ApplyPlot()
    {
        if (_vm is null) { _wpfPlot.Reset(); _wpfPlot.Refresh(); return; }
        _vm.ApplyPlot(_wpfPlot);
    }

    private void OnExportRequested(object? sender, EventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title = _vm.LabelExportCsv,
            FileName = "waves.csv",
            Filter = "CSV files (*.csv)|*.csv"
        };
        if (dlg.ShowDialog() != true) return;
        File.WriteAllBytes(dlg.FileName, Encoding.UTF8.GetBytes(_vm.BuildCsv()));
    }

    private void OnExportPngRequested(object? sender, EventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title = _vm?.LabelExportPng ?? "Export PNG",
            FileName = "waves.png",
            Filter = "PNG files (*.png)|*.png"
        };
        if (dlg.ShowDialog() != true) return;
        _wpfPlot.Plot.SavePng(dlg.FileName, (int)_wpfPlot.ActualWidth, (int)_wpfPlot.ActualHeight);
    }
}
