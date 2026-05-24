// Dial45WheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// FrameworkElement for the 45° dial wheel.
/// Planets are placed at (ecliptic longitude mod 45) × 8 visual degrees.
/// Supports hover and click to show midpoint overlay lines.
/// </summary>
public class Dial45WheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(Dial45WheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(Dial45WheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualPropertyChanged));

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

    private Factors? _hoveredFactor;
    private Factors? _pinnedFactor;
    private Factors? ActiveFactor => _pinnedFactor ?? _hoveredFactor;

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((Dial45WheelCanvas)d).InvalidateVisual();

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var (center, outerR) = GetGeometry();
        var hit = DialMidpointOverlay.NearestFactor(e.GetPosition(this), center, outerR, PlotData);
        if (hit != _hoveredFactor)
        {
            _hoveredFactor = hit;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoveredFactor != null)
        {
            _hoveredFactor = null;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var (center, outerR) = GetGeometry();
        var tapped = DialMidpointOverlay.NearestFactor(e.GetPosition(this), center, outerR, PlotData);
        _pinnedFactor = (tapped != null && tapped == _pinnedFactor) ? null : tapped;
        InvalidateVisual();
    }

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

        DrawDial45.DrawBackground(ctx, center, outerRadius, theme);
        DrawDial45.DrawDegreeLabels(ctx, center, outerRadius, theme);
        DrawDial45.Draw1DegTicks(ctx, center, outerRadius, theme);
        DrawDial45.DrawHalfDegTicks(ctx, center, outerRadius, theme);
        DrawDial45.DrawRingStrokes(ctx, center, outerRadius, theme);
        DrawDial45.DrawCenterCross(ctx, center, outerRadius, theme);
        DrawDial45.DrawConnectLines(ctx, center, outerRadius, data, theme);
        DrawDial45.DrawPlanetGlyphs(ctx, center, outerRadius, data, theme);
        DrawDial45.DrawPlanetTexts(ctx, center, outerRadius, data, theme);

        if (ActiveFactor is { } active)
            DialMidpointOverlay.Draw(ctx, center, outerRadius, data, active);
    }

    private (Point center, double outerR) GetGeometry()
    {
        var diameter = Math.Min(ActualWidth, ActualHeight);
        var center   = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
        return (center, diameter / 2.0);
    }
}
