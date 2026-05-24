// DeclinationDiagramView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationDiagramView : UserControl
{
    public DeclinationDiagramView()
    {
        InitializeComponent();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new DeclinationsFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new DeclinationsHelpWindow(rosetta, "declinations.diagram.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title      = "Export declination diagram",
            FileName   = "declinations-diagram",
            DefaultExt = ".png",
            Filter     = "PNG Image|*.png"
        };
        if (dlg.ShowDialog() != true) return;

        const int exportWidth  = 1000;
        var       exportHeight = (int)(exportWidth / 1.25);

        DiagramCanvas.Measure(new Size(exportWidth, exportHeight));
        DiagramCanvas.Arrange(new Rect(0, 0, exportWidth, exportHeight));

        var rtb = new RenderTargetBitmap(exportWidth, exportHeight, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(DiagramCanvas);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using var fs = new FileStream(dlg.FileName, FileMode.Create);
        encoder.Save(fs);
    }
}
