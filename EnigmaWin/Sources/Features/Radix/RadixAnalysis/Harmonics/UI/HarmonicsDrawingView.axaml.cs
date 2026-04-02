// HarmonicsDrawingView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public partial class HarmonicsDrawingView : UserControl
{
    public HarmonicsDrawingView()
    {
        InitializeComponent();
    }

    private async void OnFactsheetClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var factsheetWindow = new HarmonicsFactsheetWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await factsheetWindow.ShowDialog(owner);
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new HarmonicsDrawingHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }

    public async void OnExportClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var vm = DataContext as HarmonicsDrawingViewModel;
        if (vm is null) return;

        var pngFilter = new FilePickerFileType("PNG Image")
        {
            Patterns = new List<string> { "*.png" }
        };
        var pdfFilter = new FilePickerFileType("PDF Document")
        {
            Patterns = new List<string> { "*.pdf" }
        };

        var options = new FilePickerSaveOptions
        {
            Title             = "Export harmonic chart",
            SuggestedFileName = "harmonic-chart",
            FileTypeChoices   = new List<FilePickerFileType> { pngFilter, pdfFilter },
            DefaultExtension  = "png"
        };

        var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (result is null) return;

        var filePath = result.Path.LocalPath;
        var plotData = vm.PlotData;
        var theme    = vm.Theme;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext == ".pdf")
            await WheelExportService.ExportToPdfAsync(plotData, theme, false, WheelCanvasType.Zodiac, filePath);
        else
            await WheelExportService.ExportToPngAsync(plotData, theme, false, WheelCanvasType.Zodiac, filePath);

        topLevel?.InvalidateMeasure();
        topLevel?.InvalidateArrange();
    }
}
