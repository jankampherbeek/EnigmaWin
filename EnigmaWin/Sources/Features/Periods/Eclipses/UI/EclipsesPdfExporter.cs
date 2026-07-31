// EclipsesPdfExporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace EnigmaWin.Sources.Features.Periods.Eclipses.UI;

/// <summary>Column labels for the eclipses PDF export header row.</summary>
public sealed record EclipsesPdfLabels(
    string Title, string ColDate, string ColPosition, string ColType, string ColVisible, string ColSaros);

/// <summary>
/// Builds the Eclipses results PDF as real vector text (selectable, searchable, crisp at
/// any zoom) instead of rasterizing the on-screen table to a bitmap.
/// </summary>
public static class EclipsesPdfExporter
{
    private const string GlyphFontFamily = "EnigmaAstrology3";
    private const string GlyphFontResourcePath = "Resources/Fonts/EnigmaAstrology3.ttf";

    private const double MarginPt   = 36;
    private const double TitleSize  = 14;
    private const double HeaderSize = 10;
    private const double RowSize    = 10;
    private const double GlyphSize  = 13;
    private const double RowHeight  = 16;

    private const double ColGlyph    = 22;
    private const double ColDate     = 110;
    private const double ColPosition = 90;
    private const double ColType     = 80;
    private const double ColVisible  = 50;
    private const double ColSaros    = 70;

    static EclipsesPdfExporter()
    {
        if (GlobalFontSettings.FontResolver is null)
            GlobalFontSettings.FontResolver = new EmbeddedGlyphFontResolver();
    }

    public static void Export(string filePath, EclipsesPdfLabels labels,
                               bool hasLocation, IReadOnlyList<EclipseResultRow> rows)
    {
        var document = new PdfDocument();
        var page     = document.AddPage();
        page.Width   = XUnit.FromPoint(595);  // A4 portrait
        page.Height  = XUnit.FromPoint(842);

        var gfx = XGraphics.FromPdfPage(page);

        var unicode = XPdfFontOptions.UnicodeDefault;
        var titleFont  = new XFont("Segoe UI", TitleSize, XFontStyleEx.Bold, unicode);
        var headerFont = new XFont("Segoe UI", HeaderSize, XFontStyleEx.Bold, unicode);
        var textFont   = new XFont("Segoe UI", RowSize, XFontStyleEx.Regular, unicode);
        var monoFont   = new XFont("Courier New", RowSize, XFontStyleEx.Regular, unicode);
        var glyphFont  = new XFont(GlyphFontFamily, GlyphSize, XFontStyleEx.Regular, unicode);

        double x = MarginPt;
        double y = MarginPt;
        var pageBottom = page.Height.Point - MarginPt;

        gfx.DrawString(labels.Title, titleFont, XBrushes.Black, new XPoint(x, y));
        y += TitleSize + 14;

        void DrawHeader()
        {
            double cx = x;
            gfx.DrawString(labels.ColDate, headerFont, XBrushes.Black, new XPoint(cx, y)); cx += ColGlyph + ColDate;
            gfx.DrawString(labels.ColPosition, headerFont, XBrushes.Black, new XPoint(cx, y)); cx += ColPosition;
            if (hasLocation)
            {
                gfx.DrawString(labels.ColType, headerFont, XBrushes.Black, new XPoint(cx, y)); cx += ColType;
                gfx.DrawString(labels.ColVisible, headerFont, XBrushes.Black, new XPoint(cx, y)); cx += ColVisible;
                gfx.DrawString(labels.ColSaros, headerFont, XBrushes.Black, new XPoint(cx, y));
            }
            y += RowHeight;
            gfx.DrawLine(XPens.Gray, x, y, x + TotalWidth(hasLocation), y);
            y += 8;
        }

        DrawHeader();

        foreach (var row in rows)
        {
            if (y > pageBottom)
            {
                page    = document.AddPage();
                page.Width  = XUnit.FromPoint(595);
                page.Height = XUnit.FromPoint(842);
                gfx     = XGraphics.FromPdfPage(page);
                y       = MarginPt;
                DrawHeader();
            }

            double cx = x;
            gfx.DrawString(row.EclipseGlyph, glyphFont, XBrushes.Black, new XPoint(cx, y + GlyphSize - RowSize));
            cx += ColGlyph;

            gfx.DrawString(row.DateText, monoFont, XBrushes.Black, new XPoint(cx, y));
            cx += ColDate;

            gfx.DrawString(row.PositionText, monoFont, XBrushes.Black, new XPoint(cx, y));
            var posWidth = gfx.MeasureString(row.PositionText, monoFont).Width;
            if (!string.IsNullOrEmpty(row.SignGlyph))
                gfx.DrawString(row.SignGlyph, glyphFont, XBrushes.Black,
                    new XPoint(cx + posWidth + 2, y + GlyphSize - RowSize));
            cx += ColPosition;

            if (hasLocation)
            {
                gfx.DrawString(row.TypeText, textFont, XBrushes.Black, new XPoint(cx, y));
                cx += ColType;

                if (row.HasLocalData)
                    DrawCheckMark(gfx, cx, y);
                else
                    gfx.DrawString("—", textFont, XBrushes.Gray, new XPoint(cx, y));
                cx += ColVisible;

                gfx.DrawString(row.SarosText, monoFont, XBrushes.Black, new XPoint(cx, y));
            }

            y += RowHeight;
        }

        document.Save(filePath);
    }

