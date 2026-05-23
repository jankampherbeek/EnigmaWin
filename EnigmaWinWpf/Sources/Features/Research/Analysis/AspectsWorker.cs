// AspectsWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of a specific aspect between two factors, split by data group.</summary>
public sealed record AspectCount(
    Factors Factor1,
    Factors Factor2,
    double AspectAngle,
    int DataCount,
    int ControlCount);

/// <summary>Full result of an AspectsWorker run.</summary>
public sealed record AspectsResult(IReadOnlyList<AspectCount> Counts, int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum AspectsWorkerError { EclipticalNotEnabled, FileNotMapped, NoAspectsConfigured }

public sealed class AspectsWorkerException(AspectsWorkerError error)
    : Exception($"AspectsWorker error: {error}") { public AspectsWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>Reads results.bin and counts how many times each enabled factor pair forms
/// each enabled aspect, separately for real-data and control-group records.
/// An aspect is detected when the shortest arc between two longitudes differs from
/// the aspect angle by no more than orb degrees.</summary>
public sealed class AspectsWorker
{
    private readonly ResultsBinaryFile _binaryFile;
    private readonly ResearchConfig    _config;
    private readonly IReadOnlyList<double> _aspectAngles;
    private readonly double _orb;

    public AspectsWorker(ResultsBinaryFile binaryFile, ResearchConfig config,
                         IReadOnlyList<double> aspectAngles, double orb)
    {
        _binaryFile   = binaryFile;
        _config       = config;
        _aspectAngles = aspectAngles;
        _orb          = orb;
    }

    public AspectsResult Run()
    {
        if (!_config.UseEcliptical)   throw new AspectsWorkerException(AspectsWorkerError.EclipticalNotEnabled);
        if (_aspectAngles.Count == 0) throw new AspectsWorkerException(AspectsWorkerError.NoAspectsConfigured);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;
        var m = _aspectAngles.Count;

        // dataCounts[i][j][k] = count for factor-i × factor-j × aspect-k
        var dataCounts    = new int[n][][];
        var controlCounts = new int[n][][];
        for (var i = 0; i < n; i++)
        {
            dataCounts[i]    = new int[n][];
            controlCounts[i] = new int[n][];
            for (var j = 0; j < n; j++)
            {
                dataCounts[i][j]    = new int[m];
                controlCounts[i][j] = new int[m];
            }
        }
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
                for (var j = i + 1; j < n; j++)
                {
                    if (longitudes[j] is not { } lonJ) continue;
                    var arc = ShortestArc(lonI, lonJ);
                    for (var k = 0; k < m; k++)
                    {
                        if (Math.Abs(arc - _aspectAngles[k]) <= _orb)
                        {
                            if (isData) dataCounts[i][j][k]++;
                            else        controlCounts[i][j][k]++;
                        }
                    }
                }
            }
        }

        var counts = new List<AspectCount>();
        for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
                for (var k = 0; k < m; k++)
                {
                    var dc = dataCounts[i][j][k];
                    var cc = controlCounts[i][j][k];
                    if (dc > 0 || cc > 0)
                        counts.Add(new AspectCount(layouts[i].Factor, layouts[j].Factor,
                                                   _aspectAngles[k], dc, cc));
                }

        return new AspectsResult(counts, skipped);
    }

    private static double ShortestArc(double a, double b)
    {
        var diff = Math.Abs(a - b) % 360;
        return diff > 180 ? 360 - diff : diff;
    }
}
