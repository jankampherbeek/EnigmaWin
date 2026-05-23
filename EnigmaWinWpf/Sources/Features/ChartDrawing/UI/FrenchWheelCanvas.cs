// FrenchWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// FrameworkElement that renders a French-style horoscope wheel by overriding OnRender().
/// In this layout the zodiac sign ring sits inside the house ring and planets
/// are drawn outside the house ring.
/// </summary>
public class FrenchWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(FrenchWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(FrenchWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(FrenchWheelCanvas),
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
        => ((FrenchWheelCanvas)d).InvalidateVisual();

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
        var asc    = data.AscendantLongitude;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        DrawFrenchCircles.Draw(ctx, center, outerRadius, theme);
        DrawFrenchSigns.DrawElementSectors(ctx, center, outerRadius, asc, theme);
        DrawFrenchSigns.DrawSignSeparators(ctx, center, outerRadius, asc, theme);
        DrawFrenchSigns.DrawSignGlyphs(ctx, center, outerRadius, asc, theme);
        DrawFrenchSigns.DrawDegreeLines(ctx, center, outerRadius, asc, theme);

        if (data.HasTime)
        {
            DrawFrenchCusps.DrawCuspLines(ctx, center, outerRadius, data, theme);
            DrawFrenchCusps.DrawHouseNumbers(ctx, center, outerRadius, data, theme);
            DrawFrenchCusps.DrawCardinalGlyphs(ctx, center, outerRadius, data, theme);
        }

        if (ShowAspects)
            DrawAspects.Draw(ctx, center, outerRadius, data, theme, FrenchWheelMetrics.OuterAspect);

        DrawFrenchPlanets.DrawPlanetConnectLines(ctx, center, outerRadius, data, theme);
        DrawFrenchPlanets.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawFrenchPlanets.DrawPlanetTexts(ctx, center, outerRadius, data, theme);
    }
}
