// HarmonicsWorker.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Research.Pipeline;

namespace EnigmaWin.Sources.Features.Research.Analysis;

// ── Result types ──────────────────────────────────────────────────────────────

/// <summary>Count of a harmonic conjunction: radix factor A's harmonic position conjuncts radix factor B.</summary>
public sealed record HarmonicConjunctionCount(
    Factors HarmonicFactor,
    Factors RadixFactor,
    int HarmonicNumber,
    int DataCount,
    int ControlCount);

/// <summary>Full result of a HarmonicsWorker run.</summary>
public sealed record HarmonicsResult(
    IReadOnlyList<HarmonicConjunctionCount> Counts,
    int HarmonicNumber,
    int SkippedRecords);

// ── Errors ────────────────────────────────────────────────────────────────────

public enum HarmonicsWorkerError { EclipticalNotEnabled, FileNotMapped }

public sealed class HarmonicsWorkerException(HarmonicsWorkerError error)
    : Exception($"HarmonicsWorker error: {error}") { public HarmonicsWorkerError Error => error; }

// ── Worker ────────────────────────────────────────────────────────────────────

/// <summary>For each record, multiplies every factor's longitude by harmonicNumber (mod 360)
/// and checks if the result is within orb of any other factor's radix longitude.
/// Counts matches per (harmonicFactor, radixFactor) pair.</summary>
public sealed class HarmonicsWorker
{
    private readonly ResultsBinaryFile _binaryFile;
    private readonly ResearchConfig    _config;
    private readonly int    _harmonicNumber;
    private readonly double _orb;

    public HarmonicsWorker(ResultsBinaryFile binaryFile, ResearchConfig config,
                           int harmonicNumber, double orb)
    {
        _binaryFile     = binaryFile;
        _config         = config;
        _harmonicNumber = harmonicNumber;
        _orb            = orb;
    }

    public HarmonicsResult Run()
    {
        if (!_config.UseEcliptical) throw new HarmonicsWorkerException(HarmonicsWorkerError.EclipticalNotEnabled);

        var layouts     = _config.FactorLayouts;
        var recordCount = (int)_binaryFile.RecordCount;
        var n = layouts.Count;
        var h = (double)_harmonicNumber;

        var dataCounts    = new int[n][];
        var controlCounts = new int[n][];
        for (var i = 0; i < n; i++) { dataCounts[i] = new int[n]; controlCounts[i] = new int[n]; }
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
                var harmonicPos = (lonI * h) % 360;
                for (var j = 0; j < n; j++)
                {
                    if (j == i || longitudes[j] is not { } lonJ) continue;
                    var diff = Math.Abs(harmonicPos - lonJ) % 360;
                    var arc  = diff > 180 ? 360 - diff : diff;
                    if (arc <= _orb)
                    {
                        if (isData) dataCounts[i][j]++;
                        else        controlCounts[i][j]++;
                    }
                }
            }
        }

        var counts = new List<HarmonicConjunctionCount>();
        for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
            {
                if (j == i) continue;
                var dc = dataCounts[i][j];
                var cc = controlCounts[i][j];
                if (dc > 0 || cc > 0)
                    counts.Add(new HarmonicConjunctionCount(
                        layouts[i].Factor, layouts[j].Factor, _harmonicNumber, dc, cc));
            }

        return new HarmonicsResult(counts, _harmonicNumber, skipped);
    }
}
