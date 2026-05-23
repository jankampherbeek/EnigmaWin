// HarmonicsDrawingView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public partial class HarmonicsDrawingView : UserControl
{
    public HarmonicsDrawingView()
    {
        InitializeComponent();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        new HarmonicsFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        new HarmonicsDrawingHelpWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not HarmonicsDrawingViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = "Export harmonic chart",
            Filter     = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };

        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var filePath = dlg.FileName;
        var plotData = vm.PlotData;
        var theme    = vm.Theme;

        if (filePath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase))
            WheelExportService.ExportToPdfAsync(plotData, theme, false, WheelCanvasType.Zodiac, filePath);
        else
            WheelExportService.ExportToPngAsync(plotData, theme, false, WheelCanvasType.Zodiac, filePath);
    }
}
