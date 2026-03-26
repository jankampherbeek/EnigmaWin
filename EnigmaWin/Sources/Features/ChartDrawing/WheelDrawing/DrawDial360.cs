// DrawDial360.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026
//
// Static drawing helpers for the Ebertin-style 360° dial wheel.
//
// Layout from centre outward:
//   1. Planet area      (0 … fTickInner)          — glyphs + degree texts + centre cross
//   2. 1° degree ring   (fTickInner … fDeg10Inner) — 360 ticks, varying length
//   3. 10° ring         (fDeg10Inner … fSignInner) — 36 ticks spanning full ring width
//   4. Sign ring        (fSignInner … fSignOuter)  — element colours, sign glyphs, degree labels
//
// Angle convention: 0° = top, counter-clockwise (same as WheelGeometry.PointOnCircle).
// Dial angle = ecliptic longitude (Aries at top, fixed ring — no ascendant rotation).

using System;
using Avalonia;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>All static rendering methods for the Dial360 wheel.</summary>
public static class DrawDial360
{
    // MARK: - Layout constants (fraction of outerRadius)

    private const double FSignOuter  = 0.99;   // outer boundary of sign ring
    private const double FSignInner  = 0.90;   // inner boundary of sign ring
    private const double FDeg10Inner = 0.87;   // inner boundary of 10° ring
    private const double FTickInner  = 0.82;   // inner boundary of 1° ring (planet area starts here)

    private const double FPlanetGlyph = 0.78;  // planet glyph radius
    private const double FPlanetText  = 0.59;  // planet degree-text radius

    private const double FCrossArm    = 0.04;  // centre cross arm half-length

    // Tick lengths (fraction of outerRadius, pointing inward from FDeg10Inner)
    private const double FTick1    = 0.010;
    private const double FTick5    = 0.025;
    private const double FTick10   = 0.042;

    // Degree labels: midpoint of sign ring, 5° inside each sign sector
    private const double FDegLabel       = 0.945;
    private const double FDegLabelOffset = 5.0;

    // MARK: - Background

    public static void DrawBackground(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var r    = outerR * FSignOuter;
        var rect = new Rect(center.X - r, center.Y - r, r * 2, r * 2);
        ctx.DrawEllipse(new SolidColorBrush(theme.OuterCircleBackground), null, center, r, r);
    }

    // MARK: - Ring strokes

    public static void DrawRingStrokes(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var sw   = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen  = new Pen(new SolidColorBrush(theme.CircleStroke), sw);
        foreach (var frac in new[] { FSignOuter, FSignInner, FDeg10Inner, FTickInner })
        {
            var r = outerR * frac;
            ctx.DrawEllipse(null, pen, center, r, r);
        }
    }

    // MARK: - Sign ring

    public static void DrawSignSectors(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var innerR = outerR * FSignInner;
        var rOuter = outerR * FSignOuter;

        for (var i = 0; i < 12; i++)
        {
            var startDeg = i * 30.0;
            var sign     = (Signs)(i + 1);
            var color    = theme.SignSectorColor(sign);
            if (color == Colors.Transparent) continue;

            var path = BuildAnnularSector(startDeg, startDeg + 30.0, innerR, rOuter, center);
            ctx.DrawGeometry(new SolidColorBrush(color), null, path);
        }
    }

    public static void DrawSignGlyphs(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var glyphR   = outerR * ((FSignInner + FSignOuter) / 2.0);
        var fontSize = WheelMetrics.FontSize(WheelMetrics.SignGlyphFontFraction, outerR);
        var brush    = new SolidColorBrush(theme.SignGlyph);

        for (var i = 0; i < 12; i++)
        {
            var midAngle = i * 30.0 + 15.0;
            var sign     = (Signs)(i + 1);
            var glyph    = GlyphSelector.GetGlyphForSign(sign);
            var pt       = WheelGeometry.PointOnCircle(midAngle, glyphR, center);
            DrawTextCentered(ctx, glyph, pt, fontSize, "EnigmaAstrology2", brush);
        }
    }

