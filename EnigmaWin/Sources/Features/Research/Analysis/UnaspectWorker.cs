// UnaspectWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of unaspected occurrences for one factor, split by data group.</summary>
public sealed record UnaspectCount(Factors Factor, int DataCount, int ControlCount);

/// <summary>Full result of an UnaspectWorker run.</summary>
public sealed record UnaspectResult(IReadOnlyList<UnaspectCount> Counts, int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum UnaspectWorkerError { EclipticalNotEnabled, FileNotMapped, NoAspectsConfigured }

public sealed class UnaspectWorkerException(UnaspectWorkerError error)
    : Exception($"UnaspectWorker error: {error}") { public UnaspectWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts how many times each enabled factor is unaspected —
/// i.e. forms no aspect (within orb) with any other enabled factor — separately for
/// real-data and control-group records.</summary>
public sealed class UnaspectWorker
{
    private readonly ResultsBinaryFile    _binaryFile;
    private readonly ResearchConfig       _config;
    private readonly IReadOnlyList<double> _aspectAngles;
    private readonly double _orb;

    public UnaspectWorker(ResultsBinaryFile binaryFile, ResearchConfig config,
                          IReadOnlyList<double> aspectAngles, double orb)
    {
        _binaryFile   = binaryFile;
        _config       = config;
        _aspectAngles = aspectAngles;
        _orb          = orb;
    }

    public UnaspectResult Run()
    {
        if (!_config.UseEcliptical)   throw new UnaspectWorkerException(UnaspectWorkerError.EclipticalNotEnabled);
        if (_aspectAngles.Count == 0) throw new UnaspectWorkerException(UnaspectWorkerError.NoAspectsConfigured);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;

        var dataCounts    = new int[n];
        var controlCounts = new int[n];
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

            for (var i = 0; i < n; i++)
            {
                if (longitudes[i] is not { } lonI) continue;

                var isUnaspected = true;
                for (var j = 0; j < n && isUnaspected; j++)
                {
                    if (j == i || longitudes[j] is not { } lonJ) continue;
                    var arc = ShortestArc(lonI, lonJ);
                    foreach (var angle in _aspectAngles)
                    {
                        if (Math.Abs(arc - angle) <= _orb) { isUnaspected = false; break; }
                    }
                }

                if (isUnaspected)
                {
                    if (isData) dataCounts[i]++;
                    else        controlCounts[i]++;
                }
            }
        }

        var counts = new List<UnaspectCount>(n);
        for (var fi = 0; fi < n; fi++)
            counts.Add(new UnaspectCount(layouts[fi].Factor, dataCounts[fi], controlCounts[fi]));

        return new UnaspectResult(counts, skipped);
    }

    private static double ShortestArc(double a, double b)
    {
        var diff = Math.Abs(a - b) % 360;
        return diff > 180 ? 360 - diff : diff;
    }
}
