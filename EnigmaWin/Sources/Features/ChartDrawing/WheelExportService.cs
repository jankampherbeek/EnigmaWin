// WheelExportService.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia;
using EnigmaWin.Sources.Features.ChartDrawing.UI;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace EnigmaWin.Sources.Features.ChartDrawing;

/// <summary>Identifies which canvas type to render during export.</summary>
public enum WheelCanvasType { Zodiac, House, French }

/// <summary>
/// Service for exporting the horoscope wheel to PNG or PDF files.
/// Creates a fresh off-screen canvas so the live on-screen canvas is never
/// re-measured or re-arranged — avoiding the post-export zoom issue.
/// </summary>
public static class WheelExportService
{
    private const int ExportSize = 1200;

    // MARK: - Public API

    public static Task ExportToPngAsync(WheelPlotData plotData, WheelTheme theme,
                                        bool showAspects, WheelCanvasType canvasType, string filePath)
    {
        var pngBytes = RenderToPngBytes(plotData, theme, showAspects, canvasType);
        File.WriteAllBytes(filePath, pngBytes);
        return Task.CompletedTask;
    }

    public static Task ExportToPdfAsync(WheelPlotData plotData, WheelTheme theme,
                                        bool showAspects, WheelCanvasType canvasType, string filePath)
    {
        var pngBytes = RenderToPngBytes(plotData, theme, showAspects, canvasType);
        var rgbPixels = ExtractRgbPixelsFromPng(pngBytes, out var imgWidth, out var imgHeight);
        var pdfBytes  = BuildMinimalPdf(rgbPixels, imgWidth, imgHeight);
        File.WriteAllBytes(filePath, pdfBytes);
        return Task.CompletedTask;
    }

    // MARK: - PNG rendering

    private static byte[] RenderToPngBytes(WheelPlotData plotData, WheelTheme theme,
                                            bool showAspects, WheelCanvasType canvasType)
    {
        // Create a fresh off-screen canvas — never touches the live UI control.
        Control canvas = canvasType switch
        {
            WheelCanvasType.Zodiac => new ZodiacWheelCanvas
            {
                PlotData    = plotData,
                Theme       = theme,
                ShowAspects = showAspects
            },
            WheelCanvasType.French => new FrenchWheelCanvas
            {
                PlotData    = plotData,
                Theme       = theme,
                ShowAspects = showAspects
            },
            _ => new HouseWheelCanvas
            {
                PlotData = plotData,
                Theme    = theme
            }
        };

        var size   = new PixelSize(ExportSize, ExportSize);
        var dpi    = new Vector(96, 96);
        var bitmap = new RenderTargetBitmap(size, dpi);

        var renderSize = new Size(ExportSize, ExportSize);
        canvas.Measure(renderSize);
        canvas.Arrange(new Rect(renderSize));

        bitmap.Render(canvas);

        using var ms = new MemoryStream();
        bitmap.Save(ms);
        return ms.ToArray();
    }

    // MARK: - PNG pixel extraction

