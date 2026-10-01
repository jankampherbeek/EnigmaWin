// ChartWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// FrameworkElement that renders a single chart in the given drawing type.
/// PlotData is zodiac-based data from <see cref="WheelPlotDataBuilder"/>; it is projected to the drawing type.
/// </summary>
public class ChartWheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(ChartWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty DrawingTypeProperty =
        DependencyProperty.Register(nameof(DrawingType), typeof(DrawingTypes), typeof(ChartWheelCanvas),
            new PropertyMetadata(DrawingTypes.SignBased, OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(ChartWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(ChartWheelCanvas),
            new PropertyMetadata(true, OnVisualPropertyChanged));

    public WheelPlotData PlotData
    {
        get => (WheelPlotData)GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    public DrawingTypes DrawingType
    {
        get => (DrawingTypes)GetValue(DrawingTypeProperty);
        set => SetValue(DrawingTypeProperty, value);
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
        => ((ChartWheelCanvas)d).InvalidateVisual();

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w           = ActualWidth;
        var h           = ActualHeight;
        var outerRadius = Math.Min(w, h) / 2.0;
        if (outerRadius <= 0) return;

        var type = WheelProjection.Effective(DrawingType, PlotData);
        var data = WheelProjection.ProjectChart(PlotData, type);

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
        WheelRenderer.Render(type, ctx, new Point(w / 2.0, h / 2.0), outerRadius, data, Theme, ShowAspects);
    }
}
