// AllDeclinationsView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class AllDeclinationsView : UserControl
{
    public AllDeclinationsView()
    {
        InitializeComponent();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        var window  = new DeclinationsFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) };
        window.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        var window  = new DeclinationsHelpWindow(rosetta, "declinations.all.help") { Owner = Window.GetWindow(this) };
        window.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title             = "Export declination strip",
            FileName          = "declinations-strip",
            DefaultExt        = ".png",
            Filter            = "PNG Image|*.png"
        };
        if (dlg.ShowDialog() != true) return;

        ExportCanvasToPng(StripCanvas, dlg.FileName);
    }

    private static void ExportCanvasToPng(FrameworkElement canvas, string filePath)
    {
        const int exportWidth = 800;
        var exportHeight = (int)(exportWidth / 0.55);

        canvas.Measure(new Size(exportWidth, exportHeight));
        canvas.Arrange(new Rect(0, 0, exportWidth, exportHeight));

        var rtb = new RenderTargetBitmap(exportWidth, exportHeight, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(canvas);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using var fs = new FileStream(filePath, FileMode.Create);
        encoder.Save(fs);
    }
}
