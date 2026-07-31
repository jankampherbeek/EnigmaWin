// SynastryMidpointComparisonPdfExporter.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace EnigmaWin.Sources.Features.Synastry.UI;

/// <summary>Column labels for the synastry midpoint comparison PDF export header rows.</summary>
public sealed record SynastryMidpointComparisonPdfLabels(
    string Title, string ChartAName, string ChartBName, string ColOrb, string ColExactness);

/// <summary>
/// Builds the Synastry midpoint comparison PDF as real vector text. Renders both tables
/// (chart A's own midpoints occupied by chart B, and vice versa) one below the other.
/// </summary>
public static class SynastryMidpointComparisonPdfExporter
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
    private const double ColMidpoint = 90;
    private const double ColOrb      = 60;
    private const double ColExactness = 60;

    static SynastryMidpointComparisonPdfExporter()
    {
        if (GlobalFontSettings.FontResolver is null)
            GlobalFontSettings.FontResolver = new EmbeddedGlyphFontResolver();
    }

    public static void Export(string filePath, SynastryMidpointComparisonPdfLabels labels,
                               IReadOnlyList<SynastryMidpointRow> rowsA, IReadOnlyList<SynastryMidpointRow> rowsB)
    {
        var document = new PdfDocument();
        var page     = document.AddPage();
        page.Width   = XUnit.FromPoint(595);
        page.Height  = XUnit.FromPoint(842);

        var gfx = XGraphics.FromPdfPage(page);

        var unicode = XPdfFontOptions.UnicodeDefault;
        var titleFont  = new XFont("Segoe UI", TitleSize, XFontStyleEx.Bold, unicode);
        var headerFont = new XFont("Segoe UI", HeaderSize, XFontStyleEx.Bold, unicode);
        var monoFont   = new XFont("Courier New", RowSize, XFontStyleEx.Regular, unicode);
        var glyphFont  = new XFont(GlyphFontFamily, GlyphSize, XFontStyleEx.Regular, unicode);

        var x = MarginPt;
        double y = MarginPt;
        var pageBottom = page.Height.Point - MarginPt;

        gfx.DrawString(labels.Title, titleFont, XBrushes.Black, new XPoint(x, y));
        y += TitleSize + 14;

        void DrawSection(string chartName, IReadOnlyList<SynastryMidpointRow> rows)
        {
            if (y > pageBottom - RowHeight * 3)
            {
                page    = document.AddPage();
                page.Width  = XUnit.FromPoint(595);
                page.Height = XUnit.FromPoint(842);
                gfx     = XGraphics.FromPdfPage(page);
                y       = MarginPt;
            }

            gfx.DrawString(chartName, headerFont, XBrushes.Black, new XPoint(x, y));
            y += RowHeight;

            double cx = x + ColGlyph * 3 + ColMidpoint + ColGlyph;
            gfx.DrawString(labels.ColOrb, headerFont, XBrushes.Black, new XPoint(cx, y)); cx += ColOrb;
            gfx.DrawString(labels.ColExactness, headerFont, XBrushes.Black, new XPoint(cx, y));
            y += RowHeight;
            gfx.DrawLine(XPens.Gray, x, y, x + ColGlyph * 3 + ColMidpoint + ColGlyph + ColOrb + ColExactness, y);
            y += 8;

            foreach (var row in rows)
            {
                if (y > pageBottom)
                {
                    page    = document.AddPage();
                    page.Width  = XUnit.FromPoint(595);
                    page.Height = XUnit.FromPoint(842);
                    gfx     = XGraphics.FromPdfPage(page);
                    y       = MarginPt;
                }

                double rx = x;
                gfx.DrawString(row.Glyph1, glyphFont, XBrushes.Black, new XPoint(rx, y + GlyphSize - RowSize)); rx += ColGlyph;
                gfx.DrawString(row.Glyph2, glyphFont, XBrushes.Black, new XPoint(rx, y + GlyphSize - RowSize)); rx += ColGlyph;
                gfx.DrawString(row.MidpointDms, monoFont, XBrushes.Black, new XPoint(rx, y));
                var midWidth = gfx.MeasureString(row.MidpointDms, monoFont).Width;
                if (!string.IsNullOrEmpty(row.MidpointSignGlyph))
                    gfx.DrawString(row.MidpointSignGlyph, glyphFont, XBrushes.Black, new XPoint(rx + midWidth + 2, y + GlyphSize - RowSize));
                rx += ColMidpoint;
                gfx.DrawString(row.PartnerGlyph, glyphFont, XBrushes.Black, new XPoint(rx, y + GlyphSize - RowSize)); rx += ColGlyph;
                gfx.DrawString(row.OrbText, monoFont, XBrushes.Black, new XPoint(rx, y)); rx += ColOrb;
                gfx.DrawString(row.ExactnessText, monoFont, XBrushes.Black, new XPoint(rx, y));

                y += RowHeight;
            }

            y += 12;
        }

        DrawSection(labels.ChartAName, rowsA);
        DrawSection(labels.ChartBName, rowsB);

        document.Save(filePath);
    }

    /// <summary>
    /// Resolves the embedded EnigmaAstrology3 glyph font plus the small set of system fonts
    /// used in the export (Segoe UI, Courier New), loaded directly from the Windows Fonts folder.
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