    /// <summary>
    /// Draws a small vector checkmark rather than relying on a font glyph for U+2713,
    /// which is not reliably available across embedded system fonts.
    /// </summary>
    private static void DrawCheckMark(XGraphics gfx, double left, double top)
    {
        var pen = new XPen(XColors.Green, 1.4) { LineJoin = XLineJoin.Round, LineCap = XLineCap.Round };
        var baseY = top + RowSize - 9;
        gfx.DrawLine(pen, left + 1, baseY - 3, left + 4, baseY);
        gfx.DrawLine(pen, left + 4, baseY, left + 10, baseY - 8);
    }

    private static double TotalWidth(bool hasLocation)
    {
        var w = ColGlyph + ColDate + ColPosition;
        if (hasLocation) w += ColType + ColVisible + ColSaros;
        return w;
    }

    /// <summary>
    /// Resolves the embedded EnigmaAstrology3 glyph font plus the small set of system fonts
    /// used in the export (Segoe UI, Courier New), loaded directly from the Windows Fonts folder.
    /// PdfSharp 6.x has no public built-in platform fallback, so a custom resolver must cover
    /// every face name used by the document, not just the custom one.
    /// </summary>
    private sealed class EmbeddedGlyphFontResolver : IFontResolver
    {
        private const string WindowsFontsDir = @"C:\Windows\Fonts";

        private readonly Dictionary<string, byte[]> _fontData = new(StringComparer.OrdinalIgnoreCase)
        {
            [GlyphFontFamily]         = LoadGlyphFontBytes(),
            ["Segoe UI"]              = LoadSystemFontBytes("segoeui.ttf"),
            ["Segoe UI#Bold"]         = LoadSystemFontBytes("segoeuib.ttf"),
            ["Courier New"]           = LoadSystemFontBytes("cour.ttf"),
        };

        public byte[] GetFont(string faceName) =>
            _fontData.TryGetValue(faceName, out var data)
                ? data
                : throw new InvalidOperationException($"Unknown font face '{faceName}'.");

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName == GlyphFontFamily)
                return new FontResolverInfo(GlyphFontFamily);

            if (familyName.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo(isBold ? "Segoe UI#Bold" : "Segoe UI");

            if (familyName.Equals("Courier New", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("Courier New");

            return null;
        }

        private static byte[] LoadGlyphFontBytes()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var path    = Path.Combine(baseDir, GlyphFontResourcePath);
            if (!File.Exists(path))
                throw new FileNotFoundException($"EnigmaAstrology3.ttf not found at '{path}'.");

            return File.ReadAllBytes(path);
        }

        private static byte[] LoadSystemFontBytes(string fileName)
        {
            var path = Path.Combine(WindowsFontsDir, fileName);
            return File.Exists(path)
                ? File.ReadAllBytes(path)
                : throw new FileNotFoundException($"System font '{fileName}' not found in {WindowsFontsDir}.");
        }
    }
}
