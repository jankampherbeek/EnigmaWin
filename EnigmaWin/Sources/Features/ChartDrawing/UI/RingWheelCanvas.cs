// RingWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// FrameworkElement that renders a Ring-style horoscope wheel by overriding OnRender().
/// One circle shows all house cusps as spokes from the centre; planets sit on the ring.
/// </summary>
public class RingWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(RingWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(RingWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(RingWheelCanvas),
            new PropertyMetadata(true, OnVisualPropertyChanged));

    public WheelPlotData PlotData
    {
        get => (WheelPlotData)GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    public WheelTheme Theme
    {
        get => (WheelTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public bool ShowAspects
    {
        get => (bool)GetValue(ShowAspectsProperty);
        set => SetValue(ShowAspectsProperty, value);
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((RingWheelCanvas)d).InvalidateVisual();

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w           = ActualWidth;
        var h           = ActualHeight;
        var diameter    = Math.Min(w, h);
        var outerRadius = diameter / 2.0;
        if (outerRadius <= 0) return;

        var center = new Point(w / 2.0, h / 2.0);
        var data   = PlotData;
        var theme  = Theme;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        DrawRingWheel.DrawCircle(ctx, center, outerRadius, theme);

        if (data.HasTime)
        {
            DrawRingWheel.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawRingWheel.DrawCuspLabels(ctx, center, outerRadius, data, theme);
            DrawRingWheel.DrawInterceptedSignGlyphs(ctx, center, outerRadius, data, theme);
        }

        if (ShowAspects)
            DrawRingWheel.DrawAspectLines(ctx, center, outerRadius, data, theme);

        DrawRingWheel.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawRingWheel.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
