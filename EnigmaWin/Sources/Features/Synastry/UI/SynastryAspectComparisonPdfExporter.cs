// SynastryAspectComparisonPdfExporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Column labels for the synastry aspect comparison PDF export header rows.</summary>
public sealed record SynastryAspectComparisonPdfLabels(
    string Title, string ChartAName, string ChartBName, string ColOrb);

/// <summary>
/// Builds the Synastry aspect comparison PDF as real vector text (selectable, searchable, crisp
/// at any zoom) instead of rasterizing the on-screen tables to a bitmap. Renders both directional
/// tables (from chart A's perspective and from chart B's perspective) side by side.
/// </summary>
public static class SynastryAspectComparisonPdfExporter
{
    private const string GlyphFontFamily = "EnigmaAstrology3";
    private const string GlyphFontResourcePath = "Resources/Fonts/EnigmaAstrology3.ttf";

    private const double MarginPt   = 36;
    private const double TitleSize  = 14;
    private const double HeaderSize = 10;
    private const double RowSize    = 10;
    private const double GlyphSize  = 13;
    private const double RowHeight  = 16;

    private const double ColGlyph = 22;
    private const double ColOrb   = 60;
    private const double TableGap = 30;

    static SynastryAspectComparisonPdfExporter()
    {
        if (GlobalFontSettings.FontResolver is null)
            GlobalFontSettings.FontResolver = new EmbeddedGlyphFontResolver();
    }

    public static void Export(string filePath, SynastryAspectComparisonPdfLabels labels,
                               IReadOnlyList<SynastryAspectRow> rowsFromA, IReadOnlyList<SynastryAspectRow> rowsFromB)
    {
        var document = new PdfDocument();
        var page     = document.AddPage();
        page.Width   = XUnit.FromPoint(595);  // A4 portrait
        page.Height  = XUnit.FromPoint(842);

        var gfx = XGraphics.FromPdfPage(page);

        var unicode = XPdfFontOptions.UnicodeDefault;
        var titleFont  = new XFont("Segoe UI", TitleSize, XFontStyleEx.Bold, unicode);
        var headerFont = new XFont("Segoe UI", HeaderSize, XFontStyleEx.Bold, unicode);
        var monoFont   = new XFont("Courier New", RowSize, XFontStyleEx.Regular, unicode);
        var glyphFont  = new XFont(GlyphFontFamily, GlyphSize, XFontStyleEx.Regular, unicode);

        var xLeft  = MarginPt;
        var xRight = MarginPt + ColGlyph * 3 + ColOrb + TableGap;
        double y   = MarginPt;
        var pageBottom = page.Height.Point - MarginPt;

        gfx.DrawString(labels.Title, titleFont, XBrushes.Black, new XPoint(xLeft, y));
        y += TitleSize + 14;

        void DrawHeader()
        {
            gfx.DrawString(labels.ChartAName, headerFont, XBrushes.Black, new XPoint(xLeft, y));
            gfx.DrawString(labels.ChartBName, headerFont, XBrushes.Black, new XPoint(xRight, y));
            y += RowHeight;

            double cx = xLeft + ColGlyph * 3;
            gfx.DrawString(labels.ColOrb, headerFont, XBrushes.Black, new XPoint(cx, y));
            cx = xRight + ColGlyph * 3;
            gfx.DrawString(labels.ColOrb, headerFont, XBrushes.Black, new XPoint(cx, y));
            y += RowHeight;

            gfx.DrawLine(XPens.Gray, xLeft, y, xLeft + ColGlyph * 3 + ColOrb, y);
            gfx.DrawLine(XPens.Gray, xRight, y, xRight + ColGlyph * 3 + ColOrb, y);
            y += 8;
        }

        DrawHeader();

        var rowCount = Math.Max(rowsFromA.Count, rowsFromB.Count);
        for (var i = 0; i < rowCount; i++)
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

            if (i < rowsFromA.Count)
                DrawRow(gfx, xLeft, y, rowsFromA[i], glyphFont, monoFont);
            if (i < rowsFromB.Count)
                DrawRow(gfx, xRight, y, rowsFromB[i], glyphFont, monoFont);

            y += RowHeight;
        }

        document.Save(filePath);
    }

    private static void DrawRow(XGraphics gfx, double x, double y, SynastryAspectRow row, XFont glyphFont, XFont monoFont)
    {
        var cx = x;
        gfx.DrawString(row.RadixGlyph, glyphFont, XBrushes.Black, new XPoint(cx, y + GlyphSize - RowSize));
        cx += ColGlyph;
        gfx.DrawString(row.AspectGlyph, glyphFont, XBrushes.Black, new XPoint(cx, y + GlyphSize - RowSize));
        cx += ColGlyph;
        gfx.DrawString(row.PartnerGlyph, glyphFont, XBrushes.Black, new XPoint(cx, y + GlyphSize - RowSize));
        cx += ColGlyph;
        gfx.DrawString(row.OrbText, monoFont, XBrushes.Black, new XPoint(cx, y));
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
