// CyclesChartView.xaml.cs
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

namespace EnigmaWin.Sources.Features.Cycles.CyclesAstronomical.UI;

public partial class CyclesChartView : UserControl
{
    private readonly WpfPlot _wpfPlot = new();
    private CyclesChartViewModel? _vm;

    public CyclesChartView()
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

        _vm = DataContext as CyclesChartViewModel;

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
        if (e.PropertyName == nameof(CyclesChartViewModel.PlotActions))
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
            FileName = "astronomical_cycles.csv",
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
            FileName = "astronomical_cycles.png",
            Filter = "PNG files (*.png)|*.png"
        };
        if (dlg.ShowDialog() != true) return;
        _wpfPlot.Plot.SavePng(dlg.FileName, (int)_wpfPlot.ActualWidth, (int)_wpfPlot.ActualHeight);
    }
}
