// ParallelsWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of a parallel or contra-parallel between two factors.</summary>
public sealed record ParallelCount(
    Factors Factor1,
    Factors Factor2,
    bool IsContraParallel,
    int DataCount,
    int ControlCount);

/// <summary>Full result of a ParallelsWorker run.</summary>
public sealed record ParallelsResult(IReadOnlyList<ParallelCount> Counts, int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum ParallelsWorkerError { EquatorialNotEnabled, FileNotMapped }

public sealed class ParallelsWorkerException(ParallelsWorkerError error)
    : Exception($"ParallelsWorker error: {error}") { public ParallelsWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts declination parallels and contra-parallels.
/// Parallel:        |decl_A − decl_B| ≤ orb
/// Contra-parallel: |decl_A + decl_B| ≤ orb
/// Declination is the second double (offset +8) in the equatorial block.</summary>
public sealed class ParallelsWorker
{
    private readonly ResultsBinaryFile _binaryFile;
    private readonly ResearchConfig    _config;
    private readonly double _orb;

    public ParallelsWorker(ResultsBinaryFile binaryFile, ResearchConfig config, double orb)
    {
        _binaryFile = binaryFile;
        _config     = config;
        _orb        = orb;
    }

    public ParallelsResult Run()
    {
        if (!_config.UseEquatorial) throw new ParallelsWorkerException(ParallelsWorkerError.EquatorialNotEnabled);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;

        // [i][j][0] = parallel, [i][j][1] = contra-parallel  (i < j)
        var dataCounts    = new int[n][][];
        var controlCounts = new int[n][][];
        for (var i = 0; i < n; i++)
        {
            dataCounts[i]    = new int[n][];
            controlCounts[i] = new int[n][];
            for (var j = 0; j < n; j++) { dataCounts[i][j] = new int[2]; controlCounts[i][j] = new int[2]; }
        }
        var skipped = 0;

        for (var position = 0; position < recordCount; position++)
        {
            bool isData; byte[] regionData;
            try { var rec = _binaryFile.ReadRecord(position); isData = rec.IsData; regionData = rec.Data; }
            catch { skipped++; continue; }
            var declinations = new double?[n];
            for (var fi = 0; fi < n; fi++)
            {
                var layout = layouts[fi];
                if (!layout.HasEquatorial) continue;
                try { declinations[fi] = ResultsBinaryFile.ReadDouble(regionData, layout.ByteOffset + 8); }
                catch { /* leave null */ }
            }

            for (var i = 0; i < n; i++)
            {
                if (declinations[i] is not { } declI) continue;
                for (var j = i + 1; j < n; j++)
                {
                    if (declinations[j] is not { } declJ) continue;
                    if (Math.Abs(declI - declJ) <= _orb)
                    {
                        if (isData) dataCounts[i][j][0]++;
                        else        controlCounts[i][j][0]++;
                    }
                    if (Math.Abs(declI + declJ) <= _orb)
                    {
                        if (isData) dataCounts[i][j][1]++;
                        else        controlCounts[i][j][1]++;
                    }
                }
            }
        }

        var counts = new List<ParallelCount>();
        for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
                for (var k = 0; k < 2; k++)
                {
                    var dc = dataCounts[i][j][k];
                    var cc = controlCounts[i][j][k];
                    if (dc > 0 || cc > 0)
                        counts.Add(new ParallelCount(layouts[i].Factor, layouts[j].Factor,
                                                     k == 1, dc, cc));
                }

        return new ParallelsResult(counts, skipped);
    }
}
