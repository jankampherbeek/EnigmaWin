// ZodiacDivisionsWheelCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;

/// <summary>
/// Renders the standard radix wheel scaled to 70% with an outer ring showing
/// sign, decan, dodecatemoria and bound glyphs for each factor.
/// </summary>
public sealed class ZodiacDivisionsWheelCanvas : FrameworkElement
{
    // Main wheel is drawn at 70% of the available radius; the remaining 30% is the division ring.
    private const double RadixScale    = 0.70;
    private const double OuterFraction = 0.96;

    public static readonly DependencyProperty PlotDataProperty =
        DependencyProperty.Register(nameof(PlotData), typeof(WheelPlotData), typeof(ZodiacDivisionsWheelCanvas),
            new FrameworkPropertyMetadata(WheelPlotData.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MarksProperty =
        DependencyProperty.Register(nameof(Marks), typeof(ZodiacDivisionMark[]), typeof(ZodiacDivisionsWheelCanvas),
            new FrameworkPropertyMetadata(Array.Empty<ZodiacDivisionMark>(), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(ZodiacDivisionsWheelCanvas),
            new FrameworkPropertyMetadata(WheelTheme.Color, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowAspectsProperty =
        DependencyProperty.Register(nameof(ShowAspects), typeof(bool), typeof(ZodiacDivisionsWheelCanvas),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public WheelPlotData PlotData
    {
        get => (WheelPlotData)GetValue(PlotDataProperty);
        set => SetValue(PlotDataProperty, value);
    }

    public ZodiacDivisionMark[] Marks
    {
        get => (ZodiacDivisionMark[])GetValue(MarksProperty);
        set => SetValue(MarksProperty, value);
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

    protected override void OnRender(DrawingContext ctx)
    {
        base.OnRender(ctx);

        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var fullRadius  = Math.Min(w, h) / 2.0;
        var innerRadius = fullRadius * RadixScale;   // main wheel radius
        var center      = new Point(w / 2.0, h / 2.0);
        var data        = PlotData;
        var theme       = Theme;
        var asc         = data.AscendantLongitude;

        ctx.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

        // Outer ring background circle
        var ringOuterR = fullRadius * OuterFraction;
        var bg = new SolidColorBrush(theme.OuterCircleBackground);
        ctx.DrawEllipse(bg, null, center, ringOuterR, ringOuterR);

        // Standard wheel drawn at innerRadius
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

        // Division ring
        DrawDivisionMarks(ctx, center, fullRadius, innerRadius, Marks, theme);
    }

    private static void DrawDivisionMarks(
        DrawingContext ctx, Point center,
        double fullRadius, double innerRadius,
        ZodiacDivisionMark[] marks, WheelTheme theme)
    {
        var ringOuter = fullRadius * OuterFraction;
        var ringWidth = ringOuter - innerRadius;
        var glyphSize = ringWidth * 0.16;
        var strokeW   = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, fullRadius) * 1.5;

        var rBound   = innerRadius + ringWidth * 0.12;
        var rDodecat = innerRadius + ringWidth * 0.37;
        var rDecan   = innerRadius + ringWidth * 0.62;
        var rSign    = innerRadius + ringWidth * 0.87;

        var glyphBrush = new SolidColorBrush(theme.PlanetGlyph);
        var tickPen    = new Pen(glyphBrush, strokeW);
        var typeface   = WheelMetrics.GlyphTypeface;

        foreach (var mark in marks)
        {
            var angle  = mark.MundaneAngle;
            var rotDeg = angle <= 180.0 ? (90.0 - angle) : (270.0 - angle);

            // Tick from inner wheel edge outward to ring boundary
            var tickInner = WheelGeometry.PointOnCircle(angle, innerRadius * WheelMetrics.OuterSign, center);
            var tickOuter = WheelGeometry.PointOnCircle(angle, innerRadius, center);
            ctx.DrawLine(tickPen, tickInner, tickOuter);

            DrawDivisionGlyph(ctx, mark.SignGlyph,    angle, rSign,    center, rotDeg, glyphSize, typeface, glyphBrush);
            DrawDivisionGlyph(ctx, mark.DecanGlyph,   angle, rDecan,   center, rotDeg, glyphSize, typeface, glyphBrush);
            DrawDivisionGlyph(ctx, mark.DodecatGlyph, angle, rDodecat, center, rotDeg, glyphSize, typeface, glyphBrush);
            DrawDivisionGlyph(ctx, mark.BoundGlyph,   angle, rBound,   center, rotDeg, glyphSize, typeface, glyphBrush);
        }
    }

    private static void DrawDivisionGlyph(
        DrawingContext ctx, string glyph,
        double angle, double radius, Point center,
        double rotDeg, double fontSize,
        Typeface typeface, Brush brush)
    {
        var pt = WheelGeometry.PointOnCircle(angle, radius, center);
        var ft = new FormattedText(glyph, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, fontSize, brush, 1.0);

        var rad       = rotDeg * Math.PI / 180.0;
        var cos       = Math.Cos(rad);
        var sin       = Math.Sin(rad);
        var transform = new MatrixTransform(new Matrix(cos, sin, -sin, cos, pt.X, pt.Y));
        ctx.PushTransform(transform);
        ctx.DrawText(ft, new Point(-ft.Width / 2, -ft.Height / 2));
        ctx.Pop();
    }
}
