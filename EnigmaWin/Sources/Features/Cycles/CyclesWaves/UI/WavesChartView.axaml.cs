// WavesChartView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ScottPlot.Avalonia;

namespace EnigmaWin.Sources.Features.Cycles.CyclesWaves.UI;

public partial class WavesChartView : UserControl
{
    private readonly AvaPlot _avaPlot = new();
    private WavesChartViewModel? _vm;

    public WavesChartView()
    {
        InitializeComponent();
        _avaPlot.Plot.FigureBackground.Color = ScottPlot.Colors.White;
        ChartPanel.Children.Add(_avaPlot);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
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
        var action = _vm?.PlotActions;
        if (action is null) { _avaPlot.Reset(); _avaPlot.Refresh(); }
        else action(_avaPlot);
    }

    private async void OnExportRequested(object? sender, EventArgs e)
    {
        if (_vm is null) return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title             = _vm.LabelExportCsv,
            SuggestedFileName = "waves.csv",
            FileTypeChoices   = [new FilePickerFileType("CSV") { Patterns = ["*.csv"] }]
        });

        if (file is null) return;
        await using var stream = await file.OpenWriteAsync();
        var bytes = Encoding.UTF8.GetBytes(_vm.BuildCsv());
        await stream.WriteAsync(bytes);
    }

    private async void OnExportPngRequested(object? sender, EventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title             = _vm?.LabelExportPng ?? "Export PNG",
            SuggestedFileName = "waves.png",
            FileTypeChoices   = [new FilePickerFileType("PNG") { Patterns = ["*.png"] }]
        });

        if (file is null) return;
        var path = file.TryGetLocalPath();
        if (path is null) return;
        _avaPlot.Plot.SavePng(path, (int)_avaPlot.Bounds.Width, (int)_avaPlot.Bounds.Height);
    }
}