    public static void DrawSignSeparators(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var innerR = outerR * FSignInner;
        var rOuter = outerR * FSignOuter;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen    = new Pen(new SolidColorBrush(theme.SignSeparator), sw);

        for (var i = 0; i < 12; i++)
        {
            var angle = i * 30.0;
            var p1    = WheelGeometry.PointOnCircle(angle, innerR, center);
            var p2    = WheelGeometry.PointOnCircle(angle, rOuter, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawDegreeBoundaryLabels(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var labelR   = outerR * FDegLabel;
        var fontSize = WheelMetrics.FontSize(WheelMetrics.PositionTextFraction * 1.3, outerR);
        var brush    = new SolidColorBrush(theme.PlanetText);

        for (var i = 0; i < 12; i++)
        {
            var angle  = i * 30.0 + FDegLabelOffset;
            var degree = i * 30;
            var pt     = WheelGeometry.PointOnCircle(angle, labelR, center);
            DrawTextCentered(ctx, $"{degree}", pt, fontSize, null, brush);
        }
    }

    // MARK: - Tick rings

    public static void Draw10DegTicks(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FSignInner;
        var rInner = outerR * FDeg10Inner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction * 0.8, outerR);
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), sw);

        for (var i = 0; i < 36; i++)
        {
            var angle = i * 10.0;
            var p1    = WheelGeometry.PointOnCircle(angle, rOuter, center);
            var p2    = WheelGeometry.PointOnCircle(angle, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    public static void DrawDegTicks(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var rOuter = outerR * FDeg10Inner;
        var pen    = new Pen(new SolidColorBrush(theme.DegreeTickStroke), 1.0);

        for (var i = 0; i < 360; i++)
        {
            double tickFrac;
            if      (i % 10 == 0) tickFrac = FTick10;
            else if (i % 5  == 0) tickFrac = FTick5;
            else                  tickFrac = FTick1;

            var rInner = rOuter - outerR * tickFrac;
            var p1     = WheelGeometry.PointOnCircle(i, rOuter, center);
            var p2     = WheelGeometry.PointOnCircle(i, rInner, center);
            ctx.DrawLine(pen, p1, p2);
        }
    }

    // MARK: - Centre cross

    public static void DrawCenterCross(DrawingContext ctx, Point center, double outerR, WheelTheme theme)
    {
        var arm = outerR * FCrossArm;
        var sw  = WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, outerR);
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke), sw);

        ctx.DrawLine(pen, new Point(center.X - arm, center.Y), new Point(center.X + arm, center.Y));
        ctx.DrawLine(pen, new Point(center.X, center.Y - arm), new Point(center.X, center.Y + arm));
    }

    // MARK: - Planets

    public static void DrawConnectLines(DrawingContext ctx, Point center, double outerR,
                                        WheelPlotData data, WheelTheme theme)
    {
        var glyphR = outerR * FPlanetGlyph;
        var ringR  = outerR * FTickInner;
        var sw     = WheelMetrics.StrokeWidth(WheelMetrics.ConnectLineFraction, outerR);
        var color  = theme.PlanetConnectLine;
        var pen    = new Pen(new SolidColorBrush(
            Avalonia.Media.Color.FromArgb(
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
            DrawTextCentered(ctx, item.Glyph, pt, fontSize, "EnigmaAstrology2", brush);
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
            var pt = WheelGeometry.PointOnCircle(item.PlotAngle, r, center);
            DrawRotatedText(ctx, item.PositionText, pt, item.PlotAngle, fontSize, brush);
        }
    }

    // MARK: - Drawing helpers

    private static void DrawTextCentered(DrawingContext ctx, string text, Point center,
                                          double fontSize, string? fontFamily, IBrush brush)
    {
        var typeface  = fontFamily != null
            ? new Typeface(fontFamily)
            : Typeface.Default;
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        var origin = new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2);
        ctx.DrawText(formatted, origin);
    }

    private static void DrawRotatedText(DrawingContext ctx, string text, Point pt,
                                         double angleDeg, double fontSize, IBrush brush)
    {
        double rotDeg;
        if (angleDeg < 180.0)
            rotDeg = 90.0 - angleDeg;
        else
            rotDeg = 270.0 - angleDeg;

        var typeface  = Typeface.Default;
        var formatted = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            brush);

        var rad         = rotDeg * Math.PI / 180.0;
        var transform   = new Matrix(
            Math.Cos(rad), Math.Sin(rad),
           -Math.Sin(rad), Math.Cos(rad),
            pt.X, pt.Y);

        using (ctx.PushTransform(transform))
        {
            var origin = angleDeg < 180.0
                ? new Point(-formatted.Width, -formatted.Height / 2)
                : new Point(0, -formatted.Height / 2);
            ctx.DrawText(formatted, origin);
        }
    }

    // MARK: - Annular sector geometry

    private static PathGeometry BuildAnnularSector(double startDeg, double endDeg,
                                                    double innerR, double outerR, Point center)
    {
        var geo  = new PathGeometry();
        var fig  = new PathFigure { IsClosed = true, IsFilled = true };

        var p1 = WheelGeometry.PointOnCircle(startDeg, outerR, center);
        fig.StartPoint = p1;

        // Outer arc
        var p2       = WheelGeometry.PointOnCircle(endDeg, outerR, center);
        // PointOnCircle uses x = center.X - sin(rad), so increasing angle is counter-clockwise
        // on screen. The outer arc therefore sweeps CounterClockwise from startDeg to endDeg.
        var outerArc = new ArcSegment
        {
            Point          = p2,
            Size           = new Size(outerR, outerR),
            SweepDirection = SweepDirection.CounterClockwise,
            IsLargeArc     = (endDeg - startDeg) > 180
        };
        fig.Segments.Add(outerArc);

        // Line to inner arc start
        var p3 = WheelGeometry.PointOnCircle(endDeg, innerR, center);
        fig.Segments.Add(new LineSegment { Point = p3 });

        // Inner arc back to start — clockwise to close the shape correctly
        var p4       = WheelGeometry.PointOnCircle(startDeg, innerR, center);
        var innerArc = new ArcSegment
        {
            Point          = p4,
            Size           = new Size(innerR, innerR),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc     = (endDeg - startDeg) > 180
        };
        fig.Segments.Add(innerArc);

        geo.Figures.Add(fig);
        return geo;
    }
}
