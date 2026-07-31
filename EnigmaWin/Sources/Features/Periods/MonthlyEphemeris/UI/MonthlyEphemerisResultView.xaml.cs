// MonthlyEphemerisResultView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnigmaWin.Sources.Features.ChartDrawing;
using Microsoft.Win32;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.Periods.MonthlyEphemeris.UI;

public partial class MonthlyEphemerisResultView : UserControl
{
    private readonly WpfPlot _wpfPlot = new();
    private MonthlyEphemerisResultViewModel? _vm;

    public MonthlyEphemerisResultView()
    {
        InitializeComponent();
        _wpfPlot.Plot.FigureBackground.Color = ScottPlot.Colors.White;
        _wpfPlot.SizeChanged += (_, _) => ApplyPlot();
        GraphicPanel.Children.Add(_wpfPlot);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.PropertyChanged -= OnVmPropertyChanged;

        _vm = DataContext as MonthlyEphemerisResultViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += OnVmPropertyChanged;

        ApplyPlot();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MonthlyEphemerisResultViewModel.PlotActions))
            ApplyPlot();
    }

    private void ApplyPlot()
    {
        if (_vm is null) { _wpfPlot.Reset(); _wpfPlot.Refresh(); return; }
        _vm.ApplyPlot(_wpfPlot);
    }

    private void OnTabTabularClick(object sender, RoutedEventArgs e) => _vm?.SelectTabular();

    private void OnTabGraphicClick(object sender, RoutedEventArgs e)
    {
        _vm?.SelectGraphic();
        ApplyPlot();
    }

    // ── Table → PDF (all rows, not just the visible portion) ──────────────────

    private void OnExportTablePdfClick(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExportPdf,
            FileName   = "ephemeris_table.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        // Render the title TextBlock + full table content (header + all data rows) unclipped.
        // TableFullContent is the StackPanel inside the ScrollViewer — force it to its full desired size.
        var pngBytes  = RenderFullSizePng(TableFullContent);
        var rgbPixels = WheelExportService.ExtractRgbPixels(pngBytes, out var w, out var h);
        var pdfBytes  = WheelExportService.BuildPdf(rgbPixels, w, h);
        File.WriteAllBytes(dlg.FileName, pdfBytes);
    }

    // ── Chart → PNG (plot + legend) ───────────────────────────────────────────

    private void OnExportChartPngClick(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExportPng,
            FileName   = "ephemeris_chart.png",
            Filter     = "PNG Image (*.png)|*.png",
            DefaultExt = ".png"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        File.WriteAllBytes(dlg.FileName, RenderVisualToPng(ChartPanel));
    }

    // ── Chart → PDF (plot + legend) ───────────────────────────────────────────

    private void OnExportChartPdfClick(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExportPdf,
            FileName   = "ephemeris_chart.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var pngBytes  = RenderVisualToPng(ChartPanel);
        var rgbPixels = WheelExportService.ExtractRgbPixels(pngBytes, out var w, out var h);
        var pdfBytes  = WheelExportService.BuildPdf(rgbPixels, w, h);
        File.WriteAllBytes(dlg.FileName, pdfBytes);
    }

    // ── Render helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Renders a visual at its current on-screen size, composited onto white.
    /// </summary>
    private static byte[] RenderVisualToPng(FrameworkElement element)
    {
        var w = (int)Math.Max(element.ActualWidth,  800);
        var h = (int)Math.Max(element.ActualHeight, 600);
        return RenderToPngWithWhiteBackground(element, w, h);
    }

    /// <summary>
    /// Forces the element to lay out at its full desired size (bypassing ScrollViewer clipping),
    /// renders it composited onto white, then restores the original layout.
    /// </summary>
    private static byte[] RenderFullSizePng(FrameworkElement element)
    {
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var fullW = (int)Math.Max(element.DesiredSize.Width,  400);
        var fullH = (int)Math.Max(element.DesiredSize.Height, 100);

        element.Arrange(new Rect(0, 0, fullW, fullH));
        element.UpdateLayout();

        var pngBytes = RenderToPngWithWhiteBackground(element, fullW, fullH);

        element.InvalidateMeasure();
        element.InvalidateArrange();
        element.UpdateLayout();

        return pngBytes;
    }

    /// <summary>
    /// Core render: draws a white rectangle first, then the element on top, so all
    /// transparent pixels become white instead of black in the PDF.
    /// </summary>
    private static byte[] RenderToPngWithWhiteBackground(FrameworkElement element, int w, int h)
    {
        var bitmap = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);

        // Draw white background.
        var whiteBg = new DrawingVisual();
        using (var ctx = whiteBg.RenderOpen())
            ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
        bitmap.Render(whiteBg);

        // Draw the element on top.
        bitmap.Render(element);

        using var ms = new MemoryStream();
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bitmap));
        enc.Save(ms);
        return ms.ToArray();
    }
}
