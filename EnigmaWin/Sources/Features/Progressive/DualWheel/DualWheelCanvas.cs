// DualWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>Ecliptic longitudes of the ring chart's own house cusps (index 0 = cusp 1). Each cusp is
    /// drawn in the ring like a factor: a connect line with the cusp number and its position. A cusp that
    /// coincides with a ring Ascendant or MC is skipped, as that point is already shown. Dials have no
    /// houses, so no cusps are drawn there. Empty by default; only the solar return passes these.</summary>
    public static readonly DependencyProperty RingCuspLongitudesProperty =
        DependencyProperty.Register(nameof(RingCuspLongitudes), typeof(double[]), typeof(DualWheelCanvas),
            new PropertyMetadata(Array.Empty<double>(), OnVisualPropertyChanged));

    public double[] RingCuspLongitudes
    {
        get => (double[])GetValue(RingCuspLongitudesProperty);
        set => SetValue(RingCuspLongitudesProperty, value);
    }

    /// <summary>Maximum distance in degrees for a cusp to count as coinciding with the Ascendant or MC.</summary>
    private const double CuspCoincidenceTolerance = 0.001;

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
        // Overlapping ring glyphs are moved apart; the connect line then runs obliquely to the exact position.
        var (ringItems, ringCusps) = ResolveRingOverlap(outerItems, BuildRingCusps(type, asc, cusps));
        outerItems = ringItems;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        // Transit ring background (annulus between innerRadius and transitBackgroundFraction × fullRadius)
        WheelRenderer.DrawRingBackground(ctx, type, center, fullRadius * TransitBackgroundFraction, theme);

        // Radix wheel (scaled to innerRadius), in the configured drawing type
        WheelRenderer.Render(type, ctx, center, innerRadius, data, theme, ShowAspects);
        WheelRenderer.DrawBoundaryIfNeeded(ctx, type, center, innerRadius * WheelRenderer.RingStartFraction(type),
                                           fullRadius, theme);

        // Transit ring (glyphs, texts, connect lines), plus the ring chart's own cusps if any
        DrawTransitConnectLines(ctx, center, fullRadius, edgeRadius,
            [.. outerItems.Select(i => (i.PlotAngle, i.MundaneAngle)),
             .. ringCusps.Select(c => (c.PlotAngle, c.MundaneAngle))], theme);
        DrawTransitGlyphs(ctx, center, fullRadius, innerRadius, outerItems, theme);
        DrawRingCuspNumbers(ctx, center, fullRadius, innerRadius, ringCusps, theme);
        DrawTransitTexts(ctx, center, fullRadius, innerRadius,
            [.. outerItems.Select(i => (i.PlotAngle, i.PositionText))], theme.PlanetText);
        DrawTransitTexts(ctx, center, fullRadius, innerRadius,
            [.. ringCusps.Select(c => (c.PlotAngle, c.PositionText))], theme.CuspText);

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

    // ── Ring cusps ───────────────────────────────────────────────────────────

    /// <summary>A house cusp of the ring chart, drawn in the ring as a numbered position.</summary>
    /// <param name="MundaneAngle">Angle on the wheel of the exact cusp position; the end point of the connect line.</param>
    /// <param name="PlotAngle">Angle of the cusp number after anti-overlap adjustment.</param>
    private sealed record RingCusp(int Number, string PositionText, double MundaneAngle, double PlotAngle);

    /// <summary>Ring cusps that are not already shown as the ring Ascendant or MC. Dials have no houses.</summary>
    private RingCusp[] BuildRingCusps(DrawingTypes type, double asc, double[] radixCusps)
    {
        if (WheelProjection.IsDial(type) || RingCuspLongitudes.Length == 0) return [];

        var shownAngles = TransitItems
            .Where(i => i.Factor is Factors.Ascendant or Factors.Mc)
            .Select(i => i.EclipticLongitude)
            .ToList();

        var result = new List<RingCusp>();
        for (var index = 0; index < RingCuspLongitudes.Length; index++)
        {
            var longitude = RingCuspLongitudes[index];
            var coincides = shownAngles.Any(shown =>
            {
                var diff = Math.Abs(WheelGeometry.Normalise(longitude - shown));
                return Math.Min(diff, 360.0 - diff) < CuspCoincidenceTolerance;
            });
            if (coincides) continue;
            var angle = WheelProjection.LongitudeToAngle(type, longitude, asc, radixCusps);
            result.Add(new RingCusp(index + 1, DrawCusps.CuspPositionText(longitude), angle, angle));
        }
        return [.. result];
    }

    /// <summary>Resolves glyph overlap for ring factors and ring cusps together, so neither factor glyphs
    /// nor cusp numbers overlap. Starts from the exact (mundane) angles, so earlier adjustments are ignored.</summary>
    private static (WheelPlotItem[] Items, RingCusp[] Cusps) ResolveRingOverlap(WheelPlotItem[] items,
                                                                                 RingCusp[] cusps)
    {
        var plotAngles = GlyphOverlapResolver.ResolvedPlotAngles(
            [.. items.Select(i => i.MundaneAngle), .. cusps.Select(c => c.MundaneAngle)]);
        var resolvedItems = items.Select((item, i) => item.WithPlotAngle(plotAngles[i])).ToArray();
        var resolvedCusps = cusps.Select((cusp, i) => cusp with { PlotAngle = plotAngles[items.Length + i] })
                                 .ToArray();
        return (resolvedItems, resolvedCusps);
    }

    /// <summary>Ring cusp numbers at the glyph radius, in the cusp text colour.</summary>
    private static void DrawRingCuspNumbers(DrawingContext ctx, Point center,
                                            double fullRadius, double innerRadius,
                                            RingCusp[] cusps, WheelTheme theme)
    {
        var r        = fullRadius * TransitGlyphFraction;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.CardinalFontFraction, innerRadius);
        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold,
                                    FontStretches.Normal);
        var brush    = new SolidColorBrush(theme.CuspText);

        foreach (var cusp in cusps)
        {
            var pt = WheelGeometry.PointOnCircle(cusp.PlotAngle, r, center);
            DrawTextCentered(ctx, cusp.Number.ToString(CultureInfo.InvariantCulture), pt, fontSize, typeface, brush);
        }
    }

    // ── Transit ring drawing ─────────────────────────────────────────────────

    /// <summary>Connect lines run from just inside the ring glyph (plot angle) down to the outer edge
    /// of the radix wheel (mundane angle).</summary>
    private static void DrawTransitConnectLines(DrawingContext ctx, Point center,
                                                double fullRadius, double signRingR,
                                                (double PlotAngle, double MundaneAngle)[] angles,
                                                WheelTheme theme)
    {
        var startR     = fullRadius * TransitConnectStart;
        var stroke     = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, fullRadius);
        var pc         = theme.PlanetConnectLine;
        var alpha      = (byte)(WheelMetrics.ConnectLineOpacity * 255);
        var color      = Color.FromArgb(alpha, pc.R, pc.G, pc.B);
        var pen        = new Pen(new SolidColorBrush(color), stroke);

        foreach (var angle in angles)
        {
            var p1 = WheelGeometry.PointOnCircle(angle.PlotAngle,    startR,    center);
            var p2 = WheelGeometry.PointOnCircle(angle.MundaneAngle, signRingR, center);
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

    /// <summary>Ring position texts (factors and cusps) rotated tangentially.</summary>
    private static void DrawTransitTexts(DrawingContext ctx, Point center,
                                         double fullRadius, double innerRadius,
                                         (double PlotAngle, string PositionText)[] items, Color color)
    {
        var r       = fullRadius * TransitTextFraction;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, innerRadius);
        var typeface = new Typeface("Segoe UI");
        var brush    = new SolidColorBrush(color);

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
