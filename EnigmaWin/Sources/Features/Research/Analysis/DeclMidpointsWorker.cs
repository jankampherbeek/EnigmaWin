// DeclMidpointsWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of an occupied declination midpoint.</summary>
public sealed record DeclMidpointCount(
    Factors FactorA,
    Factors FactorB,
    Factors Occupant,
    int DataCount,
    int ControlCount);

/// <summary>Full result of a DeclMidpointsWorker run.</summary>
public sealed record DeclMidpointsResult(IReadOnlyList<DeclMidpointCount> Counts, int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum DeclMidpointsWorkerError { EquatorialNotEnabled, FileNotMapped }

public sealed class DeclMidpointsWorkerException(DeclMidpointsWorkerError error)
    : Exception($"DeclMidpointsWorker error: {error}") { public DeclMidpointsWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts occupied midpoints in declination.
/// For trio (A, B, C): midpoint = (decl_A + decl_B) / 2.
/// C occupies it when |decl_C − midpoint| ≤ orb.
/// Unlike ecliptical midpoints there is no dial wrapping; declination is linear (−90…+90°).</summary>
public sealed class DeclMidpointsWorker
{
    private readonly ResultsBinaryFile _binaryFile;
    private readonly ResearchConfig    _config;
    private readonly double _orb;

    public DeclMidpointsWorker(ResultsBinaryFile binaryFile, ResearchConfig config, double orb)
    {
        _binaryFile = binaryFile;
        _config     = config;
        _orb        = orb;
    }

    public DeclMidpointsResult Run()
    {
        if (!_config.UseEquatorial) throw new DeclMidpointsWorkerException(DeclMidpointsWorkerError.EquatorialNotEnabled);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;

        var dataCounts    = new Dictionary<TriKey, int>();
        var controlCounts = new Dictionary<TriKey, int>();
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

            for (var a = 0; a < n; a++)
            {
                if (declinations[a] is not { } dA) continue;
                for (var b = a + 1; b < n; b++)
                {
                    if (declinations[b] is not { } dB) continue;
                    var midpoint = (dA + dB) / 2.0;
                    for (var c = 0; c < n; c++)
                    {
                        if (declinations[c] is not { } dC) continue;
                        if (Math.Abs(dC - midpoint) <= _orb)
                        {
                            var key = new TriKey(a, b, c);
                            if (isData)
                            {
                                dataCounts.TryGetValue(key, out var existing);
                                dataCounts[key] = existing + 1;
                            }
                            else
                            {
                                controlCounts.TryGetValue(key, out var existing);
                                controlCounts[key] = existing + 1;
                            }
                        }
                    }
                }
            }
        }

        var counts = new List<DeclMidpointCount>();
        foreach (var (key, dc) in dataCounts)
        {
            controlCounts.TryGetValue(key, out var cc);
            counts.Add(new DeclMidpointCount(layouts[key.A].Factor, layouts[key.B].Factor,
                                             layouts[key.C].Factor, dc, cc));
        }
        foreach (var (key, cc) in controlCounts)
        {
            if (!dataCounts.ContainsKey(key))
                counts.Add(new DeclMidpointCount(layouts[key.A].Factor, layouts[key.B].Factor,
                                                 layouts[key.C].Factor, 0, cc));
        }
        counts.Sort((x, y) =>
        {
            var cmp = ((int)x.FactorA).CompareTo((int)y.FactorA);
            if (cmp != 0) return cmp;
            cmp = ((int)x.FactorB).CompareTo((int)y.FactorB);
            return cmp != 0 ? cmp : ((int)x.Occupant).CompareTo((int)y.Occupant);
        });
        return new DeclMidpointsResult(counts, skipped);
    }

    private readonly record struct TriKey(int A, int B, int C);
}
