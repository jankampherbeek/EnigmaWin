// DeclDiagramCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

/// <summary>
/// Custom Avalonia control that draws the declination diagram (Kt Boehrer style).
///
/// Layout mirrors the WPF DeclDiagramWindow / DeclDiagramCanvasController:
/// - Light-blue background with a lighter cyan in-bounds band
/// - Alternating khaki sign-column overlays (6 sign columns top + bottom)
/// - Horizontal longitude-degree ruler at top and bottom
/// - Left and right vertical declination-degree rulers
/// - North (Aries–Virgo) declination curve and South (Libra–Pisces) curve as filled polygons
/// - Factor glyphs plotted at their (longitude, declination) position
/// - Optional position lines (horizontal decl line + vertical longitude line per factor)
/// </summary>
public sealed class DeclDiagramCanvas : Control
{
    // ── Styled properties ─────────────────────────────────────────────────

    public static readonly StyledProperty<IReadOnlyList<DeclDiagramItem>> ItemsProperty =
        AvaloniaProperty.Register<DeclDiagramCanvas, IReadOnlyList<DeclDiagramItem>>(nameof(Items), []);

    public static readonly StyledProperty<double> ObliquityProperty =
        AvaloniaProperty.Register<DeclDiagramCanvas, double>(nameof(Obliquity), 23.45);

    public static readonly StyledProperty<bool> IsBlackWhiteProperty =
        AvaloniaProperty.Register<DeclDiagramCanvas, bool>(nameof(IsBlackWhite), false);

    public static readonly StyledProperty<bool> ShowPositionLinesProperty =
        AvaloniaProperty.Register<DeclDiagramCanvas, bool>(nameof(ShowPositionLines), false);

    public IReadOnlyList<DeclDiagramItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public double Obliquity
    {
        get => GetValue(ObliquityProperty);
        set => SetValue(ObliquityProperty, value);
    }

    public bool IsBlackWhite
    {
        get => GetValue(IsBlackWhiteProperty);
        set => SetValue(IsBlackWhiteProperty, value);
    }

    public bool ShowPositionLines
    {
        get => GetValue(ShowPositionLinesProperty);
        set => SetValue(ShowPositionLinesProperty, value);
    }

    static DeclDiagramCanvas()
    {
        AffectsRender<DeclDiagramCanvas>(
            ItemsProperty, ObliquityProperty, IsBlackWhiteProperty, ShowPositionLinesProperty);
    }

    // ── Layout fractions (match WPF metrics constants) ────────────────────

    private const double DiagramOffsetLeftFraction   = 0.08;
    private const double DiagramOffsetRightFraction  = 0.08;
    private const double DiagramWidthFraction        = 0.84;
    private const double DeclDegreeTopFraction       = 0.07;
    private const double DeclDegreeBottomFraction    = 0.07;
    private const double DeclDegreeLeftFraction      = 0.03;
    private const double DeclDegreeRightFraction     = 0.03;
    private const double LongDegreeTopFraction       = 0.04;
    private const double LongDegreeBottomFraction    = 0.04;
    private const double SignWidthFraction           = 0.14;
    private const double DegreeSizeLargeFraction     = 0.012;
    private const double DegreeSizeSmallFraction     = 0.006;
    private const double DegreeTextSizeFraction      = 0.02;
    private const double SignGlyphSizeFraction       = 0.04;
    private const double CelPointGlyphSizeFraction   = 0.03;
    private const double DeclCharLeftFraction        = 0.03;
    private const double DeclCharRightFraction       = 0.005;
    private const int    LongDegreesCount            = 180;

    // Sign glyphs using Unicode private-use area (matches SignGlyphs.cs):
    // Aries=\uE000 .. Capricorn=\uE009, Aquarius=\uE010, Pisces=\uE011
    // Top row  (longitude 0–180):  Aries, Taurus, Gemini, Cancer, Leo, Virgo
    // Bottom row (longitude 180–360, right-to-left): Libra, Scorpio, Sagittarius, Capricorn, Aquarius, Pisces
    private static readonly string[] SignGlyphChars =
    [
        "\uE000", "\uE001", "\uE002", "\uE003", "\uE004", "\uE005",  // top:    Aries–Virgo
        "\uE006", "\uE007", "\uE008", "\uE009", "\uE010", "\uE011",  // bottom: Libra–Pisces
    ];

