// DrawDial45.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>All static rendering methods for the 45° dial wheel.</summary>
public static class DrawDial45
{
    private const double FLabelOuter = 0.99;
    private const double FLabelInner = 0.90;
    private const double FDeg1Inner  = 0.87;
    private const double FTickInner  = 0.82;

    private const double FPlanetGlyph = 0.78;
    private const double FPlanetText  = 0.59;

    private const double FCrossArm = 0.04;

    private const double FTickHalfDeg  = 0.012;
    private const double FTickWholeDeg = 0.030;
    private const double FTick5Deg     = 0.080;

    public static void DrawBackground(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FLabelOuter;
        var rInner = outerR * FLabelInner;
        ctx.DrawEllipse(new SolidColorBrush(theme.OuterCircleBackground), null, center, rOuter, rOuter);
        ctx.DrawEllipse(Brushes.White, null, center, rInner, rInner);
    }

    public static void DrawRingStrokes(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        foreach (var frac in new[] { FLabelOuter, FLabelInner, FDeg1Inner, FTickInner })
        {
            var r = outerR * frac;
            ctx.DrawEllipse(null, pen, center, r, r);
        }
    }

    public static void DrawDegreeLabels(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var labelR   = outerR * ((FLabelInner + FLabelOuter) / 2.0);
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 1.3, outerR);
        var brush    = new SolidColorBrush(theme.PlanetText);

        for (var i = 0; i < 45; i += 5)
        {
            var visualAngle = i * 8.0;
            var pt          = WheelGeometry.PointOnCircle(visualAngle, labelR, center);
            DrawTextCentered(ctx, $"{i}", pt, fontSize, null, brush);
        }
    }

    public static void Draw1DegTicks(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FLabelInner;
        var rInner = outerR * FDeg1Inner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 0.8, outerR);
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), sw);

        for (var i = 0; i < 45; i++)
        {
            var angle = i * 8.0;
            var p1    = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2    = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawHalfDegTicks(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FDeg1Inner;
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);

        for (var i = 0; i < 90; i++)
        {
            var angle = i * 4.0;
            double tickFrac;
            if      (i % 10 == 0) tickFrac = FTick5Deg;
            else if (i % 2  == 0) tickFrac = FTickWholeDeg;
            else                  tickFrac = FTickHalfDeg;

            var rInner = rOuter - outerR * tickFrac;
            var p1     = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2     = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawCenterCross(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var arm = outerR * FCrossArm;
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);

        ctx.DrawLine(pen, new Point(center.X - arm, center.Y), new Point(center.X + arm, center.Y));
        ctx.DrawLine(pen, new Point(center.X, center.Y - arm), new Point(center.X, center.Y + arm));
    }

    public static void DrawConnectLines(DrawingContext ctx, Point center, double outerR,
                                        WheelPlotData data, WheelTheme theme)
    {
        var glyphR = outerR * FPlanetGlyph;
        var ringR  = outerR * FTickInner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, outerR);
        var color  = theme.PlanetConnectLine;
        var pen    = new Pen(new SolidColorBrush(
            Color.FromArgb(
                (byte)(WheelMetrics.ConnectLineOpacity * 255),
                color.R, color.G, color.B)), sw);

        foreach (var item in data.PlanetItems)
        {
            var p1 = WheelGeometry.PointOnCircle(item.PlotAngle,    glyphR, center);
            var p2 = WheelGeometry.PointOnCircle(item.MundaneAngle, ringR,  center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawPlanetGlyphs(DrawingContext ctx, Point center, double outerR,
                                         WheelPlotData data, WheelTheme theme)
    {
        var r        = outerR * FPlanetGlyph;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PlanetGlyphFontFraction, outerR);
        var brush    = new SolidColorBrush(theme.PlanetGlyph);

        foreach (var item in data.PlanetItems)
        {
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, WheelMetrics.GlyphTypeface, brush);
        }
    }

    public static void DrawPlanetTexts(DrawingContext ctx, Point center, double outerR,
                                        WheelPlotData data, WheelTheme theme)
    {
        var r        = outerR * FPlanetText;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction, outerR);
        var brush    = new SolidColorBrush(theme.PlanetText);

        foreach (var item in data.PlanetItems)
        {
            var pt        = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            var signGlyph = GlyphSelector.GetGlyphForSign(LongitudeToSign(item.EclipticLongitude));
            DrawRotatedTextWithGlyph(ctx, item.PositionText, signGlyph, pt, item.PlotAngle, fontSize, brush);
        }
    }

    private static Signs LongitudeToSign(double longitude)
    {
        var signIndex = (int)(longitude / 30.0) % 12;
        return (Signs)(signIndex + 1);
    }

    private static void DrawTextCentered(DrawingContext ctx, string text, Point center,
                                          double fontSize, Typeface? typeface, Brush brush)
    {
        typeface ??= new Typeface("Segoe UI");
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush,
            1.0);

        ctx.DrawText(formatted, new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
    }

    private static void DrawRotatedTextWithGlyph(DrawingContext ctx, string posText, string signGlyph,
                                                  Point pt, double angleDeg, double fontSize, Brush brush)
    {
        double rotDeg = angleDeg < 180.0 ? 90.0 - angleDeg : 270.0 - angleDeg;

        var posFormatted = new FormattedText(
            posText,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            fontSize,
            brush,
            1.0);

        var glyphFormatted = new FormattedText(
            " " + signGlyph,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            WheelMetrics.GlyphTypeface,
            fontSize,
            brush,
            1.0);

        var totalWidth = posFormatted.Width + glyphFormatted.Width;
        var rad        = rotDeg * Math.PI / 180.0;
        var cos        = Math.Cos(rad);
        var sin        = Math.Sin(rad);
        var transform  = new MatrixTransform(new Matrix(cos, sin, -sin, cos, pt.X, pt.Y));
        ctx.PushTransform(transform);

        double startX = angleDeg < 180.0 ? -totalWidth : 0.0;
        double baseY  = -posFormatted.Height / 2;

        ctx.DrawText(posFormatted,   new Point(startX,                     baseY));
        ctx.DrawText(glyphFormatted, new Point(startX + posFormatted.Width, baseY));

        ctx.Pop();
    }
}