    /// <summary>
    /// Extracts raw RGB (24-bit, no alpha) pixel rows from a PNG byte array.
    /// Returns a flat byte array: rows * width * 3 bytes (R G B per pixel, top-to-bottom).
    /// </summary>
    private static byte[] ExtractRgbPixelsFromPng(byte[] pngBytes,
                                                   out int width, out int height)
    {
        // PNG structure:
        //   8-byte signature
        //   IHDR chunk (13 bytes of data: width[4] height[4] bitdepth[1] colortype[1] ...)
        //   IDAT chunk(s) — zlib-compressed filtered rows
        //   IEND chunk

        // Read IHDR
        // Signature: 8 bytes
        var span = pngBytes.AsSpan();
        width  = ReadInt32BE(span, 16);
        height = ReadInt32BE(span, 20);
        var bitDepth  = span[24];
        var colorType = span[25]; // 2=RGB, 6=RGBA

        if (bitDepth != 8)
            throw new NotSupportedException($"PNG bit depth {bitDepth} not supported for PDF export.");

        // Collect all IDAT chunk data
        using var idatMs = new MemoryStream();
        var pos = 8; // skip signature
        while (pos < pngBytes.Length - 4)
        {
            var chunkLen  = ReadInt32BE(span, pos);
            var chunkType = Encoding.ASCII.GetString(pngBytes, pos + 4, 4);
            if (chunkType == "IDAT")
                idatMs.Write(pngBytes, pos + 8, chunkLen);
            pos += 12 + chunkLen; // length(4) + type(4) + data + crc(4)
            if (chunkType == "IEND") break;
        }

        // Decompress zlib data (skip 2-byte zlib header)
        var compressed = idatMs.ToArray();
        var bytesPerPixel = colorType switch
        {
            2 => 3, // RGB
            6 => 4, // RGBA
            _ => throw new NotSupportedException($"PNG color type {colorType} not supported for PDF export.")
        };
        var stride = width * bytesPerPixel;

        byte[] filtered;
        using (var zlibInput  = new MemoryStream(compressed, 2, compressed.Length - 2))
        using (var deflate    = new DeflateStream(zlibInput, CompressionMode.Decompress))
        using (var deflateOut = new MemoryStream())
        {
            deflate.CopyTo(deflateOut);
            filtered = deflateOut.ToArray();
        }

        // Reverse PNG filter to get raw pixels
        var rgbPixels = new byte[width * height * 3];
        var prevRow   = new byte[stride];

        for (var row = 0; row < height; row++)
        {
            var filterType  = filtered[row * (stride + 1)];
            var rowStart    = row * (stride + 1) + 1;
            var rawRow      = new byte[stride];
            Array.Copy(filtered, rowStart, rawRow, 0, stride);

            ApplyPngFilter(filterType, rawRow, prevRow, bytesPerPixel);

            // Copy RGB (skip alpha if RGBA)
            for (var x = 0; x < width; x++)
            {
                var srcIdx  = x * bytesPerPixel;
                var dstIdx  = (row * width + x) * 3;
                rgbPixels[dstIdx]     = rawRow[srcIdx];
                rgbPixels[dstIdx + 1] = rawRow[srcIdx + 1];
                rgbPixels[dstIdx + 2] = rawRow[srcIdx + 2];
            }

            Array.Copy(rawRow, prevRow, stride);
        }

        return rgbPixels;
    }

    private static void ApplyPngFilter(byte filterType, byte[] row, byte[] prev, int bpp)
    {
        switch (filterType)
        {
            case 0: // None
                break;

            case 1: // Sub
                for (var i = bpp; i < row.Length; i++)
                    row[i] = (byte)(row[i] + row[i - bpp]);
                break;

            case 2: // Up
                for (var i = 0; i < row.Length; i++)
                    row[i] = (byte)(row[i] + prev[i]);
                break;

            case 3: // Average
                for (var i = 0; i < row.Length; i++)
                {
                    var a = i >= bpp ? row[i - bpp] : 0;
                    var b = prev[i];
                    row[i] = (byte)(row[i] + (a + b) / 2);
                }
                break;

            case 4: // Paeth
                for (var i = 0; i < row.Length; i++)
                {
                    var a = i >= bpp ? row[i - bpp]      : 0;
                    var b = prev[i];
                    var c = i >= bpp ? prev[i - bpp]     : 0;
                    row[i] = (byte)(row[i] + PaethPredictor(a, b, c));
                }
                break;
        }
    }

    private static int PaethPredictor(int a, int b, int c)
    {
        var p  = a + b - c;
        var pa = Math.Abs(p - a);
        var pb = Math.Abs(p - b);
        var pc = Math.Abs(p - c);
        return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
    }

    private static int ReadInt32BE(ReadOnlySpan<byte> data, int offset)
        => (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];

    // MARK: - Minimal PDF builder

