// HarmonicsDrawingView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public partial class HarmonicsDrawingView : UserControl
{
    public HarmonicsDrawingView()
    {
        InitializeComponent();
    }

    public async void OnExportClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
