// HarmonicsWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

/// <summary>
/// Renders a zodiac-based wheel for harmonics (0° Aries at left, no aspects/cusps).
/// </summary>
public sealed class HarmonicsWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(HarmonicsWheelCanvas),
            new FrameworkPropertyMetadata(WheelPlotData.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(HarmonicsWheelCanvas),
            new FrameworkPropertyMetadata(WheelTheme.Color,
                FrameworkPropertyMetadataOptions.AffectsRender));

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

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var diameter    = Math.Min(w, h);
        var outerRadius = diameter / 2.0;
        var center      = new Point(w / 2.0, h / 2.0);
        var data        = PlotData;
        var theme       = Theme;
        var asc         = data.AscendantLongitude;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        DrawCircles.Draw(ctx, center, outerRadius, theme);
        DrawSigns.DrawElementSectors(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignSeparators(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawSignGlyphs(ctx, center, outerRadius, asc, theme);
        DrawSigns.DrawDegreeLines(ctx, center, outerRadius, asc, theme);

        DrawPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
