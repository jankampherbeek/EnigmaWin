// DualWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.UI;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects;

namespace EnigmaWin.Sources.Features.Progressive.DualWheel;

/// <summary>
/// FrameworkElement that renders a radix wheel in the configured drawing type, scaled to ~78% of the
/// available radius, with an outer transit ring containing the progressive planet glyphs and position texts.
/// RadixData, TransitItems and InterChartAspects are zodiac-based; they are projected to the drawing type.
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

    /// <summary>Optional aspects between the inner (radix) and outer (transit/synastry) rings.
    /// Empty by default so existing (Progressive) usage is unaffected.</summary>
    public static readonly DependencyProperty InterChartAspectsProperty =
        DependencyProperty.Register(nameof(InterChartAspects), typeof(WheelAspectItem[]), typeof(DualWheelCanvas),
            new PropertyMetadata(Array.Empty<WheelAspectItem>(), OnVisualPropertyChanged));

    /// <summary>Drawing type of the inner (radix) wheel; the outer ring follows its angles.</summary>
    public static readonly DependencyProperty DrawingTypeProperty =
        DependencyProperty.Register(nameof(DrawingType), typeof(DrawingTypes), typeof(DualWheelCanvas),
            new PropertyMetadata(DrawingTypes.SignBased, OnVisualPropertyChanged));

    public DrawingTypes DrawingType
    {
        get => (DrawingTypes)GetValue(DrawingTypeProperty);
        set => SetValue(DrawingTypeProperty, value);
    }

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

    public WheelAspectItem[] InterChartAspects
    {
        get => (WheelAspectItem[])GetValue(InterChartAspectsProperty);
        set => SetValue(InterChartAspectsProperty, value);
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
        var type        = WheelProjection.Effective(DrawingType, RadixData);
        var innerRadius = fullRadius * RadixScale * WheelRenderer.ScaleFactor(type);
        var edgeRadius  = innerRadius * WheelRenderer.ContentFraction(type);
        var data        = WheelProjection.ProjectChart(RadixData, type);
        var theme       = Theme;
        var asc         = RadixData.AscendantLongitude;
        var cusps       = RadixData.CuspLongitudes;
        var outerItems  = WheelProjection.ProjectItems(TransitItems, type, asc, cusps);

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        // Transit ring background (annulus between innerRadius and transitBackgroundFraction × fullRadius)
        WheelRenderer.DrawRingBackground(ctx, type, center, fullRadius * TransitBackgroundFraction, theme);

        // Radix wheel (scaled to innerRadius), in the configured drawing type
        WheelRenderer.Render(type, ctx, center, innerRadius, data, theme, ShowAspects);
        WheelRenderer.DrawBoundaryIfNeeded(ctx, type, center, innerRadius * WheelRenderer.RingStartFraction(type),
                                           fullRadius, theme);

        // Transit ring (glyphs, texts, connect lines)
        DrawTransitConnectLines(ctx, center, fullRadius, edgeRadius, outerItems, theme);
        DrawTransitGlyphs(ctx, center, fullRadius, innerRadius, outerItems, theme);
        DrawTransitTexts(ctx, center, fullRadius, innerRadius, outerItems, theme);

        // The 90° and 45° dials do not show aspect lines
        if (ShowAspects && !WheelProjection.HasNoAspects(type))
            DrawInterChartAspects(ctx, center, fullRadius, edgeRadius,
                WheelProjection.ProjectAspects(InterChartAspects, type, asc, cusps), theme);
    }

    // ── Inter-chart (synastry) aspect lines ─────────────────────────────────────

    /// <summary>Builds inter-chart aspect items connecting inner-ring plot angles (by factor,
    /// from RadixData.PlanetItems) to outer-ring plot angles (by factor, from the outer item list),
    /// mirroring WheelPlotDataBuilder's intra-chart aspect-item construction.</summary>
    public static WheelAspectItem[] BuildInterChartAspects(
        IReadOnlyList<FoundAspect> foundAspects,
        WheelPlotItem[] innerItems,
        WheelPlotItem[] outerItems,
        AspectConfig aspectConfig)
    {
        if (foundAspects.Count == 0) return [];

        var innerAngles = new Dictionary<Factors, double>();
        foreach (var item in innerItems)
            innerAngles[item.Factor] = item.MundaneAngle;

        var outerAngles = new Dictionary<Factors, double>();
        foreach (var item in outerItems)
            outerAngles[item.Factor] = item.MundaneAngle;

        var colorMap = new Dictionary<Aspects, Color>();
        foreach (var setting in aspectConfig.Settings)
        {
            var c = setting.Color;
            colorMap[setting.Aspect] = Color.FromArgb(
                (byte)(c.Opacity * 255),
                (byte)(c.Red   * 255),
                (byte)(c.Green * 255),
                (byte)(c.Blue  * 255));
        }

        var result = new List<WheelAspectItem>();
        foreach (var found in foundAspects)
        {
            if (!innerAngles.TryGetValue(found.Factor1, out var angle1)) continue;
            if (!outerAngles.TryGetValue(found.Factor2, out var angle2)) continue;

            colorMap.TryGetValue(found.Aspect, out var color);
            var exactness = found.MaxOrb > 0
                ? Math.Max(0.0, 1.0 - found.Orb / found.MaxOrb)
                : 1.0;

            result.Add(new WheelAspectItem(angle1, angle2, color, exactness, found.Aspect));
        }

        return [.. result];
    }

    private static void DrawInterChartAspects(DrawingContext ctx, Point center,
                                              double fullRadius, double signRingR,
                                              WheelAspectItem[] items, WheelTheme theme)
    {
        if (items.Length == 0) return;

        var transitR   = fullRadius * TransitConnectStart;
        var maxStroke  = WheelMetrics.StrokeWidth(WheelMetrics.AspectLineFraction, fullRadius);
        var minStroke  = Math.Max(0.5, maxStroke * 0.15);

        foreach (var item in items)
        {
            var p1        = WheelGeometry.PointOnCircle(item.Angle1, signRingR, center);
            var p2        = WheelGeometry.PointOnCircle(item.Angle2, transitR, center);
            var lineWidth = minStroke + (maxStroke - minStroke) * item.Exactness;
            var ac        = theme.AspectLineColor(item.Color);
            var drawColor = Color.FromArgb((byte)(WheelMetrics.AspectOpacity * 255), ac.R, ac.G, ac.B);
            var pen       = new Pen(new SolidColorBrush(drawColor), lineWidth);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // ── Transit ring drawing ─────────────────────────────────────────────────

    private static void DrawTransitConnectLines(DrawingContext ctx, Point center,
                                                double fullRadius, double signRingR,
                                                WheelPlotItem[] items, WheelTheme theme)
    {
        var startR     = fullRadius * TransitConnectStart;
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

    private static void DrawTransitGlyphs(DrawingContext ctx, Point center,
                                          double fullRadius, double innerRadius,
                                          WheelPlotItem[] items, WheelTheme theme)
    {
        var r       = fullRadius * TransitGlyphFraction;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, innerRadius);
        var typeface = WheelMetrics.GlyphTypeface;
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in items)
        {
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, typeface, brush);
        }
    }

    private static void DrawTransitTexts(DrawingContext ctx, Point center,
                                         double fullRadius, double innerRadius,
                                         WheelPlotItem[] items, WheelTheme theme)
    {
        var r       = fullRadius * TransitTextFraction;
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
