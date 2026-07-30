// EclipsesResultView.xaml.cs
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

namespace EnigmaWin.Sources.Features.Periods.Eclipses.UI;

public partial class EclipsesResultView : UserControl
{
    private EclipsesResultViewModel? _vm;

    public EclipsesResultView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => _vm = DataContext as EclipsesResultViewModel;
    }

    private void OnExportPdfClick(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExportPdf,
            FileName   = "eclipses.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var pngBytes  = RenderFullSizePng(TableFullContent);
        var rgbPixels = WheelExportService.ExtractRgbPixels(pngBytes, out var w, out var h);
        var pdfBytes  = WheelExportService.BuildPdf(rgbPixels, w, h);
        File.WriteAllBytes(dlg.FileName, pdfBytes);
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new EclipsesHelpWindow(rosetta, "view.eclipses.help.results") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    // ── Render helpers ────────────────────────────────────────────────────────

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

        var whiteBg = new DrawingVisual();
        using (var ctx = whiteBg.RenderOpen())
            ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
        bitmap.Render(whiteBg);

        bitmap.Render(element);

        using var ms = new MemoryStream();
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bitmap));
        enc.Save(ms);
        return ms.ToArray();
    }
}
