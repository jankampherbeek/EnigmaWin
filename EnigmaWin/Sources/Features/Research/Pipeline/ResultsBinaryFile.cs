// ResultsBinaryFile.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

// ── Header layout (256 bytes, fixed) ────────────────────────────────────────
//
//  Offset  Size  Field
//  0       8     magic: "ENIGMARS" (ASCII)
//  8       4     version: uint32 LE (currently 1)
//  12      8     recordCount: uint64 LE
//  20      4     recordSize: uint32 LE
//  24      4     factorCount: uint32 LE
//  28      4     coordinateFlags: uint32 LE  bit0=ecliptical bit1=equatorial bit2=horizontal
//  32      4*N   factorIds: uint32 LE array (N = factorCount, max 56)
//  ...     pad   zero-filled to 256 bytes
//
// Record layout:
//  0       8     recordId: int64 LE
//  8       1     isData: uint8 (1=real, 0=control)
//  9       7     padding
//  16      N     data region: per-factor blocks in Factors enum order
//
// Per factor, per enabled coordinate system: 5 doubles (40 bytes)
//   mainPos, deviation, distance, speedMain, speedDeviation
// Horizontal (if enabled): azimuth, altitude, 0, 0, 0  (padded to 40 bytes)

/// <summary>Creates, writes, and memory-maps results.bin for a research pipeline run.</summary>
public sealed class ResultsBinaryFile : IDisposable
{
    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("ENIGMARS");
    private const uint Version = 1;
    private const int HeaderSize = 256;
    private const int RecordPrefix = 16;
    private const int BytesPerCoordSystem = 40; // 5 doubles × 8 bytes

    private readonly string _filePath;
    public long RecordCount { get; }
    public int RecordSize { get; }

    private FileStream? _writeStream;
    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;

    // ── Creation ─────────────────────────────────────────────────────────────

