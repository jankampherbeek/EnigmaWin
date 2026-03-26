// ZodiacWheelCanvas.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Canvas that renders a zodiac-based horoscope wheel by overriding Render().
/// Set PlotData, Theme, and ShowAspects before the control is shown.
/// </summary>
public partial class ZodiacWheelCanvas : UserControl
{
    public static readonly StyledProperty<WheelPlotData> PlotDataProperty =
        AvaloniaProperty.Register<ZodiacWheelCanvas, WheelPlotData>(
            nameof(PlotData), WheelPlotData.Empty);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<ZodiacWheelCanvas, WheelTheme>(
            nameof(Theme), WheelTheme.Color);

    public static readonly StyledProperty<bool> ShowAspectsProperty =
        AvaloniaProperty.Register<ZodiacWheelCanvas, bool>(
            nameof(ShowAspects), true);

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

    public bool ShowAspects
    {
        get => GetValue(ShowAspectsProperty);
        set => SetValue(ShowAspectsProperty, value);
    }

    public ZodiacWheelCanvas()
    {
        InitializeComponent();

        // Redraw whenever any property changes
        PlotDataProperty.Changed.AddClassHandler<ZodiacWheelCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<ZodiacWheelCanvas>((c, _) => c.InvalidateVisual());
        ShowAspectsProperty.Changed.AddClassHandler<ZodiacWheelCanvas>((c, _) => c.InvalidateVisual());
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        var size        = Bounds.Size;
        var diameter    = System.Math.Min(size.Width, size.Height);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center      = new Point(size.Width / 2.0, size.Height / 2.0);
        var data        = PlotData;
        var theme       = Theme;
        var asc         = data.AscendantLongitude;

        // White background
        ctx.FillRectangle(Brushes.White, new Rect(size));

        DrawCircles.Draw(ctx, center, outerRadius, theme);
        DrawSigns.DrawElementSectors(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignSeparators(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignGlyphs(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawDegreeLines(ctx, center, outerRadius, asc, theme);

        if (data.HasTime)
        {
            DrawCusps.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawCusps.DrawCardinalLines(ctx, center, outerRadius, data, theme);
            DrawCusps.DrawCardinalLabels(ctx, center, outerRadius, data, theme);
            DrawCusps.DrawCuspTexts(ctx, center, outerRadius, data, theme);
        }

        if (ShowAspects)
            DrawAspects.Draw(ctx, center, outerRadius, data, theme);

        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
