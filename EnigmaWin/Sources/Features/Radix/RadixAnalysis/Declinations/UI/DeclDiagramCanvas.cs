// DeclDiagramCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclDiagramCanvas : FrameworkElement
{
    private const double DiagramOffsetLeftFraction  = 0.08;
    private const double DiagramOffsetRightFraction = 0.08;
    private const double DiagramWidthFraction       = 0.84;
    private const double DeclDegreeTopFraction      = 0.07;
    private const double DeclDegreeBottomFraction   = 0.07;
    private const double DeclDegreeLeftFraction     = 0.03;
    private const double DeclDegreeRightFraction    = 0.03;
    private const double LongDegreeTopFraction      = 0.04;
    private const double LongDegreeBottomFraction   = 0.04;
    private const double SignWidthFraction          = 0.14;
    private const double DegreeSizeLargeFraction    = 0.012;
    private const double DegreeSizeSmallFraction    = 0.006;
    private const double DegreeTextSizeFraction     = 0.02;
    private const double SignGlyphSizeFraction      = 0.04;
    private const double CelPointGlyphSizeFraction  = 0.03;
    private const double DeclCharLeftFraction       = 0.03;
    private const double DeclCharRightFraction      = 0.005;
    private const int    LongDegreesCount           = 180;

    private static readonly string[] SignGlyphChars =
    [
        "", "", "", "", "", "",
        "", "", "", "", "", "",
    ];

    private static readonly Color[] PositionLineColorsColor =
    [
        Color.FromRgb(0x00, 0x8B, 0x8B),
        Color.FromRgb(0xCC, 0x00, 0xCC),
        Color.FromRgb(0x00, 0x00, 0xCC),
    ];

    private static readonly Color[] PositionLineColorsBw =
    [
        Color.FromRgb(0x22, 0x22, 0x22),
        Color.FromRgb(0x66, 0x66, 0x66),
        Color.FromRgb(0xAA, 0xAA, 0xAA),
    ];

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IReadOnlyList<DeclDiagramItem>),
            typeof(DeclDiagramCanvas),
            new FrameworkPropertyMetadata(
                (IReadOnlyList<DeclDiagramItem>)Array.Empty<DeclDiagramItem>(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ObliquityProperty =
        DependencyProperty.Register(nameof(Obliquity), typeof(double), typeof(DeclDiagramCanvas),
            new FrameworkPropertyMetadata(23.45, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsBlackWhiteProperty =
        DependencyProperty.Register(nameof(IsBlackWhite), typeof(bool), typeof(DeclDiagramCanvas),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowPositionLinesProperty =
        DependencyProperty.Register(nameof(ShowPositionLines), typeof(bool), typeof(DeclDiagramCanvas),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<DeclDiagramItem> Items
    {
        get => (IReadOnlyList<DeclDiagramItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public double Obliquity
    {
        get => (double)GetValue(ObliquityProperty);
        set => SetValue(ObliquityProperty, value);
    }

    public bool IsBlackWhite
    {
        get => (bool)GetValue(IsBlackWhiteProperty);
        set => SetValue(IsBlackWhiteProperty, value);
    }

    public bool ShowPositionLines
    {
        get => (bool)GetValue(ShowPositionLinesProperty);
        set => SetValue(ShowPositionLinesProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (availableSize.Width > 0 && !double.IsInfinity(availableSize.Width))
        {
            var h = availableSize.Width / 1.25;
            if (!double.IsInfinity(availableSize.Height))
                h = Math.Min(h, availableSize.Height);
            return new Size(availableSize.Width, h);
        }
        return new Size(800, 640);
    }

    protected override void OnRender(DrawingContext ctx)
    {
        var boundsW = ActualWidth;
        var h       = ActualHeight;
        if (boundsW < 10 || h < 10) return;

        var w      = Math.Min(boundsW, h * 1.25);
        var xShift = (boundsW - w) / 2.0;

        var bw = IsBlackWhite;

        var bgColor       = bw ? Color.FromRgb(0xCC, 0xCC, 0xCC) : Color.FromRgb(0xAD, 0xD8, 0xE6);
        var inBoundsColor = bw ? Color.FromRgb(0xF0, 0xF0, 0xF0) : Color.FromRgb(0xE0, 0xFF, 0xFF);
        var signBarColor  = bw ? Color.FromArgb(80, 0x99, 0x99, 0x99) : Color.FromArgb(80, 0xF0, 0xE6, 0x8C);
        var lineColor     = Colors.Black;
        var degTextColor  = bw ? Colors.Black : Color.FromRgb(0x00, 0x00, 0x8B);
        var signGlyphColor= bw ? Colors.Black : Color.FromRgb(0x00, 0x00, 0x8B);
        var northFill     = bw ? Color.FromArgb(80, 0x88, 0x88, 0x88) : Color.FromArgb(80, 0xF5, 0xCC, 0xB0);
        var northStroke   = bw ? Color.FromRgb(0x22, 0x22, 0x22) : Color.FromRgb(0xFF, 0x7F, 0x50);
        var southFill     = bw ? Color.FromArgb(80, 0xBB, 0xBB, 0xBB) : Color.FromArgb(80, 0xAF, 0xEE, 0xEE);
        var southStroke   = bw ? Color.FromRgb(0x55, 0x55, 0x55) : Color.FromRgb(0x64, 0x95, 0xED);
        var posLineColors = bw ? PositionLineColorsBw : PositionLineColorsColor;

        var items      = Items;
        var maxAbsDecl = items.Count > 0 ? items.Max(i => Math.Abs(i.Declination)) : 0.0;
        var declRange  = 30;
        if (maxAbsDecl > declRange)
        {
            var extra = (int)(maxAbsDecl - 30.0);
            declRange += (extra / 5 + 1) * 5;
        }
        var declRangeD = (double)declRange;

        var diagramLeft  = DiagramOffsetLeftFraction  * w;
        var diagramWidth = DiagramWidthFraction       * w;
        var declTopOff   = DeclDegreeTopFraction      * h;
        var declBotOff   = DeclDegreeBottomFraction   * h;
        var declLeftOff  = DeclDegreeLeftFraction     * w;
        var declRightOff = DeclDegreeRightFraction    * w;
        var longTopOff   = LongDegreeTopFraction      * h;
        var longBotOff   = LongDegreeBottomFraction   * h;
        var signWidth    = SignWidthFraction           * w;
        var minDim       = Math.Min(w, h);
        var largeTick    = DegreeSizeLargeFraction    * minDim;
        var smallTick    = DegreeSizeSmallFraction    * minDim;
        var degTextSize  = Math.Max(8.0, DegreeTextSizeFraction    * minDim);
        var signGlyphSz  = Math.Max(10.0, SignGlyphSizeFraction    * minDim);
        var celGlyphSz   = Math.Max(10.0, CelPointGlyphSizeFraction * minDim);
        var declCharLeft = DeclCharLeftFraction       * minDim;
        var declCharRight= DeclCharRightFraction      * minDim;

        var transform = new TranslateTransform(xShift, 0);
        ctx.PushTransform(transform);

        var declBarH    = h - declTopOff - declBotOff;
        var inBoundsH   = (Obliquity / declRangeD) * declBarH;
        var inBoundsTop = (h - inBoundsH) / 2.0;

        ctx.DrawRectangle(new SolidColorBrush(bgColor),       null, new Rect(0, 0, w, h));
        ctx.DrawRectangle(new SolidColorBrush(inBoundsColor), null, new Rect(0, inBoundsTop, w, inBoundsH));

        var signBarBrush = new SolidColorBrush(signBarColor);
        for (var i = 0; i < 3; i++)
        {
            var xSig = diagramLeft + i * signWidth * 2;
            ctx.DrawRectangle(signBarBrush, null, new Rect(xSig, 0, signWidth, inBoundsTop));
            ctx.DrawRectangle(signBarBrush, null, new Rect(xSig, inBoundsTop, signWidth, inBoundsH));
            ctx.DrawRectangle(signBarBrush, null, new Rect(xSig, inBoundsTop + inBoundsH, signWidth, h - (inBoundsTop + inBoundsH)));
        }

        var rulePen = new Pen(new SolidColorBrush(lineColor), 1.0);
        ctx.DrawLine(rulePen, new Point(diagramLeft, longTopOff), new Point(diagramLeft + diagramWidth, longTopOff));
        ctx.DrawLine(rulePen, new Point(diagramLeft, h - longBotOff), new Point(diagramLeft + diagramWidth, h - longBotOff));

        var longDegW = diagramWidth / LongDegreesCount;
        for (var i = 0; i <= LongDegreesCount; i++)
        {
            var xTick = diagramLeft + i * longDegW;
            var tickH = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(xTick, longTopOff), new Point(xTick, longTopOff + tickH));
            ctx.DrawLine(rulePen, new Point(xTick, h - longBotOff), new Point(xTick, h - longBotOff - tickH));
        }

        ctx.DrawLine(rulePen, new Point(declLeftOff, declTopOff), new Point(declLeftOff, h - declBotOff));
        var leftVertInterval = declBarH / (declRangeD * 2);
        for (var i = 0; i <= declRangeD * 2; i++)
        {
            var yTick = declTopOff + i * leftVertInterval;
            var tickW = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(declLeftOff, yTick), new Point(declLeftOff + tickW, yTick));
        }

        var degLabelBrush = new SolidColorBrush(degTextColor);
        var degTypeface   = new Typeface("Segoe UI");
        for (var i = 0; i <= declRange / 5; i++)
        {
            var degVal = declRange - i * 10;
            var yLabel = declTopOff + i * leftVertInterval * 10 - leftVertInterval * 0.8;
            var xLabel = declLeftOff - declCharLeft;
            DrawSmallText(ctx, degVal.ToString(), xLabel, yLabel, degTextSize, degLabelBrush, degTypeface);
        }

        var rightX = w - declRightOff;
        ctx.DrawLine(rulePen, new Point(rightX, declTopOff), new Point(rightX, h - declBotOff));
        for (var i = 0; i <= declRangeD * 2; i++)
        {
            var yTick = declTopOff + i * leftVertInterval;
            var tickW = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(rightX, yTick), new Point(rightX - tickW, yTick));
        }
        for (var i = 0; i <= declRange / 5; i++)
        {
            var degVal = declRange - i * 10;
            var yLabel = declTopOff + i * leftVertInterval * 10 - leftVertInterval * 0.8;
            var xLabel = rightX + declCharRight;
            DrawSmallText(ctx, degVal.ToString(), xLabel, yLabel, degTextSize, degLabelBrush, degTypeface);
        }

        var glyphFont  = (Application.Current?.TryFindResource("GlyphFont") as FontFamily)
                         ?? new FontFamily(new Uri("pack://application:,,,/"), "/Resources/Fonts/#EnigmaAstrology2");
        var glyphTypef = new Typeface(glyphFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var glyphBrush = new SolidColorBrush(signGlyphColor);

        for (var i = 0; i <= 6; i++)
        {
            var xSep = diagramLeft + i * signWidth;
            ctx.DrawLine(rulePen, new Point(xSep, 0), new Point(xSep, longTopOff));
            ctx.DrawLine(rulePen, new Point(xSep, h - longBotOff), new Point(xSep, h));
        }
        var signGlyphCenterY = longTopOff / 2.0;
        var signGlyphBottomY = h - longBotOff / 2.0;
        for (var i = 0; i < 6; i++)
        {
            var xCenter = diagramLeft + (i + 0.5) * signWidth;
            DrawGlyphCentered(ctx, SignGlyphChars[i],      xCenter, signGlyphCenterY, signGlyphSz, glyphTypef, glyphBrush);
            DrawGlyphCentered(ctx, SignGlyphChars[11 - i], xCenter, signGlyphBottomY, signGlyphSz, glyphTypef, glyphBrush);
        }

        DrawObliquityPolygons(ctx, diagramLeft, diagramWidth, h, declTopOff, declBotOff,
            declRangeD, Obliquity, northFill, northStroke, southFill, southStroke);

        if (items.Count > 0)
        {
            DrawPositions(ctx, diagramLeft, diagramWidth, h, declTopOff, declBotOff,
                longTopOff, longBotOff, declRangeD, longDegW,
                declLeftOff, rightX, celGlyphSz, glyphTypef, posLineColors);
        }

        ctx.Pop();
    }

    private void DrawObliquityPolygons(
        DrawingContext ctx,
        double diagramLeft, double diagramWidth, double h,
        double declTopOff, double declBotOff,
        double declRangeD, double obliquity,
        Color northFill, Color northStroke,
        Color southFill, Color southStroke)
    {
        var declBarH  = h - declTopOff - declBotOff;
        var declDegSz = declBarH / (declRangeD * 2.0);
        var longDegW  = diagramWidth / LongDegreesCount;
        var midY      = declTopOff + declBarH / 2.0;

        var northPts = new List<Point>(LongDegreesCount + 2);
        var southPts = new List<Point>(LongDegreesCount + 2);

        for (var i = 0; i <= LongDegreesCount; i++)
        {
            var decl = CalcMaxDecl(i, obliquity);
            var xPt  = diagramLeft + i * longDegW;
            northPts.Add(new Point(xPt, midY - decl * declDegSz));
            southPts.Add(new Point(xPt, midY + decl * declDegSz));
        }

        DrawFilledPolyline(ctx, northPts,
            new SolidColorBrush(northFill), new Pen(new SolidColorBrush(northStroke), 1.0), 0.5);
        DrawFilledPolyline(ctx, southPts,
            new SolidColorBrush(southFill), new Pen(new SolidColorBrush(southStroke), 1.0), 0.5);
    }

    private static double CalcMaxDecl(double longitude, double obliquity)
        => RadToDeg(Math.Asin(Math.Sin(DegToRad(longitude)) * Math.Sin(DegToRad(obliquity))));

    private static void DrawFilledPolyline(DrawingContext ctx, List<Point> pts,
                                            Brush fill, Pen stroke, double opacity)
    {
        if (pts.Count < 2) return;
        var geo = new StreamGeometry();
        var sgc = geo.Open();
        sgc.BeginFigure(pts[0], true, true);
        for (var i = 1; i < pts.Count; i++)
            sgc.LineTo(pts[i], true, false);
        sgc.Close();
        geo.Freeze();
        ctx.PushOpacity(opacity);
        ctx.DrawGeometry(fill, stroke, geo);
        ctx.Pop();
    }

    private void DrawPositions(
        DrawingContext ctx,
        double diagramLeft, double diagramWidth, double h,
        double declTopOff, double declBotOff,
        double longTopOff, double longBotOff,
        double declRangeD, double longDegW,
        double declLeftX, double declRightX,
        double celGlyphSize, Typeface glyphFont, Color[] lineColors)
    {
        var items  = Items;
        var sorted = items.OrderBy(p => p.Longitude).ToList();
        var columns = new List<List<DeclDiagramItem>>(72);
        for (var col = 0; col < 72; col++)
            columns.Add([]);
        foreach (var item in sorted)
        {
            var colIdx = (int)(item.Longitude / (360.0 / 72)) % 72;
            columns[colIdx].Add(item);
        }

        var plottable = new List<(DeclDiagramItem item, double plotDecl)>();
        const double minSep = 1.5;
        foreach (var col in columns)
        {
            col.Sort((a, b) => Math.Abs(a.Declination).CompareTo(Math.Abs(b.Declination)));
            var lastPlot = -100.0;
            foreach (var item in col)
            {
                var plotDecl = item.Declination;
                var delta    = Math.Abs(plotDecl) - Math.Abs(lastPlot);
                if (Math.Abs(delta) < minSep)
                {
                    if (plotDecl >= 0) plotDecl += minSep - Math.Abs(delta);
                    else               plotDecl -= minSep - Math.Abs(delta);
                }
                plottable.Add((item, plotDecl));
                lastPlot = plotDecl;
            }
        }

        var declBarH  = h - declTopOff - declBotOff;
        var midY      = declTopOff + declBarH / 2.0;

        var colorIndex = 0;
        foreach (var (item, plotDecl) in plottable)
        {
            var col = lineColors[colorIndex++ % lineColors.Length];

            var longForX     = item.Longitude < 180.0 ? item.Longitude : 360.0 - item.Longitude;
            var xPos         = diagramLeft + longForX * longDegW;
            var yPos         = midY - item.Declination * (declBarH / (declRangeD * 2.0));
            var yPlot        = midY - plotDecl         * (declBarH / (declRangeD * 2.0));
            var yBorderRuler = item.Longitude < 180.0 ? longTopOff : h - longBotOff;

            if (ShowPositionLines)
            {
                var linePen = new Pen(new SolidColorBrush(Color.FromArgb(179, col.R, col.G, col.B)), 0.7);
                ctx.DrawLine(linePen, new Point(declLeftX, yPos), new Point(declRightX, yPos));
                ctx.DrawLine(linePen, new Point(xPos, yBorderRuler), new Point(xPos, yPos));
            }

            var ft = new FormattedText(item.Glyph, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, glyphFont, celGlyphSize,
                new SolidColorBrush(col), 1.0);
            ctx.DrawText(ft, new Point(xPos - ft.Width / 2.0, yPlot - ft.Height / 2.0));
        }
    }

    private static void DrawSmallText(DrawingContext ctx, string text, double x, double y,
                                       double size, Brush brush, Typeface typeface)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, typeface, size, brush, 1.0);
        ctx.DrawText(ft, new Point(x, y));
    }

    private static void DrawGlyphCentered(DrawingContext ctx, string glyph, double cx, double cy,
                                           double size, Typeface font, Brush brush)
    {
        var ft = new FormattedText(glyph, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, font, size, brush, 1.0);
        ctx.DrawText(ft, new Point(cx - ft.Width / 2.0, cy - ft.Height / 2.0));
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;
    private static double RadToDeg(double rad) => rad * 180.0 / Math.PI;
}
