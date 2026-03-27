// Dial90WheelCanvas.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Canvas for the 90° dial wheel.
/// Planets are placed at (ecliptic longitude mod 90) × 4 visual degrees.
/// </summary>
public partial class Dial90WheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<Dial90WheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<Dial90WheelCanvas, WheelTheme>(
            nameof(Theme), WheelTheme.Color);

    public WheelPlotData PlotData
    {
        get => GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    public WheelTheme Theme
    {
        get => GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public Dial90WheelCanvas()
    {
        InitializeComponent();

        PlotDataProperty.Changed.AddClassHandler<Dial90WheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<Dial90WheelCanvas>((c, _) => c.InvalidateVisual());
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        var size        = Bounds.Size;
        var diameter    = System.Math.Min(size.Width, size.Height);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center = new Point(size.Width / 2.0, size.Height / 2.0);
        var data   = PlotData;
        var theme  = Theme;

        ctx.FillRectangle(Brushes.White, new Rect(size));

        DrawDial90.DrawBackground(ctx, center, outerRadius, theme);
        DrawDial90.DrawDegreeLabels(ctx, center, outerRadius, theme);
        DrawDial90.Draw1DegTicks(ctx, center, outerRadius, theme);
        DrawDial90.DrawHalfDegTicks(ctx, center, outerRadius, theme);
        DrawDial90.DrawRingStrokes(ctx, center, outerRadius, theme);
        DrawDial90.DrawCenterCross(ctx, center, outerRadius, theme);
        DrawDial90.DrawConnectLines(ctx, center, outerRadius, data, theme);
        DrawDial90.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawDial90.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
