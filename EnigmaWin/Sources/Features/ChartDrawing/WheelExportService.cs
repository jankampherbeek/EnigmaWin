// WheelExportService.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnigmaWin.Sources.Features.ChartDrawing.UI;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Progressive.DualWheel;
using EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;

namespace EnigmaWin.Sources.Features.ChartDrawing;

/// <summary>Identifies which canvas type to render during export.</summary>
public enum WheelCanvasType { Zodiac, House, French, Ring, Dial360, Dial90, Dial45, DualWheel }

/// <summary>
/// Service for exporting the horoscope wheel to PNG or PDF files.
/// Creates a fresh off-screen FrameworkElement so the live on-screen canvas is never
/// re-measured or re-arranged.
/// </summary>
public static class WheelExportService
{
    private const int ExportSize = 1200;

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
        var pngBytes  = RenderToPngBytes(plotData, theme, showAspects, canvasType);
        var rgbPixels = ExtractRgbPixelsFromPng(pngBytes, out var imgWidth, out var imgHeight);
        var pdfBytes  = BuildMinimalPdf(rgbPixels, imgWidth, imgHeight);
        File.WriteAllBytes(filePath, pdfBytes);
        return Task.CompletedTask;
    }

    public static Task ExportZodiacDivisionsToPngAsync(
        WheelPlotData plotData, ZodiacDivisionMark[] marks, WheelTheme theme, bool showAspects, string filePath)
    {
        var pngBytes = RenderZodiacDivisionsToPngBytes(plotData, marks, theme, showAspects);
        File.WriteAllBytes(filePath, pngBytes);
        return Task.CompletedTask;
    }

    public static Task ExportZodiacDivisionsToPdfAsync(
        WheelPlotData plotData, ZodiacDivisionMark[] marks, WheelTheme theme, bool showAspects, string filePath)
    {
        var pngBytes  = RenderZodiacDivisionsToPngBytes(plotData, marks, theme, showAspects);
        var rgbPixels = ExtractRgbPixelsFromPng(pngBytes, out var imgWidth, out var imgHeight);
        var pdfBytes  = BuildMinimalPdf(rgbPixels, imgWidth, imgHeight);
        File.WriteAllBytes(filePath, pdfBytes);
        return Task.CompletedTask;
    }

    public static Task ExportDualWheelToPngAsync(WheelPlotData radixData, WheelPlotItem[] transitItems,
                                                  WheelTheme theme, bool showAspects, string filePath)
    {
        var pngBytes = RenderDualWheelToPngBytes(radixData, transitItems, theme, showAspects);
        File.WriteAllBytes(filePath, pngBytes);
        return Task.CompletedTask;
    }

    public static Task ExportDualWheelToPdfAsync(WheelPlotData radixData, WheelPlotItem[] transitItems,
                                                  WheelTheme theme, bool showAspects, string filePath)
    {
        var pngBytes  = RenderDualWheelToPngBytes(radixData, transitItems, theme, showAspects);
        var rgbPixels = ExtractRgbPixelsFromPng(pngBytes, out var imgWidth, out var imgHeight);
        var pdfBytes  = BuildMinimalPdf(rgbPixels, imgWidth, imgHeight);
        File.WriteAllBytes(filePath, pdfBytes);
        return Task.CompletedTask;
    }

    private static byte[] RenderDualWheelToPngBytes(WheelPlotData radixData, WheelPlotItem[] transitItems,
                                                     WheelTheme theme, bool showAspects)
    {
        var canvas = new DualWheelCanvas
        {
            RadixData    = radixData,
            TransitItems = transitItems,
            Theme        = theme,
            ShowAspects  = showAspects
        };

        canvas.Measure(new Size(ExportSize, ExportSize));
        canvas.Arrange(new Rect(0, 0, ExportSize, ExportSize));
        canvas.UpdateLayout();

        var bitmap = new RenderTargetBitmap(ExportSize, ExportSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);

        using var ms = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.ToArray();
    }

    private static byte[] RenderZodiacDivisionsToPngBytes(
        WheelPlotData plotData, ZodiacDivisionMark[] marks, WheelTheme theme, bool showAspects)
    {
        var canvas = new ZodiacDivisionsWheelCanvas
        {
            PlotData    = plotData,
            Marks       = marks,
            Theme       = theme,
            ShowAspects = showAspects
        };

        canvas.Measure(new Size(ExportSize, ExportSize));
        canvas.Arrange(new Rect(0, 0, ExportSize, ExportSize));
        canvas.UpdateLayout();

        var bitmap = new RenderTargetBitmap(ExportSize, ExportSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);

        using var ms = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.ToArray();
    }

    private static byte[] RenderToPngBytes(WheelPlotData plotData, WheelTheme theme,
                                            bool showAspects, WheelCanvasType canvasType)
    {
        FrameworkElement canvas = canvasType switch
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
            WheelCanvasType.Ring => new RingWheelCanvas
            {
                PlotData    = plotData,
                Theme       = theme,
                ShowAspects = showAspects
            },
            WheelCanvasType.Dial360 => new Dial360WheelCanvas
            {
                PlotData = plotData,
                Theme    = theme
            },
            WheelCanvasType.Dial90 => new Dial90WheelCanvas
            {
                PlotData = plotData,
                Theme    = theme
            },
            WheelCanvasType.Dial45 => new Dial45WheelCanvas
            {
                PlotData = plotData,
                Theme    = theme
            },
            _ => new HouseWheelCanvas
            {
                PlotData = plotData,
                Theme    = theme
            }
        };

        canvas.Measure(new Size(ExportSize, ExportSize));
        canvas.Arrange(new Rect(0, 0, ExportSize, ExportSize));
        canvas.UpdateLayout();

        var bitmap = new RenderTargetBitmap(ExportSize, ExportSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas);

        using var ms = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(ms);
        return ms.ToArray();
    }

    private static byte[] ExtractRgbPixelsFromPng(byte[] pngBytes,
                                                   out int width, out int height)
    {
        var span = pngBytes.AsSpan();
        width  = ReadInt32BE(span, 16);
        height = ReadInt32BE(span, 20);
        var bitDepth  = span[24];
        var colorType = span[25];

        if (bitDepth != 8)
            throw new NotSupportedException($"PNG bit depth {bitDepth} not supported for PDF export.");

        using var idatMs = new MemoryStream();
        var pos = 8;
        while (pos < pngBytes.Length - 4)
        {
            var chunkLen  = ReadInt32BE(span, pos);
            var chunkType = Encoding.ASCII.GetString(pngBytes, pos + 4, 4);
            if (chunkType == "IDAT")
                idatMs.Write(pngBytes, pos + 8, chunkLen);
            pos += 12 + chunkLen;
            if (chunkType == "IEND") break;
        }

        var compressed = idatMs.ToArray();
        var bytesPerPixel = colorType switch
        {
            2 => 3,
            6 => 4,
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

        var rgbPixels = new byte[width * height * 3];
        var prevRow   = new byte[stride];

        for (var row = 0; row < height; row++)
        {
            var filterType = filtered[row * (stride + 1)];
            var rowStart   = row * (stride + 1) + 1;
            var rawRow     = new byte[stride];
            Array.Copy(filtered, rowStart, rawRow, 0, stride);

            ApplyPngFilter(filterType, rawRow, prevRow, bytesPerPixel);

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
            case 0:
                break;
            case 1:
                for (var i = bpp; i < row.Length; i++)
                    row[i] = (byte)(row[i] + row[i - bpp]);
                break;
            case 2:
                for (var i = 0; i < row.Length; i++)
                    row[i] = (byte)(row[i] + prev[i]);
                break;
            case 3:
                for (var i = 0; i < row.Length; i++)
                {
                    var a = i >= bpp ? row[i - bpp] : 0;
                    var b = prev[i];
                    row[i] = (byte)(row[i] + (a + b) / 2);
                }
                break;
            case 4:
                for (var i = 0; i < row.Length; i++)
                {
                    var a = i >= bpp ? row[i - bpp]  : 0;
                    var b = prev[i];
                    var c = i >= bpp ? prev[i - bpp] : 0;
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

    private static byte[] BuildMinimalPdf(byte[] rgbPixels, int imgWidth, int imgHeight)
    {
        byte[] compressed;
        using (var compressedMs = new MemoryStream())
        {
            compressedMs.WriteByte(0x78);
            compressedMs.WriteByte(0x9C);

            using (var deflate = new DeflateStream(compressedMs, CompressionLevel.Optimal, leaveOpen: true))
                deflate.Write(rgbPixels, 0, rgbPixels.Length);

            var adler = Adler32(rgbPixels);
            compressedMs.WriteByte((byte)(adler >> 24));
            compressedMs.WriteByte((byte)(adler >> 16));
            compressedMs.WriteByte((byte)(adler >> 8));
            compressedMs.WriteByte((byte)(adler));

            compressed = compressedMs.ToArray();
        }

        const double ptPerPx = 72.0 / 96.0;
        var pageW = (int)Math.Round(imgWidth  * ptPerPx);
        var pageH = (int)Math.Round(imgHeight * ptPerPx);

        using var ms = new MemoryStream();
        var offsets = new long[6];

        WriteStr(ms, "%PDF-1.4\n");
        WriteStr(ms, "%\xFF\xFF\xFF\xFF\n");

        offsets[1] = ms.Position;
        WriteStr(ms, "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        offsets[2] = ms.Position;
        WriteStr(ms, "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");

        offsets[3] = ms.Position;
        WriteStr(ms, $"3 0 obj\n" +
                     $"<< /Type /Page /Parent 2 0 R\n" +
                     $"   /MediaBox [0 0 {pageW} {pageH}]\n" +
                     $"   /Contents 4 0 R\n" +
                     $"   /Resources << /XObject << /Im1 5 0 R >> >>\n" +
                     $">>\nendobj\n");

        var contentStr   = $"q\n{pageW} 0 0 {pageH} 0 0 cm\n/Im1 Do\nQ\n";
        var contentBytes = Encoding.ASCII.GetBytes(contentStr);
        offsets[4] = ms.Position;
        WriteStr(ms, $"4 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n");
        ms.Write(contentBytes);
        WriteStr(ms, "\nendstream\nendobj\n");

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
            a = (a + bt) % mod;
            b = (b + a)  % mod;
        }
        return (b << 16) | a;
    }
}