    /// <summary>Creates a new results.bin at folderPath/results.bin and writes the 256-byte header.
    /// Overwrites any existing file. Returns the file ready for writing.</summary>
    public static ResultsBinaryFile Create(string folderPath, ResearchConfig config, long recordCount)
    {
        var path = Path.Combine(folderPath, "results.bin");
        var recordSize = config.RecordSize;
        var totalSize = HeaderSize + recordCount * recordSize;

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        fs.SetLength(totalSize);
        fs.Seek(0, SeekOrigin.Begin);

        // Build 256-byte header
        var header = new byte[HeaderSize];
        Magic.CopyTo(header, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8),  Version);
        BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(12), (ulong)recordCount);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(20), (uint)recordSize);

        var factors = config.EnabledFactors;
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(24), (uint)factors.Count);

        uint coordFlags = 0;
        if (config.UseEcliptical) coordFlags |= 1;
        if (config.UseEquatorial) coordFlags |= 2;
        if (config.UseHorizontal) coordFlags |= 4;
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(28), coordFlags);

        for (var i = 0; i < factors.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(32 + i * 4), (uint)factors[i]);

        fs.Write(header, 0, HeaderSize);

        return new ResultsBinaryFile(path, recordCount, recordSize);
    }

    // ── Open existing for read ────────────────────────────────────────────────

    /// <summary>Opens an existing results.bin for read-only analysis. Validates the header.</summary>
    public static ResultsBinaryFile OpenForReading(string folderPath)
    {
        var path = Path.Combine(folderPath, "results.bin");
        if (!File.Exists(path))
            throw new ResultsBinaryFileException($"File not found: {path}");

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var header = new byte[HeaderSize];
        if (fs.Read(header, 0, HeaderSize) < HeaderSize)
            throw new ResultsBinaryFileException("File too small to contain a valid header.");

        // Validate magic
        for (var i = 0; i < 8; i++)
            if (header[i] != Magic[i])
                throw new ResultsBinaryFileException("Invalid magic number — not a results.bin file.");

        var version = BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8));
        if (version != Version)
            throw new ResultsBinaryFileException($"Unsupported version {version}.");

        var recordCount = (long)BinaryPrimitives.ReadUInt64LittleEndian(header.AsSpan(12));
        var recordSize  = (int) BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(20));

        var expectedSize = HeaderSize + recordCount * recordSize;
        if (fs.Length != expectedSize)
            throw new ResultsBinaryFileException(
                $"File size {fs.Length} does not match expected {expectedSize}.");

        return new ResultsBinaryFile(path, recordCount, recordSize);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    private ResultsBinaryFile(string filePath, long recordCount, int recordSize)
    {
        _filePath   = filePath;
        RecordCount = recordCount;
        RecordSize  = recordSize;
    }

    // ── Writing ───────────────────────────────────────────────────────────────

    /// <summary>Opens the file for positional writing. Call once before the first WriteRecord.</summary>
    public void OpenForWriting()
    {
        _writeStream = new FileStream(_filePath, FileMode.Open, FileAccess.Write,
                                      FileShare.None, bufferSize: 65536, useAsync: false);
    }

    /// <summary>Writes one record at the given zero-based position using a pre-built byte buffer.
    /// The buffer must be exactly RecordSize bytes. Thread-unsafe — call from the SE thread only.</summary>
    public void WriteRecord(int position, byte[] buffer)
    {
        if (_writeStream is null)
            throw new InvalidOperationException("Call OpenForWriting() before WriteRecord().");
        if (buffer.Length != RecordSize)
            throw new ArgumentException($"Buffer must be {RecordSize} bytes, got {buffer.Length}.");
        if (position < 0 || position >= RecordCount)
            throw new ArgumentOutOfRangeException(nameof(position));

        var fileOffset = HeaderSize + (long)position * RecordSize;
        _writeStream.Seek(fileOffset, SeekOrigin.Begin);
        _writeStream.Write(buffer, 0, RecordSize);
    }

    /// <summary>Closes the write stream. Call after all records have been written.</summary>
    public void CloseForWriting()
    {
        _writeStream?.Flush();
        _writeStream?.Dispose();
        _writeStream = null;
    }

    // ── Memory-mapped reading ─────────────────────────────────────────────────

    /// <summary>Opens the file as a memory-mapped read-only view. Call after writing is complete.</summary>
    public void OpenMemoryMap()
    {
        _mmf      = MemoryMappedFile.CreateFromFile(_filePath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
        _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
    }

    /// <summary>Reads one record from the memory-mapped file.
    /// Returns (recordId, isData, rawDataBytes) where rawDataBytes is the data region only.</summary>
    public (long RecordId, bool IsData, byte[] Data) ReadRecord(int position)
    {
        if (_accessor is null)
            throw new InvalidOperationException("Call OpenMemoryMap() before ReadRecord().");
        if (position < 0 || position >= RecordCount)
            throw new ArgumentOutOfRangeException(nameof(position));

        var fileOffset = HeaderSize + (long)position * RecordSize;
        var buf = new byte[RecordSize];
        _accessor.ReadArray(fileOffset, buf, 0, RecordSize);

        var recordId = BinaryPrimitives.ReadInt64LittleEndian(buf.AsSpan(0, 8));
        var isData   = buf[8] != 0;
        var data     = buf[RecordPrefix..RecordSize];
        return (recordId, isData, data);
    }

    // ── Byte-writing helpers (called by the pipeline) ─────────────────────────

    /// <summary>Writes 5 little-endian doubles (mainPos, deviation, distance, speedMain, speedDeviation)
    /// into buf starting at offset. Returns the next write offset.</summary>
    public static int WriteCoordBytes(
        double v0, double v1, double v2, double v3, double v4,
        byte[] buf, int offset)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(buf.AsSpan(offset),      BitConverter.DoubleToUInt64Bits(v0)); offset += 8;
        BinaryPrimitives.WriteUInt64LittleEndian(buf.AsSpan(offset),      BitConverter.DoubleToUInt64Bits(v1)); offset += 8;
        BinaryPrimitives.WriteUInt64LittleEndian(buf.AsSpan(offset),      BitConverter.DoubleToUInt64Bits(v2)); offset += 8;
        BinaryPrimitives.WriteUInt64LittleEndian(buf.AsSpan(offset),      BitConverter.DoubleToUInt64Bits(v3)); offset += 8;
        BinaryPrimitives.WriteUInt64LittleEndian(buf.AsSpan(offset),      BitConverter.DoubleToUInt64Bits(v4)); offset += 8;
        return offset;
    }

    /// <summary>Reads one double from the data region. offset is relative to the start of the data region.</summary>
    public static double ReadDouble(byte[] dataRegion, int offset) =>
        BitConverter.Int64BitsToDouble(
            (long)BinaryPrimitives.ReadUInt64LittleEndian(dataRegion.AsSpan(offset)));

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        _writeStream?.Dispose();
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}

public sealed class ResultsBinaryFileException(string message) : Exception(message);
