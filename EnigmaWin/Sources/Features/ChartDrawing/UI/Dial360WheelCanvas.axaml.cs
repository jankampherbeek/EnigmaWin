// Dial360WheelCanvas.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Canvas for the Ebertin-style 360° dial wheel.
/// Planets are placed at their ecliptic longitude; Aries is at the top.
/// </summary>
public partial class Dial360WheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<Dial360WheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<Dial360WheelCanvas, WheelTheme>(
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

    public Dial360WheelCanvas()
    {
        InitializeComponent();

        PlotDataProperty.Changed.AddClassHandler<Dial360WheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<Dial360WheelCanvas>((c, _) => c.InvalidateVisual());
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

        DrawDial360.DrawBackground(ctx, center, outerRadius, theme);
        DrawDial360.DrawSignSectors(ctx, center, outerRadius, theme);
        DrawDial360.DrawSignGlyphs(ctx, center, outerRadius, theme);
        DrawDial360.DrawSignSeparators(ctx, center, outerRadius, theme);
        DrawDial360.DrawDegreeBoundaryLabels(ctx, center, outerRadius, theme);
        DrawDial360.Draw10DegTicks(ctx, center, outerRadius, theme);
        DrawDial360.DrawDegTicks(ctx, center, outerRadius, theme);
        DrawDial360.DrawRingStrokes(ctx, center, outerRadius, theme);
        DrawDial360.DrawCenterCross(ctx, center, outerRadius, theme);
        DrawDial360.DrawConnectLines(ctx, center, outerRadius, data, theme);
        DrawDial360.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawDial360.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
