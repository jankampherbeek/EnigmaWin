// AltZodiacStartScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

public partial class AltZodiacStartScreen : UserControl
{
    private const int ExportSize = 1200;

    public AltZodiacStartScreen()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is AltZodiacStartViewModel vm)
                vm.Refresh();
        };
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new AltZodiacStartFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new AltZodiacStartHelpWindow(rosetta, "altzodiacstart.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not AltZodiacStartViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = vm.LabelBtnExport,
            Filter     = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        // Render a fresh off-screen canvas so the live one is never re-measured.
        var canvas = new AltZodiacStartWheelCanvas
        {
            DrawingType          = vm.DrawingType,
            PlotData             = vm.PlotData,
            ZodiacStartLongitude = vm.ZodiacStartLongitude,
            Theme                = vm.Theme,
            ShowAspects          = vm.ShowAspects
        };
        canvas.Measure(new Size(ExportSize, ExportSize));
        canvas.Arrange(new Rect(0, 0, ExportSize, ExportSize));
        canvas.UpdateLayout();

        var bitmap = new RenderTargetBitmap(ExportSize, ExportSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);
        using var ms = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        var pngBytes = ms.ToArray();

        if (dlg.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var rgbPixels = WheelExportService.ExtractRgbPixels(pngBytes, out var width, out var height);
            File.WriteAllBytes(dlg.FileName, WheelExportService.BuildPdf(rgbPixels, width, height));
        }
        else
        {
            File.WriteAllBytes(dlg.FileName, pngBytes);
        }
    }
}