    /// <summary>
    /// Builds a minimal single-page PDF that embeds the raw RGB pixel data as a
    /// FlateDecode-compressed image XObject. No external libraries needed.
    /// </summary>
    private static byte[] BuildMinimalPdf(byte[] rgbPixels, int imgWidth, int imgHeight)
    {
        // Compress the raw RGB rows with zlib (FlateDecode)
        byte[] compressed;
        using (var compressedMs = new MemoryStream())
        {
            // Write zlib header (CMF=0x78, FLG=0x9C → deflate, default compression)
            compressedMs.WriteByte(0x78);
            compressedMs.WriteByte(0x9C);

            using (var deflate = new DeflateStream(compressedMs, CompressionLevel.Optimal, leaveOpen: true))
                deflate.Write(rgbPixels, 0, rgbPixels.Length);

            // Append Adler-32 checksum (required by zlib)
            var adler = Adler32(rgbPixels);
            compressedMs.WriteByte((byte)(adler >> 24));
            compressedMs.WriteByte((byte)(adler >> 16));
            compressedMs.WriteByte((byte)(adler >> 8));
            compressedMs.WriteByte((byte)(adler));

            compressed = compressedMs.ToArray();
        }

        // Page size in PDF points (pt = px * 72 / 96)
        const double ptPerPx = 72.0 / 96.0;
        var pageW = (int)Math.Round(imgWidth  * ptPerPx);
        var pageH = (int)Math.Round(imgHeight * ptPerPx);

        // Build PDF objects, recording byte offsets for xref
        using var ms = new MemoryStream();
        var offsets = new long[6];

        WriteStr(ms, "%PDF-1.4\n");
        WriteStr(ms, "%\xFF\xFF\xFF\xFF\n");

        // Obj 1: Catalog
        offsets[1] = ms.Position;
        WriteStr(ms, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        // Obj 2: Pages
        offsets[2] = ms.Position;
        WriteStr(ms, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        // Obj 3: Page
        offsets[3] = ms.Position;
        WriteStr(ms, $"3 0 obj\n" +
                     $"<< /Type /Page /Parent 2 0 R\n" +
                     $"   /MediaBox [0 0 {pageW} {pageH}]\n" +
                     $"   /Contents 4 0 R\n" +
                     $"   /Resources << /XObject << /Im1 5 0 R >> >>\n" +
                     $">>\nendobj\n");

        // Obj 4: Content stream — place image covering whole page
        var contentStr   = $"q\n{pageW} 0 0 {pageH} 0 0 cm\n/Im1 Do\nQ\n";
        var contentBytes = Encoding.ASCII.GetBytes(contentStr);
        offsets[4] = ms.Position;
        WriteStr(ms, $"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        ms.Write(contentBytes);
        WriteStr(ms, "\nendstream\nendobj\n");

        // Obj 5: Image XObject
        offsets[5] = ms.Position;
        WriteStr(ms, $"5 0 obj\n" +
                     $"<< /Type /XObject /Subtype /Image\n" +
                     $"   /Width {imgWidth} /Height {imgHeight}\n" +
                     $"   /ColorSpace /DeviceRGB\n" +
                     $"   /BitsPerComponent 8\n" +
                     $"   /Filter /FlateDecode\n" +
                     $"   /Length {compressed.Length}\n" +
                     $">>\nstream\n");
        ms.Write(compressed);
        WriteStr(ms, "\nendstream\nendobj\n");

        // xref
        var xrefPos = ms.Position;
        WriteStr(ms, "xref\n0 6\n");
        WriteStr(ms, "0000000000 65535 f \n");
        for (var i = 1; i <= 5; i++)
            WriteStr(ms, $"{offsets[i]:D10} 00000 n \n");

        WriteStr(ms, $"trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF\n");

        return ms.ToArray();
    }

    private static void WriteStr(Stream s, string text)
    {
        var bytes = Encoding.ASCII.GetBytes(text);
        s.Write(bytes, 0, bytes.Length);
    }

    private static uint Adler32(byte[] data)
    {
        const uint mod = 65521;
        uint a = 1, b = 0;
        foreach (var bt in data)
        {
            a = (a + bt)  % mod;
            b = (b + a)   % mod;
        }
        return (b << 16) | a;
    }
}
