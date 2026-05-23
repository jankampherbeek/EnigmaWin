// ResearchPipelineOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

// ── Progress ─────────────────────────────────────────────────────────────────

public enum PipelinePhase { Idle, ReadingInput, Calculating, WritingResults, Completed, Failed }

/// <summary>Snapshot of pipeline progress, suitable for binding to a progress bar.</summary>
public sealed record PipelineProgress(
    PipelinePhase Phase,
    int RecordsDone,
    int TotalRecords,
    Exception? Error = null)
{
    public double Fraction => TotalRecords > 0 ? (double)RecordsDone / TotalRecords : 0.0;
}

// ── Errors ────────────────────────────────────────────────────────────────────

public sealed class PipelineException(string message, Exception? inner = null)
    : Exception(message, inner);

// ── SE-serialising channel ────────────────────────────────────────────────────

/// <summary>Wraps the single-threaded Swiss Ephemeris behind a dedicated OS thread.
/// Callers post work items and await a TaskCompletionSource that the SE thread completes.</summary>
internal sealed class SeChannel : IDisposable
{
    private sealed record WorkItem(
        ResearchInputRecord Input,
        List<FactorLayout> Layouts,
        CalculationConfig CalcConfig,
        int EclFlags,
        int EqFlags,
        char HouseSystemChar,
        bool IsTopocentric,
        bool IsSidereal,
        TaskCompletionSource<byte[]> Tcs,
        byte[] Buffer);

    private readonly Channel<WorkItem> _channel =
        Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(256)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.Wait
        });

    private readonly Thread _thread;
    private readonly int _recordSize;

    public SeChannel(int recordSize)
    {
        _recordSize = recordSize;
        _thread = new Thread(RunLoop) { IsBackground = true, Name = "SE-Pipeline-Thread" };
        _thread.Start();
    }

    /// <summary>Queues one calculation and returns the filled record buffer when done.</summary>
    public async Task<byte[]> CalculateAsync(
        ResearchInputRecord input,
        List<FactorLayout> layouts,
        CalculationConfig calcConfig,
        int eclFlags,
        int eqFlags,
        char houseSystemChar,
        bool isTopocentric,
        bool isSidereal,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var tcs    = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        var buffer = new byte[_recordSize];
        var item   = new WorkItem(input, layouts, calcConfig, eclFlags, eqFlags,
                                  houseSystemChar, isTopocentric, isSidereal, tcs, buffer);

        await _channel.Writer.WriteAsync(item, ct);

        using var reg = ct.Register(() => tcs.TrySetCanceled(ct));
        return await tcs.Task;
    }

    public void Complete() => _channel.Writer.TryComplete();

    public void Dispose()
    {
        Complete();
        _thread.Join(TimeSpan.FromSeconds(5));
    }

    // ── SE thread loop ────────────────────────────────────────────────────────

    private void RunLoop()
    {
        // Each loop body processes one record entirely on this thread — SE is never called from any other thread.
        var reader = _channel.Reader;
        while (reader.WaitToReadAsync().AsTask().GetAwaiter().GetResult())
        {
            while (reader.TryRead(out var item))
            {
                try
                {
                    FillBuffer(item);
                    item.Tcs.TrySetResult(item.Buffer);
                }
                catch (Exception ex)
                {
                    item.Tcs.TrySetException(ex);
                }
            }
        }
    }

    private static void FillBuffer(WorkItem w)
    {
        var r     = w.Input;
        var buf   = w.Buffer;

        // ── Prefix: recordId (int64 LE) + isData + 7 pad ─────────────────────
        Array.Clear(buf, 0, 16);
        BinaryPrimitives.WriteInt64LittleEndian(buf.AsSpan(0), r.Id);
        buf[8] = r.IsData ? (byte)1 : (byte)0;

        // ── Julian Day ────────────────────────────────────────────────────────
        var utHour = r.Hour + r.Minute / 60.0 + r.Second / 3600.0 - r.Offset;
        var date   = new AstronomicalDate(r.Year, r.Month, r.Day);
        var time   = new AstronomicalTime(utHour);
        var julDay = SEWrapper.JulianDay(date, time);

        if (w.IsTopocentric)
            SEWrapper.SetTopocentric(r.GeoLon, r.GeoLat, 0);

        // ── Per-factor calculation ────────────────────────────────────────────
        var offset = 16; // data region starts after 16-byte prefix
        var seWrapper = new SEWrapper();

        foreach (var layout in w.Layouts)
        {
            switch (layout.Factor.CalculationType())
            {
                case CalculationTypes.CommonSe:
                {
                    // North/South node: honour LunarNodeType via enum variant
                    var seId = layout.Factor.SeId();
                    if (layout.HasEcliptical)
                    {
                        var pos = SEWrapper.CalculateFactorPosition(julDay, seId, w.EclFlags);
                        offset = pos is not null
                            ? ResultsBinaryFile.WriteCoordBytes(
                                pos.MainPos, pos.Deviation, pos.Distance, pos.MainPosSpeed, pos.DeviationSpeed,
                                buf, offset)
                            : offset + 40;
                    }
                    if (layout.HasEquatorial)
                    {
                        var pos = SEWrapper.CalculateFactorPosition(julDay, seId, w.EqFlags);
                        offset = pos is not null
                            ? ResultsBinaryFile.WriteCoordBytes(
                                pos.MainPos, pos.Deviation, pos.Distance, pos.MainPosSpeed, pos.DeviationSpeed,
                                buf, offset)
                            : offset + 40;
                    }
                    break;
                }

                case CalculationTypes.Mundane:
                {
                    // Asc=701→ascmc[0], MC=700→ascmc[1], EastPoint=702→ascmc[4], Vertex=703→ascmc[3]
                    var ascmcIndex = layout.Factor.SeId() switch
                    {
                        700 => 1,
                        701 => 0,
                        702 => 4,
                        703 => 3,
                        _   => -1
                    };
                    double? lon = null;
                    if (ascmcIndex >= 0)
                    {
                        try
                        {
                            var result = seWrapper.CalculateHouses(julDay, 0, r.GeoLat, r.GeoLon, w.HouseSystemChar);
                            if (result[1].Length > ascmcIndex) lon = result[1][ascmcIndex];
                        }
                        catch { /* leave lon null */ }
                    }
                    if (layout.HasEcliptical)
                    {
                        offset = lon.HasValue
                            ? ResultsBinaryFile.WriteCoordBytes(lon.Value, 0, 0, 0, 0, buf, offset)
                            : offset + 40;
                    }
                    if (layout.HasEquatorial)
                    {
                        if (lon.HasValue)
                        {
                            var oblPos  = SEWrapper.CalculateFactorPosition(julDay, -1, 2);
                            var obl     = oblPos?.MainPos ?? 23.4393;
                            var (ra, dec) = seWrapper.EclipticToEquatorial([lon.Value, 0.0], obl);
                            offset = ResultsBinaryFile.WriteCoordBytes(ra, dec, 0, 0, 0, buf, offset);
                        }
                        else offset += 40;
                    }
                    break;
                }

                default:
                    // Not yet implemented in the research pipeline — advance to keep offsets correct.
                    offset += layout.ByteSize;
                    break;
            }
        }
    }
}

