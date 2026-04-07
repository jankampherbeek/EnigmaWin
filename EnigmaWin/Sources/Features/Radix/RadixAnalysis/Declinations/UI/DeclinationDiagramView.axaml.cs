// DeclinationDiagramView.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationDiagramView : UserControl
{
    // Threshold: canvas min width ~640 + table ~280 = 920; leave a little room
    private const double WideThreshold = 860;

    public DeclinationDiagramView()
    {
        InitializeComponent();
    }

    // ── Responsive layout ─────────────────────────────────────────────────

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is not DeclinationDiagramViewModel vm || !vm.HasData) return;
        var wide = e.NewSize.Width >= WideThreshold;
        WideLayout.IsVisible   = wide;
        NarrowLayout.IsVisible = !wide;
    }

    // ── Factsheet ─────────────────────────────────────────────────────────

    private async void OnFactsheetClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var window = new DeclinationsFactsheetWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await window.ShowDialog(owner);
    }

    // ── Export ────────────────────────────────────────────────────────────

    public async void OnExportClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var options = new FilePickerSaveOptions
        {
            Title             = "Export declination diagram",
            SuggestedFileName = "declination-diagram",
            FileTypeChoices   = new List<FilePickerFileType>
            {
                new FilePickerFileType("PNG Image") { Patterns = ["*.png"] }
            },
            DefaultExtension = "png"
        };

        var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (result is null) return;

        ExportCanvasToPng(DiagramCanvas, result.Path.LocalPath);

        topLevel.InvalidateMeasure();
        topLevel.InvalidateArrange();
    }

    private static void ExportCanvasToPng(Control canvas, string filePath)
    {
        const int exportWidth  = 1000;
        const int exportHeight = 800;
        var bitmap = new RenderTargetBitmap(new PixelSize(exportWidth, exportHeight), new Vector(96, 96));

        var renderSize = new Size(exportWidth, exportHeight);
        canvas.Measure(renderSize);
        canvas.Arrange(new Rect(renderSize));
        bitmap.Render(canvas);

        using var ms = new MemoryStream();
        bitmap.Save(ms);
        File.WriteAllBytes(filePath, ms.ToArray());
    }

    // ── Help ──────────────────────────────────────────────────────────────

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var window = new DeclinationsHelpWindow(rosetta, "declinations.diagram.help");
        if (TopLevel.GetTopLevel(this) is Window owner)
            await window.ShowDialog(owner);
    }
}
