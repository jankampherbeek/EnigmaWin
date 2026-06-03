// LtsWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;

/// <summary>
/// Renders the radix wheel at 78% scale with an outer ring showing either a single
/// LTS arrow (PositionsForEvent mode) or overview tick-marks (Overview mode).
/// </summary>
public class LtsWheelCanvas : FrameworkElement
{
    private const double RadixScale      = 0.78;
    private const double RingOuterFrac   = 0.864;  // outer edge of the LTS ring
    private const double ArrowShaftFrac  = 0.830;  // shaft starts here (mid-ring)
    // Tick anchored at zodiac outer circle, extending outward 30% of full span
    private const double TickOuterFrac   = 0.745;  // 0.694 + 30% of (0.864 - 0.694)
    private const double TickInnerFrac   = 0.694;  // innerRadius * OuterSign = 0.78 * 0.89
    private const double LabelFrac       = 0.800;  // ~4 char-widths outside the tick

    // ── Dependency properties ────────────────────────────────────────────────

    public static readonly DependencyProperty RadixDataProperty =
        DependencyProperty.Register(nameof(RadixData), typeof(WheelPlotData), typeof(LtsWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualChanged));

    public static readonly DependencyProperty LtsLongitudeProperty =
        DependencyProperty.Register(nameof(LtsLongitude), typeof(double?), typeof(LtsWheelCanvas),
            new PropertyMetadata(null, OnVisualChanged));

    public static readonly DependencyProperty OverviewMarksProperty =
        DependencyProperty.Register(nameof(OverviewMarks), typeof(LtsWheelMark[]), typeof(LtsWheelCanvas),
            new PropertyMetadata(Array.Empty<LtsWheelMark>(), OnVisualChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(LtsWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualChanged));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(LtsWheelCanvas),
            new PropertyMetadata(true, OnVisualChanged));

    public WheelPlotData  RadixData      { get => (WheelPlotData)GetValue(RadixDataProperty);   set => SetValue(RadixDataProperty, value); }
    public double?        LtsLongitude   { get => (double?)GetValue(LtsLongitudeProperty);       set => SetValue(LtsLongitudeProperty, value); }
    public LtsWheelMark[] OverviewMarks  { get => (LtsWheelMark[])GetValue(OverviewMarksProperty); set => SetValue(OverviewMarksProperty, value); }
    public WheelTheme     Theme          { get => (WheelTheme)GetValue(ThemeProperty);           set => SetValue(ThemeProperty, value); }
    public bool           ShowAspects    { get => (bool)GetValue(ShowAspectsProperty);           set => SetValue(ShowAspectsProperty, value); }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((LtsWheelCanvas)d).InvalidateVisual();

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

        // LTS ring background
        DrawRingBackground(ctx, center, fullRadius, RingOuterFrac, theme);

        // Radix wheel
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