// ── Orchestrator ─────────────────────────────────────────────────────────────

/// <summary>Runs the full research calculation pipeline for one ResearchProject.
///
/// Pipeline flow:
///   1. Read all ResearchInputRecord rows from horoscope_data.db
///   2. Create results.bin with the correct header (pre-allocated)
///   3. For each record: post to the dedicated SE thread, await the filled buffer
///   4. Write each buffer sequentially to results.bin
///   5. Report IProgress&lt;PipelineProgress&gt; updates (throttled to ~200 max)
///   6. Honour CancellationToken between records
/// </summary>
public sealed class ResearchPipelineOrchestrator
{
    /// <summary>Runs the pipeline. Heavy work executes on Task.Run (thread-pool);
    /// progress is marshalled back to the calling context via IProgress&lt;T&gt;,
    /// which (when created on the UI thread) posts to the UI dispatcher automatically.</summary>
    public Task RunAsync(
        string projectPath,
        ResearchConfig config,
        IProgress<PipelineProgress> progress,
        CancellationToken ct = default) =>
        Task.Run(() => ExecuteAsync(projectPath, config, progress, ct).GetAwaiter().GetResult(), ct);

    private static async Task ExecuteAsync(
        string path,
        ResearchConfig config,
        IProgress<PipelineProgress> progress,
        CancellationToken ct)
    {
        var calcConfig = config.CalculationConfig
            ?? throw new PipelineException("ResearchConfig contains no valid CalculationConfig JSON.");

        // ── Phase 1: Read input ───────────────────────────────────────────────
        progress.Report(new PipelineProgress(PipelinePhase.ReadingInput, 0, 0));

        List<ResearchInputRecord> records;
        using (var db = new ResearchDbManager(path))
            records = new List<ResearchInputRecord>(db.FetchAll());

        var total = records.Count;
        if (total == 0)
        {
            progress.Report(new PipelineProgress(PipelinePhase.Completed, 0, 0));
            return;
        }

        // ── Phase 2: Create results.bin ───────────────────────────────────────
        var binPath = Path.Combine(path, "results.bin");
        if (File.Exists(binPath)) File.Delete(binPath);

        using var binFile = ResultsBinaryFile.Create(path, config, total);
        binFile.OpenForWriting();

        // ── Phase 3+4: Calculate + write (SE thread serialises all SE calls) ──
        progress.Report(new PipelineProgress(PipelinePhase.Calculating, 0, total));

        var layouts         = config.FactorLayouts;
        var eclFlags        = SEFlags.DefineFlags(calcConfig, CoordinateSystems.Ecliptical);
        var eqFlags         = SEFlags.DefineFlags(calcConfig, CoordinateSystems.Equatorial);
        var houseSystemChar = calcConfig.HouseSystem.SeId();
        var isTopocentric   = calcConfig.ObserverPosition == ObserverPositions.Topocentric;
        var isSidereal      = calcConfig.Ayanamsha != Ayanamshas.Tropical;

        if (isSidereal) SEWrapper.SetAyanamsha(calcConfig.Ayanamsha.SeId());

        var updateInterval = Math.Max(100, total / 200);
        var lastPublished  = 0;

        using var seChannel = new SeChannel(config.RecordSize);

        for (var i = 0; i < records.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            var buf = await seChannel.CalculateAsync(
                records[i], layouts, calcConfig,
                eclFlags, eqFlags, houseSystemChar,
                isTopocentric, isSidereal, ct);

            binFile.WriteRecord(i, buf);

            var done = i + 1;
            if (done - lastPublished >= updateInterval || done == total)
            {
                lastPublished = done;
                progress.Report(new PipelineProgress(PipelinePhase.Calculating, done, total));
            }
        }

        seChannel.Complete();
        binFile.CloseForWriting();
        progress.Report(new PipelineProgress(PipelinePhase.Completed, total, total));
    }
}
