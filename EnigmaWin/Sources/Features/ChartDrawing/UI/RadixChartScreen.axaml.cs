// RadixChartScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

public partial class RadixChartScreen : UserControl
{
    private readonly RadixChartViewModel _viewModel;
    private readonly IRosetta            _rosetta;

    public RadixChartScreen()
    {
        var app = Application.Current as App
            ?? throw new InvalidOperationException("App not available");

        var chartSession  = app.Services.GetRequiredService<IChartSession>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();
        _rosetta          = app.Services.GetRequiredService<IRosetta>();

        _viewModel = new RadixChartViewModel(chartSession, configContext, _rosetta);

        InitializeComponent();
        DataContext = _viewModel;
    }

    // MARK: - Dial type switch

    /// <summary>
    /// Switches between Dial360 and Dial90 via the Tag on the clicked button.
    /// </summary>
    public void OnDialTypeClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var tag = (sender as Avalonia.Controls.Primitives.ToggleButton)?.Tag?.ToString()
               ?? (sender as Avalonia.Controls.Button)?.Tag?.ToString();
        var configContext = (Application.Current as App)
            ?.Services.GetRequiredService<IConfigContext>();
        if (configContext is null) return;

        var newType = tag switch
        {
            "360" => (DrawingTypes?)DrawingTypes.Dial360,
            "90"  => (DrawingTypes?)DrawingTypes.Dial90,
            "45"  => (DrawingTypes?)DrawingTypes.Dial45,
            _     => (DrawingTypes?)null
        };
        if (newType is null) return;

        var old = configContext.ActiveConfig;
        configContext.ActiveConfig = new UserConfiguration
        {
            Id                = old.Id,
            Name              = old.Name,
            IsActive          = old.IsActive,
            Language          = old.Language,
            CalculationConfig = old.CalculationConfig,
            DisplayConfig     = new DisplayConfig(newType.Value, old.DisplayConfig.SignColors),
            GlyphsConfig      = old.GlyphsConfig,
            FactorConfig      = old.FactorConfig,
            AspectConfig      = old.AspectConfig,
            OrbConfig         = old.OrbConfig,
            ProgressionsConfig = old.ProgressionsConfig
        };
    }

    // MARK: - Help

    public void OnHelpClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var owner = TopLevel.GetTopLevel(this) as Window;
        var helpWindow = new ChartWheelHelpWindow(_rosetta, _viewModel.DrawingType);
        helpWindow.ShowDialog(owner);
    }

    // MARK: - Export

    /// <summary>
    /// Shows a native Save File dialog with PNG and PDF filters.
    /// Exports the visible wheel canvas to the chosen format.
    /// Does nothing if the user cancels.
    /// </summary>
    public async void OnExportClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

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
            Title              = "Export chart",
            SuggestedFileName  = "chart",
            FileTypeChoices    = new List<FilePickerFileType> { pngFilter, pdfFilter },
            DefaultExtension   = "png"
        };

        var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (result is null) return;

        var filePath  = result.Path.LocalPath;
        var canvasType = _viewModel.IsZodiacWheel  ? WheelCanvasType.Zodiac
                       : _viewModel.IsFrenchWheel  ? WheelCanvasType.French
                       : _viewModel.IsRingWheel    ? WheelCanvasType.Ring
                       : _viewModel.IsDial360Wheel ? WheelCanvasType.Dial360
                       : _viewModel.IsDial90Wheel  ? WheelCanvasType.Dial90
                       : _viewModel.IsDial45Wheel  ? WheelCanvasType.Dial45
                       : WheelCanvasType.House;
        var plotData    = _viewModel.IsHouseWheel   ? _viewModel.HousePlotData
                        : _viewModel.IsDial360Wheel ? _viewModel.DialPlotData
                        : _viewModel.IsDial90Wheel  ? _viewModel.Dial90PlotData
                        : _viewModel.IsDial45Wheel  ? _viewModel.Dial45PlotData
                        : _viewModel.PlotData;
        var theme       = _viewModel.Theme;
        var showAspects = _viewModel.ShowAspects;

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext == ".pdf")
            await WheelExportService.ExportToPdfAsync(plotData, theme, showAspects, canvasType, filePath);
        else
            await WheelExportService.ExportToPngAsync(plotData, theme, showAspects, canvasType, filePath);

        // Force the window to re-layout so the live canvas restores its correct size.
        topLevel?.InvalidateMeasure();
        topLevel?.InvalidateArrange();
    }
}
