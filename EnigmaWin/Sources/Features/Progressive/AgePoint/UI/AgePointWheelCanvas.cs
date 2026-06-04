// AgePointWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

/// <summary>
/// Renders the radix wheel at 78% scale with an outer ring showing either a single
/// Age Point arrow (PositionsForEvent mode) or overview tick-marks with year labels (Overview mode).
/// </summary>
public class AgePointWheelCanvas : FrameworkElement
{
    private const double RadixScale     = 0.78;
    private const double RingOuterFrac  = 0.864;
    private const double ArrowShaftFrac = 0.830;
    private const double TickOuterFrac  = 0.745;
    private const double TickInnerFrac  = 0.694;
    private const double LabelFrac      = 0.800;

    // ── Dependency properties ────────────────────────────────────────────────

    public static readonly DependencyProperty RadixDataProperty =
        DependencyProperty.Register(nameof(RadixData), typeof(WheelPlotData), typeof(AgePointWheelCanvas),
            new PropertyMetadata(WheelPlotData.Empty, OnVisualChanged));

    public static readonly DependencyProperty ApLongitudeProperty =
        DependencyProperty.Register(nameof(ApLongitude), typeof(double?), typeof(AgePointWheelCanvas),
            new PropertyMetadata(null, OnVisualChanged));

    public static readonly DependencyProperty OverviewMarksProperty =
        DependencyProperty.Register(nameof(OverviewMarks), typeof(AgePointWheelMark[]), typeof(AgePointWheelCanvas),
            new PropertyMetadata(Array.Empty<AgePointWheelMark>(), OnVisualChanged));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(AgePointWheelCanvas),
            new PropertyMetadata(WheelTheme.Color, OnVisualChanged));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(AgePointWheelCanvas),
            new PropertyMetadata(true, OnVisualChanged));

    public WheelPlotData         RadixData      { get => (WheelPlotData)GetValue(RadixDataProperty);         set => SetValue(RadixDataProperty, value); }
    public double?               ApLongitude    { get => (double?)GetValue(ApLongitudeProperty);             set => SetValue(ApLongitudeProperty, value); }
    public AgePointWheelMark[]   OverviewMarks  { get => (AgePointWheelMark[])GetValue(OverviewMarksProperty); set => SetValue(OverviewMarksProperty, value); }
    public WheelTheme            Theme          { get => (WheelTheme)GetValue(ThemeProperty);               set => SetValue(ThemeProperty, value); }
    public bool                  ShowAspects    { get => (bool)GetValue(ShowAspectsProperty);               set => SetValue(ShowAspectsProperty, value); }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((AgePointWheelCanvas)d).InvalidateVisual();

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

        DrawRingBackground(ctx, center, fullRadius, RingOuterFrac, theme);

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

        var marks = OverviewMarks;
        if (marks is { Length: > 0 })
            DrawOverviewMarks(ctx, center, fullRadius, innerRadius, marks, theme);
        else if (ApLongitude.HasValue)
            DrawArrow(ctx, center, fullRadius, innerRadius, asc, ApLongitude.Value, theme);
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
        var angle    = WheelGeometry.MundaneAngle(longitude, asc);
        var ringOuter = fullRadius * RingOuterFrac;
        var zodiacR  = innerRadius * WheelMetrics.OuterSign;

        var tip   = WheelGeometry.PointOnCircle(angle, zodiacR, center);
        var shaft = WheelGeometry.PointOnCircle(angle, ringOuter, center);

        var shaftColor = theme.PlanetGlyph;
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(80, shaftColor.R, shaftColor.G, shaftColor.B)), 2.0);
        ctx.DrawLine(pen, shaft, tip);

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

        if (ApLongitude.HasValue)
        {
            var (dms, sign, ok) = PositionInDegreesConversion.DoubleToDmsSign(longitude);
            var dmsText      = ok ? dms : FormatDms(longitude);
            var signGlyph    = (ok && sign.HasValue) ? GlyphSelector.GetGlyphForSign(sign.Value) : string.Empty;

            var fontSize      = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, innerRadius);
            var glyphFontSize = fontSize;
            var textTypeface  = new Typeface("Segoe UI");
            var glyphTypeface = WheelMetrics.GlyphTypeface;
            var brush         = new SolidColorBrush(theme.PlanetText);

            var rotDeg = angle < 180.0 ? 90.0 - angle : 270.0 - angle;

            var dmsR   = fullRadius * ArrowShaftFrac - fontSize * 1.5;
            var glyphR = fullRadius * ArrowShaftFrac + fontSize * 1.0;

            var dmsPt   = WheelGeometry.PointOnCircle(angle, dmsR,   center);
            var glyphPt = WheelGeometry.PointOnCircle(angle, glyphR, center);

            if (!string.IsNullOrEmpty(signGlyph))
                DrawRotatedText(ctx, signGlyph, glyphPt, rotDeg, glyphFontSize, glyphTypeface, brush);
            DrawRotatedText(ctx, dmsText, dmsPt, rotDeg, fontSize, textTypeface, brush);
        }
    }

    // ── Overview tick marks ───────────────────────────────────────────────────

    private static void DrawOverviewMarks(DrawingContext ctx, Point center, double fullRadius,
                                           double innerRadius,
                                           AgePointWheelMark[] marks, WheelTheme theme)
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
            var rotDeg  = angle < 180.0 ? 90.0 - angle : 270.0 - angle;
            DrawRotatedText(ctx, mark.Label, labelPt, rotDeg, fontSize, typeface, brush);
        }
    }

    // ── Text helpers ──────────────────────────────────────────────────────────

    private static string FormatDms(double longitude)
    {
        var withinSign = longitude % 30.0;
        var deg = (int)withinSign;
        var min = (int)((withinSign - deg) * 60);
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
