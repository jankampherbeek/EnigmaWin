// Dial360WheelCanvas.cs
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
/// FrameworkElement for the Ebertin-style 360° dial wheel.
/// Planets are placed at their ecliptic longitude; Aries is at the top.
/// Supports hover and click to show midpoint overlay lines.
/// </summary>
public class Dial360WheelCanvas : FrameworkElement
{
    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(Dial360WheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(Dial360WheelCanvas),
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
        => ((Dial360WheelCanvas)d).InvalidateVisual();

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