        // LTS overlay
        var marks = OverviewMarks;
        if (marks is { Length: > 0 })
            DrawOverviewMarks(ctx, center, fullRadius, innerRadius, asc, marks, theme);
        else if (LtsLongitude.HasValue)
            DrawArrow(ctx, center, fullRadius, innerRadius, asc, LtsLongitude.Value, theme);
    }

    // ── Ring background ───────────────────────────────────────────────────────

    private static void DrawRingBackground(DrawingContext ctx, Point center,
                                            double fullRadius, double outerFrac,
                                            WheelTheme theme)
    {
        var r = fullRadius * outerFrac;
        ctx.DrawEllipse(new SolidColorBrush(theme.OuterCircleBackground), null, center, r, r);
    }

    // ── Arrow (PositionsForEvent) ─────────────────────────────────────────────

    private void DrawArrow(DrawingContext ctx, Point center, double fullRadius,
                            double innerRadius, double asc, double longitude,
                            WheelTheme theme)
    {
        var angle     = WheelGeometry.MundaneAngle(longitude, asc);
        var ringOuter = fullRadius * RingOuterFrac;
        var zodiacR   = innerRadius * WheelMetrics.OuterSign;

        var tip   = WheelGeometry.PointOnCircle(angle, zodiacR, center);
        var shaft = WheelGeometry.PointOnCircle(angle, ringOuter, center);

        // Shaft line — semi-transparent so the label remains readable
        var shaftColor = theme.PlanetGlyph;
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(80, shaftColor.R, shaftColor.G, shaftColor.B)), 2.0);
        ctx.DrawLine(pen, shaft, tip);

        // Arrowhead — filled triangle at tip
        const double wingDeg = 4.0;
        var leftWing  = WheelGeometry.PointOnCircle(angle - wingDeg, zodiacR * 1.05, center);
        var rightWing = WheelGeometry.PointOnCircle(angle + wingDeg, zodiacR * 1.05, center);

        var geo = new StreamGeometry();
        using (var sgc = geo.Open())
        {
            sgc.BeginFigure(tip, isFilled: true, isClosed: true);
            sgc.LineTo(leftWing,  isStroked: false, isSmoothJoin: false);
            sgc.LineTo(rightWing, isStroked: false, isSmoothJoin: false);
        }
        geo.Freeze();
        ctx.DrawGeometry(new SolidColorBrush(theme.PlanetGlyph), null, geo);

        // Label alongside the shaft — DMS + sign glyph, direction depends on wheel half
        if (LtsLongitude.HasValue)
        {
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var dmsText      = ok ? dms : FormatDms(longitude);
            var signGlyph    = (ok && sign.HasValue) ? GlyphSelector.GetGlyphForSign(sign.Value) : string.Empty;

            var fontSize      = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, innerRadius);
            var glyphFontSize = fontSize;
            var textTypeface  = new Typeface("Segoe UI");
            var glyphTypeface = WheelMetrics.GlyphTypeface;
            var brush         = new SolidColorBrush(theme.PlanetText);

            // Right half (angle 0–180): reads center→outside  → rotDeg = 90 - angle
            // Left half  (angle 180–360): reads outside→center → rotDeg = 270 - angle
            var rotDeg     = angle < 180.0 ? 90.0 - angle : 270.0 - angle;
            var isRightHalf = angle < 180.0;

            // DMS toward center; glyph shifted outward so the two don't overlap
            var dmsR   = fullRadius * ArrowShaftFrac - fontSize * 1.5;
            var glyphR = fullRadius * ArrowShaftFrac + fontSize * 1.0;

            var dmsPt   = WheelGeometry.PointOnCircle(angle, dmsR,   center);
            var glyphPt = WheelGeometry.PointOnCircle(angle, glyphR, center);

            // Draw glyph first (behind), then DMS text on top
            if (!string.IsNullOrEmpty(signGlyph))
                DrawRotatedText(ctx, signGlyph, glyphPt, rotDeg, glyphFontSize, glyphTypeface, brush);
            DrawRotatedText(ctx, dmsText, dmsPt, rotDeg, fontSize, textTypeface, brush);
        }
    }

    // ── Overview tick marks ───────────────────────────────────────────────────

    private void DrawOverviewMarks(DrawingContext ctx, Point center, double fullRadius,
                                    double innerRadius, double asc,
                                    LtsWheelMark[] marks, WheelTheme theme)
    {
        var pen      = new Pen(new SolidColorBrush(theme.PlanetGlyph), 1.0);
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 0.8, innerRadius);
        var typeface = new Typeface("Segoe UI");
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var mark in marks)
        {
            var angle   = mark.MundaneAngle;
            var outerPt = WheelGeometry.PointOnCircle(angle, fullRadius * TickOuterFrac, center);
            var innerPt = WheelGeometry.PointOnCircle(angle, fullRadius * TickInnerFrac, center);
            ctx.DrawLine(pen, outerPt, innerPt);

            var labelPt = WheelGeometry.PointOnCircle(angle, fullRadius * LabelFrac, center);

            // Ages >= 4 and Month 0 read from inside to outside (flipped 180°)
            var flipText = ShouldFlipLabel(mark);
            var rotDeg   = flipText ? 270.0 - angle : 90.0 - angle;
            DrawRotatedText(ctx, mark.Label, labelPt, rotDeg, fontSize, typeface, brush);
        }
    }

    // Ages >= 4 and Month 0 are written from inside outward
    private static bool ShouldFlipLabel(LtsWheelMark mark)
    {
        if (mark.IsMonth)
        {
            // label is e.g. "Month 0", "Month 1", ... — flip only Month 0
            return mark.Label.EndsWith(" 0", StringComparison.Ordinal);
        }
        // label is "Age 0", "Age 1", ... — flip ages >= 4
        var parts = mark.Label.Split(' ');
        return parts.Length == 2
            && int.TryParse(parts[1], out var age)
            && age >= 4;
    }

    // ── Text helpers ──────────────────────────────────────────────────────────

    private static string FormatDms(double longitude)
    {
        var withinSign = longitude % 30.0;
        var deg  = (int)withinSign;
        var min  = (int)((withinSign - deg) * 60);
        return $"{deg}°{min:D2}'";
    }

    private static void DrawRotatedText(DrawingContext ctx, string text, Point pt,
                                         double rotDeg, double fontSize,
                                         Typeface typeface, Brush brush)
    {
        var ft = new FormattedText(
            text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
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
