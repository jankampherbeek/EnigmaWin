// DualWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Progressive.DualWheel;

/// <summary>
/// FrameworkElement that renders a zodiac-type radix wheel scaled to ~78% of the available
/// radius, with an outer transit ring containing the progressive planet glyphs and position texts.
/// </summary>
public class DualWheelCanvas : FrameworkElement
{
    // Fractions of the full (unscaled) outer radius for the transit ring
    private const double RadixScale                = 0.78;
    private const double TransitBackgroundFraction = 0.864;
    private const double TransitGlyphFraction      = 0.760;
    private const double TransitTextFraction       = 0.815;
    private const double TransitConnectStart       = 0.73;

    // ── Dependency properties ────────────────────────────────────────────────

    public static readonly DependencyProperty RadixDataProperty =
        DependencyProperty.Register(nameof(RadixData), typeof(WheelPlotData), typeof(DualWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualPropertyChanged));

    public static readonly DependencyProperty TransitItemsProperty =
        DependencyProperty.Register(nameof(TransitItems), typeof(WheelPlotItem[]), typeof(DualWheelCanvas),
            new PropertyMetadata(Array.Empty<WheelPlotItem>(), OnVisualPropertyChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(DualWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(DualWheelCanvas),
            new PropertyMetadata(true, OnVisualPropertyChanged));

    public WheelPlotData RadixData
    {
        get => (WheelPlotData)GetValue(RadixDataProperty);
        set => SetValue(RadixDataProperty, value);
    }

    public WheelPlotItem[] TransitItems
    {
        get => (WheelPlotItem[])GetValue(TransitItemsProperty);
        set => SetValue(TransitItemsProperty, value);
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
        => ((DualWheelCanvas)d).InvalidateVisual();

    // ── Rendering ────────────────────────────────────────────────────────────

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w          = ActualWidth;
        var h          = ActualHeight;
        var fullRadius = Math.Min(w, h) / 2.0;
        if (fullRadius <= 0) return;

        var center      = new Point(w / 2.0, h / 2.0);
        var innerRadius = fullRadius * RadixScale;
        var data        = RadixData;
        var theme       = Theme;
        var asc         = data.AscendantLongitude;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        // Transit ring background (annulus between innerRadius and transitBackgroundFraction × fullRadius)
        DrawTransitBackground(ctx, center, fullRadius, TransitBackgroundFraction, theme);

        // Radix wheel (scaled to innerRadius)
        DrawCircles.Draw(ctx, center, innerRadius, theme);
        DrawSigns.DrawElementSectors(ctx, center, innerRadius, asc, theme);
        DrawSigns.DrawSignSeparators(ctx, center, innerRadius, asc, theme);
        DrawSigns.DrawSignGlyphs(ctx, center, innerRadius, asc, theme);
        DrawSigns.DrawDegreeLines(ctx, center, innerRadius, asc, theme);

        if (data.HasTime)
        {
            DrawCusps.DrawCuspLines(ctx, center, innerRadius, data, theme);
            DrawCusps.DrawCardinalLines(ctx, center, innerRadius, data, theme);
            DrawCusps.DrawCardinalLabels(ctx, center, innerRadius, data, theme);
            DrawCusps.DrawCuspTexts(ctx, center, innerRadius, data, theme);
        }

        if (ShowAspects)
            DrawAspects.Draw(ctx, center, innerRadius, data, theme);

        DrawPlanets.DrawPlanetConnectLines(ctx, center, innerRadius, data, theme);
        DrawPlanets.DrawPlanetGlyphs(ctx, center, innerRadius, data, theme);
        DrawPlanets.DrawPlanetTexts(ctx, center, innerRadius, data, theme);

        // Transit ring (glyphs, texts, connect lines)
        DrawTransitConnectLines(ctx, center, fullRadius, innerRadius, theme);
        DrawTransitGlyphs(ctx, center, fullRadius, innerRadius, theme);
        DrawTransitTexts(ctx, center, fullRadius, innerRadius, theme);
    }

    // ── Transit ring drawing ─────────────────────────────────────────────────

    private void DrawTransitBackground(DrawingContext ctx, Point center, double fullRadius,
                                        double outerFraction, WheelTheme theme)
    {
        var r    = fullRadius * outerFraction;
        var rect = new Rect(center.X - r, center.Y - r, r * 2, r * 2);
        ctx.DrawEllipse(new SolidColorBrush(theme.OuterCircleBackground), null, center, r, r);
    }

    private void DrawTransitConnectLines(DrawingContext ctx, Point center,
                                          double fullRadius, double innerRadius,
                                          WheelTheme theme)
    {
        var items      = TransitItems;
        var startR     = fullRadius * TransitConnectStart;
        var signRingR  = innerRadius * WheelMetrics.OuterSign;
        var stroke     = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, fullRadius);
        var pc         = theme.PlanetConnectLine;
        var alpha      = (byte)(WheelMetrics.ConnectLineOpacity * 255);
        var color      = Color.FromArgb(alpha, pc.R, pc.G, pc.B);
        var pen        = new Pen(new SolidColorBrush(color), stroke);

        foreach (var item in items)
        {
            var p1 = WheelGeometry.PointOnCircle(item.PlotAngle,    startR,    center);
            var p2 = WheelGeometry.PointOnCircle(item.MundaneAngle, signRingR, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    private void DrawTransitGlyphs(DrawingContext ctx, Point center,
                                    double fullRadius, double innerRadius,
                                    WheelTheme theme)
    {
        var items    = TransitItems;
        var r        = fullRadius * TransitGlyphFraction;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, innerRadius);
        var typeface = WheelMetrics.GlyphTypeface;
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in items)
        {
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, typeface, brush);
        }
    }

    private void DrawTransitTexts(DrawingContext ctx, Point center,
                                   double fullRadius, double innerRadius,
                                   WheelTheme theme)
    {
        var items    = TransitItems;
        var r        = fullRadius * TransitTextFraction;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, innerRadius);
        var typeface = new Typeface("Segoe UI");
        var brush    = new SolidColorBrush(theme.PlanetText);

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.PositionText)) continue;

            var pa = item.PlotAngle;
            var pt = WheelGeometry.PointOnCircle(pa, r, center);

            double rotDeg;
            if (pa < 180.0)
                rotDeg = 90.0 - pa;
            else
                rotDeg = 270.0 - pa;

            DrawRotatedText(ctx, item.PositionText, pt, rotDeg, fontSize, typeface, brush);
        }
    }

    // ── Text helpers ─────────────────────────────────────────────────────────

    private static void DrawTextCentered(DrawingContext ctx, string text, Point center,
                                          double fontSize, Typeface typeface, Brush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface, fontSize, brush, 1.0);
        ctx.DrawText(ft, new Point(center.X - ft.Width / 2, center.Y - ft.Height / 2));
    }

    private static void DrawRotatedText(DrawingContext ctx, string text, Point pt,
                                         double rotDeg, double fontSize,
                                         Typeface typeface, Brush brush)
    {
        var ft = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface, fontSize, brush, 1.0);

        var rad       = rotDeg * Math.PI / 180.0;
        var cos       = Math.Cos(rad);
        var sin       = Math.Sin(rad);
        var transform = new MatrixTransform(new Matrix(cos, sin, -sin, cos, pt.X, pt.Y));
        ctx.PushTransform(transform);
        ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
        ctx.Pop();
    }
}
