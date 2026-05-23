// MidpointsWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of an occupied midpoint (factorA/factorB midpoint occupied by factorC).</summary>
public sealed record MidpointCount(
    Factors FactorA,
    Factors FactorB,
    Factors Occupant,
    int DialSize,
    int DataCount,
    int ControlCount);

/// <summary>Full result of a MidpointsWorker run.</summary>
public sealed record MidpointsResult(IReadOnlyList<MidpointCount> Counts, int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum MidpointsWorkerError { EclipticalNotEnabled, FileNotMapped, NoDialSizesConfigured }

public sealed class MidpointsWorkerException(MidpointsWorkerError error)
    : Exception($"MidpointsWorker error: {error}") { public MidpointsWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts occupied midpoints on one or more dial sizes.
/// For dial d: reduce all longitudes mod d, compute midpoint of A and B as (lonA + lonB) / 2 mod d,
/// then check if factorC's reduced longitude is within orb of that midpoint.</summary>
public sealed class MidpointsWorker
{
    private readonly ResultsBinaryFile         _binaryFile;
    private readonly ResearchConfig            _config;
    private readonly IReadOnlyList<int>        _dialSizes;
    private readonly IReadOnlyDictionary<int, double> _orbsPerDialSize;

    public MidpointsWorker(ResultsBinaryFile binaryFile, ResearchConfig config,
                           IReadOnlyList<int> dialSizes, IReadOnlyDictionary<int, double> orbsPerDialSize)
    {
        _binaryFile      = binaryFile;
        _config          = config;
        _dialSizes       = dialSizes;
        _orbsPerDialSize = orbsPerDialSize;
    }

    public MidpointsResult Run()
    {
        if (!_config.UseEcliptical) throw new MidpointsWorkerException(MidpointsWorkerError.EclipticalNotEnabled);
        if (_dialSizes.Count == 0)  throw new MidpointsWorkerException(MidpointsWorkerError.NoDialSizesConfigured);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;

        var dataCounts    = new Dictionary<FourKey, int>();
        var controlCounts = new Dictionary<FourKey, int>();
        var skipped = 0;

        for (var position = 0; position < recordCount; position++)
        {
            bool isData; byte[] regionData;
            try { var rec = _binaryFile.ReadRecord(position); isData = rec.IsData; regionData = rec.Data; }
            catch { skipped++; continue; }
            var longitudes = new double?[n];
            for (var fi = 0; fi < n; fi++)
            {
                var layout = layouts[fi];
                if (!layout.HasEcliptical) continue;
                try
                {
                    var lon = ResultsBinaryFile.ReadDouble(regionData, layout.ByteOffset);
                    longitudes[fi] = ((lon % 360) + 360) % 360;
                }
                catch { /* leave null */ }
            }

            for (var di = 0; di < _dialSizes.Count; di++)
            {
                var dial = _dialSizes[di];
                var d    = (double)dial;
                var orb  = _orbsPerDialSize.TryGetValue(dial, out var o) ? o : 1.5;

                for (var a = 0; a < n; a++)
                {
                    if (longitudes[a] is not { } lonA) continue;
                    var ra = lonA % d;
                    for (var b = a + 1; b < n; b++)
                    {
                        if (longitudes[b] is not { } lonB) continue;
                        var rb   = lonB % d;
                        var mid1 = ((ra + rb) / 2) % d;
                        var mid2 = (mid1 + d / 2) % d;
                        for (var c = 0; c < n; c++)
                        {
                            if (longitudes[c] is not { } lonC) continue;
                            var rc    = lonC % d;
                            var diff1 = Math.Min(Math.Abs(rc - mid1), d - Math.Abs(rc - mid1));
                            var diff2 = Math.Min(Math.Abs(rc - mid2), d - Math.Abs(rc - mid2));
                            if (Math.Min(diff1, diff2) <= orb)
                            {
                                var key = new FourKey(a, b, c, di);
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
        }

        var counts = new List<MidpointCount>();
        foreach (var (key, dc) in dataCounts)
        {
            controlCounts.TryGetValue(key, out var cc);
            counts.Add(new MidpointCount(layouts[key.A].Factor, layouts[key.B].Factor,
                                         layouts[key.C].Factor, _dialSizes[key.D], dc, cc));
        }
        foreach (var (key, cc) in controlCounts)
        {
            if (!dataCounts.ContainsKey(key))
                counts.Add(new MidpointCount(layouts[key.A].Factor, layouts[key.B].Factor,
                                             layouts[key.C].Factor, _dialSizes[key.D], 0, cc));
        }
        counts.Sort((x, y) =>
        {
            var cmp = ((int)x.FactorA).CompareTo((int)y.FactorA);
            if (cmp != 0) return cmp;
            cmp = ((int)x.FactorB).CompareTo((int)y.FactorB);
            return cmp != 0 ? cmp : ((int)x.Occupant).CompareTo((int)y.Occupant);
        });
        return new MidpointsResult(counts, skipped);
    }

    private readonly record struct FourKey(int A, int B, int C, int D);
}