    // Position-line colours (cycling, semi-transparent) — colour mode
    private static readonly Color[] PositionLineColorsColor =
    [
        Color.FromRgb(0x00, 0x8B, 0x8B),  // DarkCyan
        Color.FromRgb(0xCC, 0x00, 0xCC),  // Fuchsia
        Color.FromRgb(0x00, 0x00, 0xCC),  // Blue
    ];

    // Position-line colours — black/white mode (dark grey, mid-grey, light grey)
    private static readonly Color[] PositionLineColorsBw =
    [
        Color.FromRgb(0x22, 0x22, 0x22),
        Color.FromRgb(0x66, 0x66, 0x66),
        Color.FromRgb(0xAA, 0xAA, 0xAA),
    ];

    // ── Rendering ─────────────────────────────────────────────────────────

    public override void Render(DrawingContext ctx)
    {
        var boundsW = Bounds.Width;
        var h       = Bounds.Height;
        if (boundsW < 10 || h < 10) return;

        // Cap drawing width to the 1.25:1 aspect ratio so the diagram never
        // stretches wider than its height warrants, and centre it horizontally.
        var w      = Math.Min(boundsW, h * 1.25);
        var xShift = (boundsW - w) / 2.0;

        var bw = IsBlackWhite;

        // ── Colour palette ────────────────────────────────────────────────
        var bgColor       = bw ? Color.FromRgb(0xCC, 0xCC, 0xCC) : Color.FromRgb(0xAD, 0xD8, 0xE6); // grey  / LightBlue
        var inBoundsColor = bw ? Color.FromRgb(0xF0, 0xF0, 0xF0) : Color.FromRgb(0xE0, 0xFF, 0xFF); // white / LightCyan
        var signBarColor  = bw ? Color.FromArgb(80, 0x99, 0x99, 0x99) : Color.FromArgb(80, 0xF0, 0xE6, 0x8C); // grey  / Khaki
        var lineColor     = bw ? Colors.Black                          : Colors.Black;
        var degTextColor  = bw ? Colors.Black                          : Color.FromRgb(0x00, 0x00, 0x8B);
        var signGlyphColor= bw ? Colors.Black                          : Color.FromRgb(0x00, 0x00, 0x8B);
        var northFill     = bw ? Color.FromArgb(80, 0x88, 0x88, 0x88) : Color.FromArgb(80, 0xF5, 0xCC, 0xB0); // dark grey  / Bisque
        var northStroke   = bw ? Color.FromRgb(0x22, 0x22, 0x22)      : Color.FromRgb(0xFF, 0x7F, 0x50);       // near-black / Coral
        var southFill     = bw ? Color.FromArgb(80, 0xBB, 0xBB, 0xBB) : Color.FromArgb(80, 0xAF, 0xEE, 0xEE); // mid grey   / PaleTurquoise
        var southStroke   = bw ? Color.FromRgb(0x55, 0x55, 0x55)      : Color.FromRgb(0x64, 0x95, 0xED);       // mid-dark   / CornflowerBlue
        var posLineColors = bw ? PositionLineColorsBw : PositionLineColorsColor;

        // ── Determine declination range ───────────────────────────────────
        var maxAbsDecl = Items.Count > 0 ? Items.Max(i => Math.Abs(i.Declination)) : 0.0;
        var declRange  = 30;
        if (maxAbsDecl > declRange)
        {
            var extra = (int)(maxAbsDecl - 30.0);
            declRange += (extra / 5 + 1) * 5;
        }
        var declRangeD = (double)declRange;

        // ── Derived metrics ───────────────────────────────────────────────
        var diagramLeft   = DiagramOffsetLeftFraction  * w;
        var diagramWidth  = DiagramWidthFraction       * w;
        var declTopOff    = DeclDegreeTopFraction      * h;
        var declBotOff    = DeclDegreeBottomFraction   * h;
        var declLeftOff   = DeclDegreeLeftFraction     * w;
        var declRightOff  = DeclDegreeRightFraction    * w;
        var longTopOff    = LongDegreeTopFraction      * h;
        var longBotOff    = LongDegreeBottomFraction   * h;
        var signWidth     = SignWidthFraction           * w;
        var minDim        = Math.Min(w, h);
        var largeTick     = DegreeSizeLargeFraction    * minDim;
        var smallTick     = DegreeSizeSmallFraction    * minDim;
        var degTextSize   = Math.Max(8.0, DegreeTextSizeFraction    * minDim);
        var signGlyphSize = Math.Max(10.0, SignGlyphSizeFraction    * minDim);
        var celGlyphSize  = Math.Max(10.0, CelPointGlyphSizeFraction * minDim);
        var declCharLeft  = DeclCharLeftFraction       * minDim;
        var declCharRight = DeclCharRightFraction      * minDim;

        // Shift all drawing to centre the diagram within the actual bounds width
        using var _ = ctx.PushTransform(Matrix.CreateTranslation(xShift, 0));

        // Declination bar geometry
        var declBarH    = h - declTopOff - declBotOff;
        var declDegH    = declBarH / (declRangeD * 2.0);  // pixels per degree (half = north or south)
        var declDegHalf = declBarH / 2.0;

        // In-bounds region: centred vertically, height proportional to obliquity/declRange
        var inBoundsH  = (Obliquity / declRangeD) * declBarH;
        var inBoundsTop = (h - inBoundsH) / 2.0;

        // ── Backgrounds ───────────────────────────────────────────────────
        ctx.FillRectangle(new SolidColorBrush(bgColor),       new Rect(0, 0, w, h));
        ctx.FillRectangle(new SolidColorBrush(inBoundsColor), new Rect(0, inBoundsTop, w, inBoundsH));

        // ── Alternating sign-column overlays (3 pairs, i.e. Aries/Cancer/Libra/Capricorn columns) ──
        var signBarBrush = new SolidColorBrush(signBarColor);
        for (var i = 0; i < 3; i++)
        {
            var xSig = diagramLeft + i * signWidth * 2;
            // Top out-of-bounds band
            ctx.FillRectangle(signBarBrush, new Rect(xSig, 0, signWidth, inBoundsTop));
            // In-bounds band
            ctx.FillRectangle(signBarBrush, new Rect(xSig, inBoundsTop, signWidth, inBoundsH));
            // Bottom out-of-bounds band
            ctx.FillRectangle(signBarBrush, new Rect(xSig, inBoundsTop + inBoundsH, signWidth, h - (inBoundsTop + inBoundsH)));
        }

        // ── Longitude ruler lines (top + bottom) ──────────────────────────
        var rulePen = new Pen(new SolidColorBrush(lineColor), 1.0);
        // Top ruler bar
        ctx.DrawLine(rulePen, new Point(diagramLeft, longTopOff), new Point(diagramLeft + diagramWidth, longTopOff));
        // Bottom ruler bar
        ctx.DrawLine(rulePen, new Point(diagramLeft, h - longBotOff), new Point(diagramLeft + diagramWidth, h - longBotOff));

        var longDegW = diagramWidth / LongDegreesCount;
        // Top ticks
        for (var i = 0; i <= LongDegreesCount; i++)
        {
            var xTick = diagramLeft + i * longDegW;
            var tickH = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(xTick, longTopOff), new Point(xTick, longTopOff + tickH));
        }
        // Bottom ticks
        for (var i = 0; i <= LongDegreesCount; i++)
        {
            var xTick = diagramLeft + i * longDegW;
            var tickH = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(xTick, h - longBotOff), new Point(xTick, h - longBotOff - tickH));
        }

        // ── Left declination ruler ────────────────────────────────────────
        ctx.DrawLine(rulePen,
            new Point(declLeftOff, declTopOff),
            new Point(declLeftOff, h - declBotOff));
        var leftVertInterval = declBarH / (declRangeD * 2);
        for (var i = 0; i <= declRangeD * 2; i++)
        {
            var yTick = declTopOff + i * leftVertInterval;
            var tickW = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(declLeftOff, yTick), new Point(declLeftOff + tickW, yTick));
        }
        // Degree labels left
        var degLabelBrush = new SolidColorBrush(degTextColor);
        var degTypeface   = Typeface.Default;
        for (var i = 0; i <= declRange / 5; i++)
        {
            var degVal = declRange - i * 10;
            var yLabel = declTopOff + i * leftVertInterval * 10 - leftVertInterval * 0.8;
            var xLabel = declLeftOff - declCharLeft;
            DrawSmallText(ctx, degVal.ToString(), xLabel, yLabel, degTextSize, degLabelBrush, degTypeface);
        }

        // ── Right declination ruler (symmetrically inset from right edge) ───
        var rightX = w - declRightOff;
        ctx.DrawLine(rulePen, new Point(rightX, declTopOff), new Point(rightX, h - declBotOff));
        for (var i = 0; i <= declRangeD * 2; i++)
        {
            var yTick = declTopOff + i * leftVertInterval;
            var tickW = i % 5 == 0 ? largeTick : smallTick;
            ctx.DrawLine(rulePen, new Point(rightX, yTick), new Point(rightX - tickW, yTick));
        }
        // Degree labels right
        for (var i = 0; i <= declRange / 5; i++)
        {
            var degVal = declRange - i * 10;
            var yLabel = declTopOff + i * leftVertInterval * 10 - leftVertInterval * 0.8;
            var xLabel = rightX + declCharRight;
            DrawSmallText(ctx, degVal.ToString(), xLabel, yLabel, degTextSize, degLabelBrush, degTypeface);
        }

        // ── Sign separator lines + glyphs ─────────────────────────────────
        var glyphFont  = new Typeface("avares://EnigmaWin/Resources/Fonts#EnigmaAstrology2");
        var glyphBrush = new SolidColorBrush(signGlyphColor);
        // Separator lines top
        for (var i = 0; i <= 6; i++)
        {
            var xSep = diagramLeft + i * signWidth;
            ctx.DrawLine(rulePen, new Point(xSep, 0), new Point(xSep, longTopOff));
        }
        // Separator lines bottom
        for (var i = 0; i <= 6; i++)
        {
            var xSep = diagramLeft + i * signWidth;
            ctx.DrawLine(rulePen, new Point(xSep, h - longBotOff), new Point(xSep, h));
        }
        // Sign glyphs top (Aries–Virgo, signs 1–6)
        var signGlyphCenterY = longTopOff / 2.0;
        for (var i = 0; i < 6; i++)
        {
            var xCenter = diagramLeft + (i + 0.5) * signWidth;
            DrawGlyphCentered(ctx, SignGlyphChars[i], xCenter, signGlyphCenterY, signGlyphSize, glyphFont, glyphBrush);
        }
        // Sign glyphs bottom (Libra–Pisces descending, signs 12–7 from left-to-right since longitude decreases)
        var signGlyphBottomY = h - longBotOff / 2.0;
        for (var i = 0; i < 6; i++)
        {
            var xCenter = diagramLeft + (i + 0.5) * signWidth;
            DrawGlyphCentered(ctx, SignGlyphChars[11 - i], xCenter, signGlyphBottomY, signGlyphSize, glyphFont, glyphBrush);
        }

        // ── Ecliptic obliquity polygon (north + south curves) ─────────────
        DrawObliquityPolygons(ctx, diagramLeft, diagramWidth, h, declTopOff, declBotOff,
            declRangeD, Obliquity,
            northFill, northStroke, southFill, southStroke);

        // ── Factor positions ───────────────────────────────────────────────
        if (Items.Count > 0)
        {
            DrawPositions(ctx, diagramLeft, diagramWidth, h, declTopOff, declBotOff,
                longTopOff, longBotOff, declRangeD, declDegH,
                longDegW, declLeftOff, rightX,
                celGlyphSize, glyphFont, posLineColors);
        }
    }

    // ── Ecliptic curve polygons ───────────────────────────────────────────

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

        DrawFilledPolyline(ctx, northPts, new SolidColorBrush(northFill), new Pen(new SolidColorBrush(northStroke), 1.0), opacity: 0.5);
        DrawFilledPolyline(ctx, southPts, new SolidColorBrush(southFill), new Pen(new SolidColorBrush(southStroke), 1.0), opacity: 0.5);
    }

    private static double CalcMaxDecl(double longitude, double obliquity)
        => RadToDeg(Math.Asin(Math.Sin(DegToRad(longitude)) * Math.Sin(DegToRad(obliquity))));

    private static void DrawFilledPolyline(DrawingContext ctx, List<Point> pts, IBrush fill, Pen stroke, double opacity)
    {
        if (pts.Count < 2) return;
        var geo = new StreamGeometry();
        using (var geoCtx = geo.Open())
        {
            geoCtx.BeginFigure(pts[0], true);
            for (var i = 1; i < pts.Count; i++)
                geoCtx.LineTo(pts[i]);
            geoCtx.EndFigure(true);
        }
        using (ctx.PushOpacity(opacity))
        {
            ctx.DrawGeometry(fill, stroke, geo);
        }
    }

    // ── Factor glyph + position lines ────────────────────────────────────

    private void DrawPositions(
        DrawingContext ctx,
        double diagramLeft, double diagramWidth, double h,
        double declTopOff, double declBotOff,
        double longTopOff, double longBotOff,
        double declRangeD, double declDegH,
        double longDegW, double declLeftX, double declRightX,
        double celGlyphSize, Typeface glyphFont, Color[] lineColors)
    {
        // Sort into 72 half-degree columns and spread glyphs to avoid overlap
        var sorted = Items.OrderBy(p => p.Longitude).ToList();
        var columns = new List<List<DeclDiagramItem>>(72);
        for (var col = 0; col < 72; col++)
            columns.Add([]);
        foreach (var item in sorted)
        {
            var colIdx = (int)(item.Longitude / (360.0 / 72)) % 72;
            columns[colIdx].Add(item);
        }

        // Flatten with anti-overlap offset
        var plottable = new List<(DeclDiagramItem item, double plotDecl)>();
        var minSep    = 1.5;
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
            var col = lineColors[colorIndex % lineColors.Length];
            colorIndex++;

            // Map longitude: 0–180 → left half; 180–360 → right half (mirrored)
            var longForX     = item.Longitude < 180.0 ? item.Longitude : 360.0 - item.Longitude;
            var xPos         = diagramLeft + longForX * longDegW;
            var yPos         = midY - item.Declination * (declBarH / (declRangeD * 2.0));
            var yPlot        = midY - plotDecl         * (declBarH / (declRangeD * 2.0));
            var yBorderRuler = item.Longitude < 180.0 ? longTopOff : h - longBotOff;

            // Position lines
            if (ShowPositionLines)
            {
                var linePen = new Pen(new SolidColorBrush(Color.FromArgb(179, col.R, col.G, col.B)), 0.7);
                // Horizontal: decl level across full diagram width
                ctx.DrawLine(linePen,
                    new Point(declLeftX, yPos),
                    new Point(declRightX, yPos));
                // Vertical: from longitude ruler to actual declination point
                ctx.DrawLine(linePen,
                    new Point(xPos, yBorderRuler),
                    new Point(xPos, yPos));
            }

            // Factor glyph at plot position
            var ft = new FormattedText(
                item.Glyph,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                glyphFont,
                celGlyphSize,
                new SolidColorBrush(col));
            ctx.DrawText(ft, new Point(xPos - ft.Width / 2.0, yPlot - ft.Height / 2.0));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static void DrawSmallText(DrawingContext ctx, string text, double x, double y,
        double size, IBrush brush, Typeface typeface)
    {
        var ft = new FormattedText(text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface, size, brush);
        ctx.DrawText(ft, new Point(x, y));
    }

    private static void DrawGlyphCentered(DrawingContext ctx, string glyph, double cx, double cy,
        double size, Typeface font, IBrush brush)
    {
        var ft = new FormattedText(glyph,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            font, size, brush);
        ctx.DrawText(ft, new Point(cx - ft.Width / 2.0, cy - ft.Height / 2.0));
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;
    private static double RadToDeg(double rad) => rad * 180.0 / Math.PI;

    // ── Desired size ──────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        // Landscape aspect ratio ~1.25 (width : height)
        if (availableSize.Width > 0 && !double.IsInfinity(availableSize.Width))
        {
            var h = availableSize.Width / 1.25;
            if (!double.IsInfinity(availableSize.Height))
                h = Math.Min(h, availableSize.Height);
            return new Size(availableSize.Width, h);
        }
        return new Size(800, 640);
    }
}
